//------------------------------------------------------------------------------
// Contributor(s): richard-cruz@hotmail.com 3/20/2018, WA
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Plugin.Shipping.Ontrac.Domain;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;

namespace Nop.Plugin.Shipping.Ontrac
{
    /// <summary>
    /// Ontrac computation method
    /// </summary>
    public class OntracComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Constants

        private const int MAXPACKAGEWEIGHT = 150;
        private const string MEASUREWEIGHTSYSTEMKEYWORD = "lb";
        private const string MEASUREDIMENSIONSYSTEMKEYWORD = "inches";

        #endregion

        #region Fields

        private readonly IMeasureService _measureService;
        private readonly IShippingService _shippingService;
        private readonly ISettingService _settingService;
        private readonly OntracSettings _ontracSettings;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;

        private readonly StringBuilder _traceMessages;

        #endregion

        #region Ctor

        public OntracComputationMethod(IMeasureService measureService,
            IShippingService shippingService,
            ISettingService settingService,
            OntracSettings ontracSettings, 
            ICountryService countryService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IOrderTotalCalculationService orderTotalCalculationService,
            ILogger logger,
            ILocalizationService localizationService,
            IWebHelper webHelper)
        {
            this._measureService = measureService;
            this._shippingService = shippingService;
            this._settingService = settingService;
            this._ontracSettings = ontracSettings;
            this._countryService = countryService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._logger = logger;
            this._localizationService = localizationService;
            this._webHelper = webHelper;

            this._traceMessages = new StringBuilder();
        }

        #endregion

        #region Utilities

        private string CreateRequest(string accessKey, string username, string password,
            GetShippingOptionRequest getShippingOptionRequest, OntracCustomerClassification customerClassification,
            OntracPickupType pickupType, OntracPackagingType packagingType, bool saturdayDelivery)
        {
            var zipPostalCodeFrom = getShippingOptionRequest.ZipPostalCodeFrom;
            var zipPostalCodeTo = getShippingOptionRequest.ShippingAddress.ZipPostalCode;
            var countryCodeFrom = getShippingOptionRequest.CountryFrom.TwoLetterIsoCode;
            var countryCodeTo = getShippingOptionRequest.ShippingAddress.Country.TwoLetterIsoCode;
            var stateCodeFrom = getShippingOptionRequest.StateProvinceFrom?.Abbreviation;
            var stateCodeTo = getShippingOptionRequest.ShippingAddress.StateProvince?.Abbreviation;

             
            _orderTotalCalculationService.GetShoppingCartSubTotal(getShippingOptionRequest.Items.Select(x => x.ShoppingCartItem).ToList(),
              false, out decimal _, out List<DiscountForCaching> _, out decimal orderSubTotal, out decimal _);


            // Rate request setup - Total Dimensions of Shopping Cart Items determines number of packages

            var usedMeasureWeight = GetUsedMeasureWeight();
            var usedMeasureDimension = GetUsedMeasureDimension();

            _shippingService.GetDimensions(getShippingOptionRequest.Items, out decimal widthTmp, out decimal lengthTmp, out decimal heightTmp, true);

            var length = ConvertFromPrimaryMeasureDimension(lengthTmp, usedMeasureDimension);
            var height = ConvertFromPrimaryMeasureDimension(heightTmp, usedMeasureDimension);
            var width = ConvertFromPrimaryMeasureDimension(widthTmp, usedMeasureDimension);
            var weight = ConvertFromPrimaryMeasureWeight(_shippingService.GetTotalWeight(getShippingOptionRequest, ignoreFreeShippedItems: true), usedMeasureWeight);
            if (length < 1)
                length = 1;
            if (height < 1)
                height = 1;
            if (width < 1)
                width = 1;
            if (weight < 1)
                weight = 1;

            var totalPackagesDims = 1;
            var totalPackagesWeights = 1;
            if (IsPackageTooHeavy(weight))
            {
                totalPackagesWeights = Convert.ToInt32(Math.Ceiling((decimal)weight / (decimal)MAXPACKAGEWEIGHT));
            }
            if (IsPackageTooLarge(length, height, width))
            {
                totalPackagesDims = Convert.ToInt32(Math.Ceiling((decimal)TotalPackageSize(length, height, width) / (decimal)108));
            }
            var totalPackages = totalPackagesDims > totalPackagesWeights ? totalPackagesDims : totalPackagesWeights;
            if (totalPackages == 0)
                totalPackages = 1;


            //The maximum declared amount per package: 50000 USD.
            var InsureCost = _ontracSettings.InsurePackage ? Convert.ToInt32(orderSubTotal / totalPackages) : 0;

            //var Product = "S";
            var res = "true";
            var urlString = "?pw="+ password+"&packages=ID1;" + zipPostalCodeFrom + ";" + zipPostalCodeTo + ";" + 
                res + ";0.00;false;" + InsureCost + ";" + weight + ";" + length + "X" + width + "X" + height + ";" ;
             

            return urlString;
        }

   
        private string DoRequest(string url, string requestString)
        {

            url = url + requestString;

            var bytes = Encoding.ASCII.GetBytes(requestString);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Get;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }




            //request.ContentType = MimeTypes.ApplicationXWwwFormUrlencoded;
            //request.ContentLength = bytes.Length;
            //using (var requestStream = request.GetRequestStream())
            //    requestStream.Write(bytes, 0, bytes.Length);
            //using (var response = request.GetResponse())
            //{
            //    string responseXml;
            //    using (var reader = new StreamReader(response.GetResponseStream()))
            //        responseXml = reader.ReadToEnd();

            //    return responseXml;
            //}
        }

        private string GetCustomerClassificationCode(OntracCustomerClassification customerClassification)
        {
            switch (customerClassification)
            {
                case OntracCustomerClassification.Wholesale:
                    return "01";
                case OntracCustomerClassification.Occasional:
                    return "03";
                case OntracCustomerClassification.Retail:
                    return "04";
                default:
                    throw new NopException("Unknown Ontrac customer classification code");
            }
        }

        private string GetPackagingTypeCode(OntracPackagingType packagingType)
        {
            switch (packagingType)
            {
                case OntracPackagingType.Letter:
                    return "01";
                case OntracPackagingType.CustomerSuppliedPackage:
                    return "02";
                case OntracPackagingType.Tube:
                    return "03";
                case OntracPackagingType.PAK:
                    return "04";
                case OntracPackagingType.ExpressBox:
                    return "21";
                case OntracPackagingType._10KgBox:
                    return "25";
                case OntracPackagingType._25KgBox:
                    return "24";
                default:
                    throw new NopException("Unknown Ontrac packaging type code");
            }
        }

        private string GetPickupTypeCode(OntracPickupType pickupType)
        {
            switch (pickupType)
            {
                case OntracPickupType.DailyPickup:
                    return "01";
                case OntracPickupType.CustomerCounter:
                    return "03";
                case OntracPickupType.OneTimePickup:
                    return "06";
                case OntracPickupType.OnCallAir:
                    return "07";
                case OntracPickupType.SuggestedRetailRates:
                    return "11";
                case OntracPickupType.LetterCenter:
                    return "19";
                case OntracPickupType.AirServiceCenter:
                    return "20";
                default:
                    throw new NopException("Unknown Ontrac pickup type code");
            }
        }

        private string GetServiceName(string serviceId)
        {
            switch (serviceId)
            {  
                case "C":
                    return "Ground";
                case "S":
                    return "Sunrise - 10:30AM Delivery";              
                case "G":
                    return "Sunrise Gold - 8:00AM Delivery";
               
                default:
                    return "Unknown";
            }
        }

        private bool IsPackageTooLarge(int length, int height, int width)
        {
            var total = TotalPackageSize(length, height, width);
            return total > 165;
        }

        private int TotalPackageSize(int length, int height, int width)
        {
            var girth = height + height + width + width;
            var total = girth + length;
            return total;
        }

        private bool IsPackageTooHeavy(int weight)
        {
            return weight > MAXPACKAGEWEIGHT;
        }

        private MeasureWeight GetUsedMeasureWeight()
        {
            var usedMeasureWeight = _measureService.GetMeasureWeightBySystemKeyword(MEASUREWEIGHTSYSTEMKEYWORD);
            if (usedMeasureWeight == null)
                throw new NopException("Ontrac shipping service. Could not load \"{0}\" measure weight", MEASUREWEIGHTSYSTEMKEYWORD);
            return usedMeasureWeight;
        }

        private MeasureDimension GetUsedMeasureDimension()
        {
            var usedMeasureDimension = _measureService.GetMeasureDimensionBySystemKeyword(MEASUREDIMENSIONSYSTEMKEYWORD);
            if (usedMeasureDimension == null)
                throw new NopException("Ontrac shipping service. Could not load \"{0}\" measure dimension", MEASUREDIMENSIONSYSTEMKEYWORD);

            return usedMeasureDimension;
        }

        private int ConvertFromPrimaryMeasureDimension(decimal quantity, MeasureDimension usedMeasureDimension)
        {
            return Convert.ToInt32(Math.Ceiling(_measureService.ConvertFromPrimaryMeasureDimension(quantity, usedMeasureDimension)));
        }

        private int ConvertFromPrimaryMeasureWeight(decimal quantity, MeasureWeight usedMeasureWeighht)
        {
            return Convert.ToInt32(Math.Ceiling(_measureService.ConvertFromPrimaryMeasureWeight(quantity, usedMeasureWeighht)));
        }

        private IEnumerable<ShippingOption> ParseResponse(string response, bool saturdayDelivery, ref string error)
        {
            var shippingOptions = new List<ShippingOption>();

            var carrierServicesOffered = _ontracSettings.CarrierServicesOffered;

            var serviceCode = string.Empty;
            var monetaryValue = string.Empty;
            var doc = new XmlDocument();
            doc.LoadXml(response);
            try
            {
                if (doc.SelectSingleNode("/OnTracRateResponse//Error").LastChild.Value.Length > 0)
                {
                    // return "";
                    //fail
                    error = "Ontrac Error returned: " + doc.SelectSingleNode("/OnTracRateResponse//Error").LastChild.Value;
                }
            }
            catch
            {
                //nothing wrong
            }

            try
            {
                if (doc.SelectSingleNode("/OnTracRateResponse//GlobalRate").LastChild.Value.Length > 0)
                {
                    monetaryValue= doc.SelectSingleNode("/OnTracRateResponse//GlobalRate").LastChild.Value;
                     
                }
            }
            catch
            {
                //problem
              //  return "";
            }

            try
            {
                if (doc.SelectSingleNode("/OnTracRateResponse//Service").LastChild.Value.Length > 0)
                { 
                    serviceCode = doc.SelectSingleNode("/OnTracRateResponse//Service").LastChild.Value;
                }
            }
            catch
            {
                //problem
                //  return "";
            }


            var service = GetServiceName(serviceCode);
            var serviceId = $"[{serviceCode}]";

            // Go to the next rate if the service ID is not in the list of services to offer
            if (!saturdayDelivery && !string.IsNullOrEmpty(carrierServicesOffered) && !carrierServicesOffered.Contains(serviceId))
            {
                
            }
            else
            {

                //Weed out unwanted or unknown service rates
                if (service.ToUpper() != "UNKNOWN")
                {
                    var shippingOption = new ShippingOption
                    {
                        Rate = Convert.ToDecimal(monetaryValue, new CultureInfo("en-US")),
                        Name = service
                    };
                    shippingOptions.Add(shippingOption);
                }
            }

            return shippingOptions;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Represents a response of getting shipping rate options</returns>
        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException(nameof(getShippingOptionRequest));

            var response = new GetShippingOptionResponse();

            if (getShippingOptionRequest.Items == null)
            {
                response.AddError("No shipment items");
                return response;
            }

            if (getShippingOptionRequest.ShippingAddress == null)
            {
                response.AddError("Shipping address is not set");
                return response;
            }

            if (getShippingOptionRequest.ShippingAddress.Country == null)
            {
                response.AddError("Shipping country is not set");
                return response;
            }

            if (getShippingOptionRequest.CountryFrom == null)
            {
                getShippingOptionRequest.CountryFrom = _countryService.GetAllCountries().FirstOrDefault();
            }

            try
            {
                var requestString = CreateRequest("","", _ontracSettings.Password, getShippingOptionRequest,
                  _ontracSettings.CustomerClassification, _ontracSettings.PickupType, _ontracSettings.PackagingType, false);

                var responseXml = DoRequest(_ontracSettings.Url, requestString+"C");

                var error = "";
                var shippingOptions = ParseResponse(responseXml, false, ref error);
                if (string.IsNullOrEmpty(error))
                {
                    foreach (var shippingOption in shippingOptions)
                    {
                        //if (!shippingOption.Name.ToLower().StartsWith("ontrac"))
                        //    shippingOption.Name = "Ontrac {shippingOption.Name}";
                        
                        shippingOption.Rate += _ontracSettings.AdditionalHandlingCharge;
                        response.ShippingOptions.Add(shippingOption);
                    }
                }
                else
                {
                    response.AddError(error);
                }

                  responseXml = DoRequest(_ontracSettings.Url, requestString + "G");

                  error = "";
                  shippingOptions = ParseResponse(responseXml, false, ref error);
                if (string.IsNullOrEmpty(error))
                {
                    foreach (var shippingOption in shippingOptions)
                    {
                        //if (!shippingOption.Name.ToLower().StartsWith("ontrac"))
                        //    shippingOption.Name = $"Ontrac {shippingOption.Name}";
                        shippingOption.Rate += _ontracSettings.AdditionalHandlingCharge;
                        response.ShippingOptions.Add(shippingOption);
                    }
                }
                else
                {
                    response.AddError(error);
                }

                  responseXml = DoRequest(_ontracSettings.Url, requestString + "S");

                  error = "";
                  shippingOptions = ParseResponse(responseXml, false, ref error);
                if (string.IsNullOrEmpty(error))
                {
                    foreach (var shippingOption in shippingOptions)
                    {
                        //if (!shippingOption.Name.ToLower().StartsWith("ontrac"))
                        //    shippingOption.Name = $"Ontrac {shippingOption.Name}";
                        shippingOption.Rate += _ontracSettings.AdditionalHandlingCharge;
                        response.ShippingOptions.Add(shippingOption);
                    }
                }
                else
                {
                    response.AddError(error);
                }



                if (response.ShippingOptions.Any())
                    response.Errors.Clear();
            }
            catch (Exception exc)
            {
                response.AddError($"Ontrac Service is currently unavailable, try again later. {exc.Message}");
            }
            finally
            {
                
            }

            return response;
        }

        /// <summary>
        /// Gets fixed shipping rate (if shipping rate computation method allows it and the rate can be calculated before checkout).
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Fixed shipping rate; or null in case there's no fixed shipping rate</returns>
        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return null;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/ShippingOntrac/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new OntracSettings
            {
                Url = "https://www.shipontrac.net/OnTracWebServices/OnTracServices.svc/V1/175512/rates",
                CustomerClassification = OntracCustomerClassification.Retail,
                PickupType = OntracPickupType.OneTimePickup,
                PackagingType = OntracPackagingType.ExpressBox,
                PackingPackageVolume = 5184,
                PackingType = PackingType.PackByDimensions,
                PassDimensions = true,
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.Url", "URL");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.Url.Hint", "Specify Ontrac URL.");
              this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.Password", "Password");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.Password.Hint", "Specify Ontrac password.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.AdditionalHandlingCharge", "Additional handling charge");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.AdditionalHandlingCharge.Hint", "Enter additional handling fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.InsurePackage", "Insure package");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.InsurePackage.Hint", "Check to insure packages.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.CustomerClassification", "Ontrac Customer Classification");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.CustomerClassification.Hint", "Choose customer classification.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PickupType", "Ontrac Pickup Type");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PickupType.Hint", "Choose Ontrac pickup type.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PackagingType", "Ontrac Packaging Type");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PackagingType.Hint", "Choose Ontrac packaging type.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.AvailableCarrierServices", "Carrier Services");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.AvailableCarrierServices.Hint", "Select the services you want to offer to customers.");
            //tracker events
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Tracker.Departed", "Departed");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Tracker.ExportScanned", "Export scanned");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Tracker.OriginScanned", "Origin scanned");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Tracker.Arrived", "Arrived");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Tracker.NotDelivered", "Not delivered");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Tracker.Booked", "Booked");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Tracker.Delivered", "Delivered");
            //packing
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PassDimensions", "Pass dimensions");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PassDimensions.Hint", "Check if you want to pass package dimensions when requesting rates.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PackingType", "Packing type");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PackingType.Hint", "Choose preferred packing type.");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Shipping.Ontrac.PackingType.PackByDimensions", "Pack by dimensions");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Shipping.Ontrac.PackingType.PackByOneItemPerPackage", "Pack by one item per package");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Shipping.Ontrac.PackingType.PackByVolume", "Pack by volume");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PackingPackageVolume", "Package volume");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PackingPackageVolume.Hint", "Enter your package volume.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.Tracing", "Tracing");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.Tracing.Hint", "Check if you want to record plugin tracing in System Log. Warning: The entire request and response XML will be logged (including AccessKey/UserName,Password). Do not leave this enabled in a production environment.");

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<OntracSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.Url");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.Url.Hint"); 
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.Password");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.Password.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.AdditionalHandlingCharge");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.AdditionalHandlingCharge.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.InsurePackage");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.InsurePackage.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.CustomerClassification");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.CustomerClassification.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PickupType");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PickupType.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PackagingType");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PackagingType.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.AvailableCarrierServices");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.AvailableCarrierServices.Hint");
            //tracker events
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Tracker.Departed");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Tracker.ExportScanned");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Tracker.OriginScanned");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Tracker.Arrived");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Tracker.NotDelivered");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Tracker.Booked");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Tracker.Delivered");
            //packing
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PassDimensions");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PassDimensions.Hint");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Shipping.Ontrac.PackingType.PackByDimensions");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Shipping.Ontrac.PackingType.PackByOneItemPerPackage");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Shipping.Ontrac.PackingType.PackByVolume");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PackingType");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PackingType.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PackingPackageVolume");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.PackingPackageVolume.Hint");
            //tracing
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.Tracing");
            this.DeletePluginLocaleResource("Plugins.Shipping.Ontrac.Fields.Tracing.Hint");

            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a shipping rate computation method type
        /// </summary>
        public ShippingRateComputationMethodType ShippingRateComputationMethodType
        {
            get { return ShippingRateComputationMethodType.Realtime; }
        }

        /// <summary>
        /// Gets a shipment tracker
        /// </summary>
        public IShipmentTracker ShipmentTracker
        {
            get { return null; }
        }

        #endregion
    }
}
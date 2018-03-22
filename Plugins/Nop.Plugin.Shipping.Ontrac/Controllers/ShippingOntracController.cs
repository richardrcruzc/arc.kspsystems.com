using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Shipping.Ontrac.Domain;
using Nop.Plugin.Shipping.Ontrac.Models;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Shipping.Ontrac.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class ShippingOntracController : BasePluginController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly OntracSettings _ontracSettings;

        #endregion

        #region Ctor

        public ShippingOntracController(ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            OntracSettings ontracSettings)
        {
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._settingService = settingService;
            this._ontracSettings = ontracSettings;
        }

        #endregion

        #region Methods

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var model = new OntracShippingModel
            {
                Url = _ontracSettings.Url,
               
                Password = _ontracSettings.Password,
                AdditionalHandlingCharge = _ontracSettings.AdditionalHandlingCharge,
                InsurePackage = _ontracSettings.InsurePackage,
                PackingPackageVolume = _ontracSettings.PackingPackageVolume,
                PackingType = (int)_ontracSettings.PackingType,
                PackingTypeValues = _ontracSettings.PackingType.ToSelectList(),
                PassDimensions = _ontracSettings.PassDimensions
            };
            foreach (OntracCustomerClassification customerClassification in Enum.GetValues(typeof(OntracCustomerClassification)))
            {
                model.AvailableCustomerClassifications.Add(new SelectListItem
                {
                    Text = CommonHelper.ConvertEnum(customerClassification.ToString()),
                    Value = customerClassification.ToString(),
                    Selected = customerClassification == _ontracSettings.CustomerClassification
                });
            }
            foreach (OntracPickupType pickupType in Enum.GetValues(typeof(OntracPickupType)))
            {
                model.AvailablePickupTypes.Add(new SelectListItem
                {
                    Text = CommonHelper.ConvertEnum(pickupType.ToString()),
                    Value = pickupType.ToString(),
                    Selected = pickupType == _ontracSettings.PickupType
                });
            }
            foreach (OntracPackagingType packagingType in Enum.GetValues(typeof(OntracPackagingType)))
            {
                model.AvailablePackagingTypes.Add(new SelectListItem
                {
                    Text = CommonHelper.ConvertEnum(packagingType.ToString()),
                    Value = packagingType.ToString(),
                    Selected = packagingType == _ontracSettings.PackagingType
                });
            }

            // Load Domestic service names
            var carrierServicesOfferedDomestic = _ontracSettings.CarrierServicesOffered;
            foreach (var service in OntracServices.Services)
                model.AvailableCarrierServices.Add(service);

            if (!string.IsNullOrEmpty(carrierServicesOfferedDomestic))
                foreach (var service in OntracServices.Services)
                {
                    var serviceId = OntracServices.GetServiceId(service);
                    if (!string.IsNullOrEmpty(serviceId))
                    {
                        // Add delimiters [] so that single digit IDs aren't found in multi-digit IDs
                        if (carrierServicesOfferedDomestic.Contains($"[{serviceId}]"))
                            model.CarrierServicesOffered.Add(service);
                    }
                }

            return View("~/Plugins/Shipping.Ontrac/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Configure(OntracShippingModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _ontracSettings.Url = model.Url;
      
            _ontracSettings.Password = model.Password;
            _ontracSettings.AdditionalHandlingCharge = model.AdditionalHandlingCharge;
            _ontracSettings.InsurePackage = model.InsurePackage;
            _ontracSettings.CustomerClassification = (OntracCustomerClassification)Enum.Parse(typeof(OntracCustomerClassification), model.CustomerClassification);
            _ontracSettings.PickupType = (OntracPickupType)Enum.Parse(typeof(OntracPickupType), model.PickupType);
            _ontracSettings.PackagingType = (OntracPackagingType)Enum.Parse(typeof(OntracPackagingType), model.PackagingType);
            _ontracSettings.PackingPackageVolume = model.PackingPackageVolume;
            _ontracSettings.PackingType = (PackingType)model.PackingType;
            _ontracSettings.PassDimensions = model.PassDimensions;
          


            // Save selected services
            var carrierServicesOfferedDomestic = new StringBuilder();
            var carrierServicesDomesticSelectedCount = 0;
            if (model.CheckedCarrierServices != null)
            {
                foreach (var cs in model.CheckedCarrierServices)
                {
                    carrierServicesDomesticSelectedCount++;
                    var serviceId = OntracServices.GetServiceId(cs);
                    if (!string.IsNullOrEmpty(serviceId))
                    {
                        // Add delimiters [] so that single digit IDs aren't found in multi-digit IDs
                        carrierServicesOfferedDomestic.AppendFormat("[{0}]:", serviceId);
                    }
                }
            }
            // Add default options if no services were selected
            if (carrierServicesDomesticSelectedCount == 0)
                _ontracSettings.CarrierServicesOffered = "[03]:[12]:[11]:[08]:";
            else
                _ontracSettings.CarrierServicesOffered = carrierServicesOfferedDomestic.ToString();

            _settingService.SaveSetting(_ontracSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        #endregion
    }
}
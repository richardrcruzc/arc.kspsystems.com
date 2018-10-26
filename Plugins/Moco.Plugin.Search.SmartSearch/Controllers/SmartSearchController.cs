using System;
using System.Collections.Generic;
using System.Net;
 
using System.Collections.Specialized;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Media;

namespace Moco.Plugin.Search.SmartSearch.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Models;
    using Service;

    public class SmartSearchController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly CatalogSettings _catalogSettings;
		private readonly SmartSearchSettings _smartSearchSettings;
		private readonly ISettingService _settingService;
		private readonly SmartSearchService _smartSearchService;
        private readonly IPermissionService _permissionService;
		private readonly ILogger _logger;

        public SmartSearchController(IProductService productService, 
            ICategoryService categoryService, 
            IRepository<Order> orderRepository, 
            IPictureService pictureService, 
            SmartSearchSettings smartSearchSettings, 
            ISettingService settingService, 
            ILogger logger, 
            ICacheManager cacheManager, 
            ILocalizationService localizationService, 
            MediaSettings mediaSettings,
            IWorkContext workContext,
            IStoreContext storeContext,
            IWebHelper webHelper,
            CatalogSettings catalogSettings,
            IPermissionService permissionService,
            IPriceFormatter priceFormatter)
        {
            _workContext = workContext;
            _localizationService = localizationService;
            _priceFormatter = priceFormatter;
            _catalogSettings = catalogSettings;
            _permissionService = permissionService;

			_smartSearchSettings = smartSearchSettings;
			_settingService = settingService;
			_logger = logger;
            _smartSearchService = new SmartSearchService(productService, categoryService, orderRepository,
                pictureService, _smartSearchSettings, _logger, cacheManager, _localizationService, mediaSettings,
                _workContext, storeContext, webHelper);
        }

		public ActionResult Index()
		{
			return Content("");
		}

        public ActionResult Search(string q, int selectedPagingField = 6, int selectedPage = 1, string selectedSortField = "Relevance", string viewMode = "grid")
		{
			ProductSearchModel model = new ProductSearchModel();
			model.q = q;
			model.SelectedSortField = selectedSortField;
			model.SelectedPagingField = selectedPagingField;
			model.SelectedPage = selectedPage;
		    
			_smartSearchService.RetrieveSearchResults(model);
            
		    foreach (var hit in model.Products.Hit)
		    {
		        hit.Document.PriceDisplay = _priceFormatter.FormatPrice(hit.Document.Price);
		        hit.Document.MinimumPriceDisplay = _priceFormatter.FormatPrice(hit.Document.MinimumPrice);
		        hit.Document.SpecialPriceDisplay = _priceFormatter.FormatPrice(hit.Document.SpecialPrice);

		        if (!_smartSearchSettings.IncludeProductImage)
		        {
		            PictureModel defaultPictureModel = _smartSearchService.GetDefaultPictureModel(hit.Document.ProductId, hit.Document.Name);
		            hit.Document.IconImageUrl = defaultPictureModel.ImageUrl;
		            hit.Document.ImageUrl = defaultPictureModel.FullSizeImageUrl;
                }
		    }

		    PreparePageSizeOptions(model, _catalogSettings.SearchPagePageSizeOptions);
            PrepareSortingOptions(model);

            NameValueCollection qscoll = new NameValueCollection(5)
            {
                { "q", q },
                { "selectedPagingField", selectedPagingField.ToString() },
                { "selectedPage", selectedPage.ToString() },
                { "selectedSortField", selectedSortField },
                { "viewMode", viewMode }
            };

            qscoll = StripDefaults(qscoll);

			ViewBag.ViewMode = viewMode;
			ViewBag.QueryStringForPaging = ToQueryString(StripSelected(qscoll, "selectedPage"));//GetQueryStringForPaging();
			ViewBag.QueryStringForLayout = ToQueryString(StripSelected(qscoll, "viewMode"));    //GetQueryStringForLayout();

		    model.DisableBuyButton = !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart) ||
		                             !_permissionService.Authorize(StandardPermissionProvider.DisplayPrices);
		    model.DisableWishlistButton = !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) ||
		                                  !_permissionService.Authorize(StandardPermissionProvider.DisplayPrices);
		    model.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;

            return View("~/Plugins/Moco.SmartSearch/Views/SmartSearch/SearchView.cshtml", model);
		}

        private NameValueCollection StripSelected(NameValueCollection nvc, string key)
		{
			var returnValue = new NameValueCollection(nvc);

			if (null != returnValue.Get(key))
			{
				returnValue.Remove(key);
			}

			return returnValue;
		}

		private NameValueCollection StripDefaults(NameValueCollection nvc)
		{
			var returnValue = new NameValueCollection(nvc);

		    string[] values = returnValue.GetValues("selectedPagingField");
            if (null != values && values.Length > 0 &&  "10".Equals(values[0], StringComparison.InvariantCultureIgnoreCase))
			{
				returnValue.Remove("selectedPagingField");
			}

		    values = returnValue.GetValues("selectedPage");
		    if (null != values && values.Length > 0 && "1".Equals(values[0], StringComparison.InvariantCultureIgnoreCase))
			{
				returnValue.Remove("selectedPage");
			}

		    values = returnValue.GetValues("selectedSortField");
		    if (null != values && values.Length > 0 && "Relevance".Equals(values[0], StringComparison.InvariantCultureIgnoreCase))
			{
				returnValue.Remove("selectedSortField");
			}

		    values = returnValue.GetValues("viewMode");
		    if (null != values && values.Length > 0 && "grid".Equals(values[0], StringComparison.InvariantCultureIgnoreCase))
			{
				returnValue.Remove("viewMode");
			}

			return returnValue;
		}

		private string ToQueryString(NameValueCollection nvc)
		{
			StringBuilder sb = new StringBuilder();

			//TODO: Url Encoding
			foreach (var key in nvc.AllKeys)
			{
				sb.AppendFormat("{0}={1}&", key, nvc[key]);
			}

			return sb.Remove(sb.Length - 1, 1).ToString();
		}

		[HttpPost]
		public ActionResult Search(ProductSearchModel model)
		{
			if (ModelState.IsValid)
			{
				_smartSearchService.RetrieveSearchResults(model);

			    foreach (var hit in model.Products.Hit)
			    {
			        hit.Document.PriceDisplay = _priceFormatter.FormatPrice(hit.Document.Price);
			        hit.Document.MinimumPriceDisplay = _priceFormatter.FormatPrice(hit.Document.MinimumPrice);
			        hit.Document.SpecialPriceDisplay = _priceFormatter.FormatPrice(hit.Document.SpecialPrice);

			        if (!_smartSearchSettings.IncludeProductImage)
			        {
			            PictureModel defaultPictureModel = _smartSearchService.GetDefaultPictureModel(hit.Document.ProductId, hit.Document.Name);
			            hit.Document.IconImageUrl = defaultPictureModel.ImageUrl;
			            hit.Document.ImageUrl = defaultPictureModel.FullSizeImageUrl;
			        }
			    }
			}

		    PreparePageSizeOptions(model, _catalogSettings.SearchPagePageSizeOptions);
		    PrepareSortingOptions(model);
            
		    NameValueCollection qscoll = new NameValueCollection(5);
		    qscoll.Add("q", model.q);
		    qscoll.Add("selectedPagingField", model.SelectedPagingField.ToString());
		    qscoll.Add("selectedPage", model.SelectedPage.ToString());
		    qscoll.Add("selectedSortField", model.SelectedSortField);
		    qscoll.Add("viewMode", model.ViewMode);

		    qscoll = StripDefaults(qscoll);

            ViewBag.ViewMode = model.ViewMode;
            ViewBag.QueryStringForPaging = ToQueryString(StripSelected(qscoll, "selectedPage"));
            ViewBag.QueryStringForLayout = ToQueryString(StripSelected(qscoll, "viewMode"));

            model.DisableBuyButton = !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart) ||
		                             !_permissionService.Authorize(StandardPermissionProvider.DisplayPrices);
            model.DisableWishlistButton = !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) ||
                                          !_permissionService.Authorize(StandardPermissionProvider.DisplayPrices);
            model.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;

			return View("~/Plugins/Moco.SmartSearch/Views/SmartSearch/SearchView.cshtml", model);
		}

		public ActionResult Feed(string key)
		{
			//check if key matches settings
			string expectedPsk = _smartSearchSettings.SmartSearchKey;
			if (!string.IsNullOrEmpty(expectedPsk) && expectedPsk.Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
			    HttpContext.Response.Buffer = false;
                HttpContext.Response.ContentType = "application/xml";
			    HttpContext.Response.StatusCode = 200; //HttpStatusCode.OK;

                _smartSearchService.GenerateSmartSearchFeed(HttpContext.Response.OutputStream);

                return new EmptyResult();
			}

			return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
		}

		//public ActionResult DisablePlugin(string key)
		//{
		//	string response = String.Empty;

		//	//check if key matches settings
		//	String expectedPSK = _smartSearchSettings.SmartSearchKey;
		//	if (!String.IsNullOrEmpty(expectedPSK) && expectedPSK.Equals(key, StringComparison.InvariantCultureIgnoreCase))
		//	{
		//		if(_smartSearchSettings.Enabled)
		//		{
		//			_smartSearchSettings.Enabled = false;

		//			_settingService.SaveSetting(_smartSearchSettings);

		//			//restart application
		//			var webHelper = EngineContext.Current.Resolve<IWebHelper>();
		//			webHelper.RestartAppDomain();

		//			response = String.Format("Successfully disabled plugin with PSK [{0}]", key);
		//		}
		//		else
		//		{
		//			response = "Plugin is not enabled.";
		//		}
		//	}
		//	else
		//	{
		//		response = String.Format("Unable to disable plugin. PSK [{0}] does not match!", key);
		//	}

		//	return Json(response, JsonRequestBehavior.AllowGet);
		//}

		public object PushFeedToHostedSmartSearch(bool fromConfigHttpPost = false)
		{
		    string text;
			try
			{
			    UriBuilder uriBuilder = new UriBuilder(_smartSearchSettings.HostedSSUrl)
			    {
			        Path = "/manage/noppluginfeedupload",
			        Query = string.Format("psk={0}", _smartSearchSettings.SmartSearchKey)
			    };
			    text = _smartSearchService.PushFeedToHss(uriBuilder.Uri);

			    if (_smartSearchSettings.Debug && null != _logger && _logger.IsEnabled(LogLevel.Information))
			    {
                    _logger.Information(string.Format("PushFeedToHSS Response:[{0}]",text));
			    }
			}
			catch (Exception ex)
			{
				text = "Error Indexing Catalog: Please check the SmartSearch Url entered.";
			    if (null != _logger && _logger.IsEnabled(LogLevel.Error))
			    {
			        _logger.Error(text, ex);
			    }
			}
			
			if(fromConfigHttpPost)
			{
				return text;
			}
			return Json(text );
		}

		 
		public String GenerateSmartSearchFeed()
		{
			//Get all of the products
			String feedResponse = _smartSearchService.GenerateSmartSearchFeedFile();
			return feedResponse;
		}

		 
		public String InvokeSmartSearchIndexing()
		{
			String feedResponse = _smartSearchService.InvokeSmartSearchIndexing();
			return feedResponse;
		}

		
		public String UploadSmartSearchFeedViaFTP()
		{
			String response = _smartSearchService.UploadFeed();
			return response;
		}

		
		
		public ActionResult Configure()
		{
			ConfigurationModel model = new ConfigurationModel();
			model.AppendDateToFileName = _smartSearchSettings.AppendDateToFileName;
			model.FilePath = _smartSearchSettings.FilePath;
			model.FileName = _smartSearchSettings.FileName;
			model.SmartSearchUrl = _smartSearchSettings.SmartSearchUrl;
			model.SmartSearchKey = _smartSearchSettings.SmartSearchKey;
			model.ImageSize = _smartSearchSettings.ImageSize;
			model.ImageIconSize = _smartSearchSettings.ImageIconSize;
			model.FtpAddress = _smartSearchSettings.FtpAddress;
			model.FtpPort = _smartSearchSettings.FtpPort;
			model.Username = _smartSearchSettings.Username;
			model.Password = _smartSearchSettings.Password;
			model.AutoFTP = _smartSearchSettings.AutoFTP;
			model.Enabled = _smartSearchSettings.Enabled;
			ViewBag.ProductsIndexedCount = GetIndexedProductCountFromSmartSearch();

			return View("~/Plugins/Moco.SmartSearch/Views/SmartSearch/Configure.cshtml", model);
		}

		
		
		[HttpPost]
		[FormValueRequired("save")]
		public ActionResult Configure(ConfigurationModel model)
		{
			_smartSearchSettings.AppendDateToFileName = model.AppendDateToFileName;
			_smartSearchSettings.FilePath = model.FilePath;
			_smartSearchSettings.FileName = model.FileName;
			_smartSearchSettings.ImageSize = model.ImageSize;
			_smartSearchSettings.ImageIconSize = model.ImageIconSize;
			_smartSearchSettings.FtpAddress = model.FtpAddress;
			_smartSearchSettings.FtpPort = model.FtpPort;
			_smartSearchSettings.Username = model.Username;
			_smartSearchSettings.Password = model.Password;
			_smartSearchSettings.AutoFTP = model.AutoFTP;
			_smartSearchSettings.Enabled = model.Enabled;
			ViewBag.ProductsIndexedCount = GetIndexedProductCountFromSmartSearch();

			_settingService.SaveSetting(_smartSearchSettings);
			model.SmartSearchUrl = _smartSearchSettings.SmartSearchUrl;

			//set the confirmation string
			model.Message = "Settings Saved";


			return View("~/Plugins/Moco.SmartSearch/Views/SmartSearch/Configure.cshtml", model);
		}

		[HttpPost]
		public ActionResult SaveSmartSearchUrlAndPushFeed(string url)
		{
			string response;

			if (!String.IsNullOrWhiteSpace(url))
			{
				Uri smartSearchUrl;
				bool urlIsValid = Uri.TryCreate(url, UriKind.Absolute, out smartSearchUrl);
				if (urlIsValid)
				{
					try
					{
						string smartSearchPreSharedKey = smartSearchUrl.Segments[1];
						smartSearchPreSharedKey = smartSearchPreSharedKey.TrimEnd('/');
						_smartSearchSettings.SmartSearchKey = smartSearchPreSharedKey;
						_smartSearchSettings.SmartSearchUrl = url;
						//Get the url for pushing feeds to hosted Smart Search
						string hostedSsUrl = smartSearchUrl.Scheme + Uri.SchemeDelimiter + smartSearchUrl.Host;
						_smartSearchSettings.HostedSSUrl = hostedSsUrl;
						_settingService.SaveSetting(_smartSearchSettings);

						// Only log error on HSS if SmartSearchUrl is in correct format
						try
						{
							response = (string)PushFeedToHostedSmartSearch(true);
						}
						catch (Exception ex)
						{
						    if (null != _logger && _logger.IsEnabled(LogLevel.Error))
						    {
						        _logger.Error("Error sending feed data", ex);
						    }

						    string errorMessage = string.Format("From nop plugin PSK: [{0}] ErrorMessage [{1}]", _smartSearchSettings.SmartSearchKey, ex);

							HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_smartSearchSettings.HostedSSUrl + "/manage/LogErrorMessageFromNop?psk=" + _smartSearchSettings.SmartSearchKey);

							byte[] data = Encoding.ASCII.GetBytes(errorMessage);
							request.Method = "POST";
							request.ContentType = "application/x-www-form-urlencoded";
							request.ContentLength = data.Length;

							using (var stream = request.GetRequestStream())
							{
								stream.Write(data, 0, data.Length);
							}

						    HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse();
						    using (var httpResponseStream = httpResponse.GetResponseStream())
						    {
						        if (null != httpResponseStream)
						        {
						            using (StreamReader sr = new StreamReader(httpResponseStream))
						            {
						                //var httpResponseString = 
						                sr.ReadToEnd();
						            }
						        }
						    }

						    response = "error indexing product catalog";
						}
					}
					catch (Exception)
					{
						response = "Oops! Could not save feed. Please check your URL and try again.";
					}
					
				}
				else
				{
					response = "Oops! Could not save feed. Please check your URL and try again.";
				}
			}
			else
			{
				response = "Smart Search Url cannot be empty!";	
			}

			return Json(response );
		}

		public string GetIndexedProductCountFromSmartSearch()
		{
			WebClient webClient = new WebClient();
			int indexedItemsCount = 0;
			string smartSearchUrl = _smartSearchSettings.SmartSearchUrl;

			try
			{
				string result = webClient.DownloadString(smartSearchUrl + "?SearchTerm=MATCH_ALL:1&pagesize=1&pagenumber=1");
				XDocument xDocument = XDocument.Parse(result);
				indexedItemsCount = Convert.ToInt32(xDocument.XPathSelectElement("/hits/count").Value);
			}
			catch (Exception e)
			{
			    if (null != _logger && _logger.IsEnabled(LogLevel.Error))
			    {
			        _logger.Error("Error Retrieving Products", e);
			    }
			}

			string outputMessage = indexedItemsCount > 0 ? "Ready to Enable!" : "Not Ready to Enable.";

			return outputMessage;
		}
        
        [NonAction]
        protected virtual void PreparePageSizeOptions(ProductSearchModel productSearchModel, string pageSizeOptions)
        {
            if (null == productSearchModel)
            {
                throw new ArgumentNullException(nameof(productSearchModel));
            }
            if (string.IsNullOrWhiteSpace(pageSizeOptions))
            {
                throw new ArgumentNullException(nameof(pageSizeOptions));
            }

            var pageSizes = pageSizeOptions.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (pageSizes.Any())
            {
                //Clear default list
                productSearchModel.PagingFields = new List<SelectListItem>();
                foreach (var pageSize in pageSizes)
                {
                    int temp;
                    if (!int.TryParse(pageSize, out temp))
                    {
                        continue;
                    }
                    if (temp <= 0)
                    {
                        continue;
                    }

                    productSearchModel.PagingFields.Add(new SelectListItem
                    {
                        Text = pageSize,
                        Value = pageSize
                    });
                }
            }
        }

        [NonAction]
        protected virtual void PrepareSortingOptions(ProductSearchModel productSearchModel)
        {
            if (null == productSearchModel)
                throw new ArgumentNullException(nameof(productSearchModel));

            productSearchModel.SortFields = new List<SelectListItem>();
            
            foreach (SearchSortingEnum enumValue in Enum.GetValues(typeof(SearchSortingEnum)))
            {
                
                var sortValue = enumValue.GetLocalizedEnum(_localizationService, _workContext);
                productSearchModel.SortFields.Add(new SelectListItem
                {
                    Text = sortValue,
                    Value = Enum.GetName(typeof(SearchSortingEnum), enumValue)
                });
            }
        }
        
    }
}
using System;
using System.IO;
using System.Web; 
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tasks;
using Nop.Core.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Tasks;

namespace Moco.Plugin.Search.SmartSearch
{
    using Microsoft.AspNetCore.Routing;
    using Service;

    public class SmartSearchPlugin : BasePlugin, IMiscPlugin
	{
		private readonly ISettingService _settingsService;
		private readonly IProductService _productService;
		private readonly ICategoryService _categoryService;
		private readonly IRepository<Order> _orderRepository;
		private readonly IPictureService _pictureService;
		private readonly IScheduleTaskService _scheduleTaskService;
		private readonly SmartSearchSettings _smartSearchSettings;
		private readonly ILogger _logger;
		private SmartSearchService _productFeedService;

	    private readonly ICacheManager _cacheManager;
	    private readonly ILocalizationService _localizationService;
	    private readonly MediaSettings _mediaSettings;
	    private readonly IWorkContext _workContext;
	    private readonly IStoreContext _storeContext;
	    private readonly IWebHelper _webHelper;


        public SmartSearchPlugin(ISettingService settingService
			, IProductService productService, ICategoryService categoryService
			, IRepository<Order> orderRepository, IPictureService pictureService
			, IScheduleTaskService scheduleTaskService, SmartSearchSettings smartSearchSettings
			, ILogger logger
            , ICacheManager cacheManager
            , ILocalizationService localizationService
            , MediaSettings mediaSettings
            , IWorkContext workContext
            , IStoreContext storeContext
            , IWebHelper webHelper)
		{
			_settingsService = settingService;
			_productService = productService;
			_categoryService = categoryService;
			_orderRepository = orderRepository;
			_pictureService = pictureService;
			_scheduleTaskService = scheduleTaskService;
			_smartSearchSettings = smartSearchSettings;
			_logger = logger;

		    _cacheManager = cacheManager;
		    _localizationService = localizationService;
		    _mediaSettings = mediaSettings;
		    _workContext = workContext;
		    _storeContext = storeContext;
		    _webHelper = webHelper;
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
		{
			actionName = "Configure";
			controllerName = "SmartSearch";
			routeValues = new RouteValueDictionary() { { "Namespaces", "Moco.Plugin.Search.SmartSearch.Controllers" }, { "area", null } };
		}

		private ScheduleTask FindScheduledTask()
		{
			return _scheduleTaskService.GetTaskByType("Moco.Plugin.Search.SmartSearch.SmartSearchTask, Moco.Plugin.Search.SmartSearch");
		}

		public void GenerateSmartSearchFeed()
		{
		    _productFeedService = new SmartSearchService(_productService, _categoryService, _orderRepository, _pictureService,
		        _smartSearchSettings, _logger, _cacheManager, _localizationService, _mediaSettings, _workContext, _storeContext, _webHelper);
			_productFeedService.GenerateSmartSearchFeedFile();
		}

		public void UploadFeed()
		{
			if (_productFeedService != null)
			{
				if (_smartSearchSettings.AutoFTP)
				{
					_productFeedService.UploadFeed();	
				}
			}
			else
			{
				if (_logger.IsEnabled(LogLevel.Error))
				{
					_logger.Error("Upload Feed Was Called Before The Feed Was Generated. No Data Was Uploaded");
				}
			}
		}

		public void InvokeSmartSearchIndexing()
		{
			if (_productFeedService != null)
			{
				_productFeedService.InvokeSmartSearchIndexing();
			}
			else
			{
				if (_logger.IsEnabled(LogLevel.Error))
				{
					_logger.Error("Generate Smart Search Index Was Called Before The Feed Was Generated. Smart Search Was Not Reindexed");
				}
			}
		}
        
		public override void Install()
		{
            #region Create Localization String Resources
            this.AddOrUpdatePluginLocaleResource("ShoppingCart.Options", "Options");

			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.AppendDateToFileName", "Append DateTime to FileName:");
			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.AppendDateToFileName.hint", "Appends date and time to the end of the SmartSearch feed file name");
			
			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.FilePath", "FilePath:");
			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.FilePath.hint", "SmartSearch feed file path");

			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.FileName", "FileName:");
			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.FileName.hint", "SmartSearch feed file name");

			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.SmartSearchKey", "SmartSearch Key:");
			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.SmartSearchKey.hint", "Key for SmartSearch");

			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.SmartSearchUrl", "SmartSearch URL:");
			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.SmartSearchUrl.hint", "Base url for SmartSearch. Example: http://www.smartsearch.com");

			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.ImageSize", "Image size:");
			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.ImageSize.hint", "Scaled image size for product images displayed in search results");

			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.IconImageSize", "Icon image size:");
			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.IconImageSize.hint", "Scaled image size for product icon images displayed in search results. This is also used the image size that is used in auto complete");

			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.FtpAddress", "FTP Address:");
			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.FtpAddress.hint", "The FTP address where the Smart Search feed will be sent.");

			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.FtpPort", "FTP Port:");
			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.FtpPort.hint", "The Port for the FTP address where the Smart Search feed will be sent");

			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.Username", "FTP Username:");
			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.Username.hint", "FTP account username");

			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.Password", "FTP Password:");
			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.Password.hint", "FTP account password");

			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.AutoFtp", "Enable Auto FTP:");
			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.AutoFtp.hint", "Allows the smart search feed to be sent to the smart search server during the automated task execution");

			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.Enabled", "Enabled:");
			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.Enabled.hint", "Enable or disable plugin");

			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.ConfigurationStatus", "Configuration status:");
			this.AddOrUpdatePluginLocaleResource("Plugins.Search.SmartSearch.ConfigurationStatus.hint", "Specifies if communicating with Hosted SmartSearch");

		    const string searchSortingEnum = "Enums.Moco.Plugin.Search.SmartSearch.Models.SearchSortingEnum.{0}";
            this.AddOrUpdatePluginLocaleResource(string.Format(searchSortingEnum, "Relevance"), "Relevance");
		    this.AddOrUpdatePluginLocaleResource(string.Format(searchSortingEnum, "HighestRated"), "Highest Rated");
		    this.AddOrUpdatePluginLocaleResource(string.Format(searchSortingEnum, "BestSellers"), "Best Sellers");
		    this.AddOrUpdatePluginLocaleResource(string.Format(searchSortingEnum, "NewArrivals"), "New Arrivals");
		    this.AddOrUpdatePluginLocaleResource(string.Format(searchSortingEnum, "LowestPrice"), "Lowest Price");
		    this.AddOrUpdatePluginLocaleResource(string.Format(searchSortingEnum, "HighestPrice"), "Highest Price");

            #endregion

            #region Create Settings
            var settings = new SmartSearchSettings()
				{
					AppendDateToFileName = false,
					FileName = "ssFeed.xml",
				//	FilePath = Path.Combine(HttpRuntime.AppDomainAppPath, @"Content\files\"),
					SmartSearchUrl = "",
					SmartSearchKey = String.Empty,
					ImageSize = 415,
					ImageIconSize = 150,
					FtpAddress = String.Empty,
					FtpPort = 21,
					Username = "UserName",
					Password = String.Empty,
            };
			_settingsService.SaveSetting(settings);
			#endregion

			#region Install Scheduled Tasks
			/* Not Needed
			var task = FindScheduledTask();
			if (task == null)
			{
				task = new ScheduleTask
				{
					Name = "Smart Search Automation",
					//each 60 minutes
					Seconds = 3200,
					Type = "Moco.Plugin.Search.SmartSearch.SmartSearchTask, Moco.Plugin.Search.SmartSearch",
					//The task should be Enabled in the configuration view,, but for testing, it is enabled during installation
					Enabled = true,
					StopOnError = false,
				};
				_scheduleTaskService.InsertTask(task);
			}
			 * */
			#endregion

			base.Install();
		}

		public override void Uninstall()
		{
			#region Remove Installed SmartSearch String Resources
			this.DeletePluginLocaleResource("ShoppingCart.Options");
			
			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.AppendDateToFileName");
			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.AppendDateToFileName.hint");

			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.FilePath");
			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.FilePath.hint");

			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.FileName");
			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.FileName.hint");

			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.SmartSearchUrl");
			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.SmartSearchUrl.hint");

			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.SmartSearchKey");
			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.SmartSearchKey.hint");

			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.ImageSize");
			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.ImageSize.hint");

			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.IconImageSize");
			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.IconImageSize.hint");

			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.FtpAddress");
			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.FtpAddress.hint");

			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.FtpPort");
			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.FtpPort.hint");

			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.Username");
			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.Username.hint");

			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.Password");
			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.Password.hint");

			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.AutoFtp");
			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.AutoFtp.hint");

			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.Enabled");
			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.Enabled.hint");

			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.ConfigurationStatus");
			this.DeletePluginLocaleResource("Plugins.Search.SmartSearch.ConfigurationStatus.hint");

		    const string searchSortingEnum = "Enums.Moco.Plugin.Search.SmartSearch.Models.SearchSortingEnum.{0}";
		    this.DeletePluginLocaleResource(string.Format(searchSortingEnum, "Relevance"));
		    this.DeletePluginLocaleResource(string.Format(searchSortingEnum, "HighestRated"));
		    this.DeletePluginLocaleResource(string.Format(searchSortingEnum, "BestSellers"));
		    this.DeletePluginLocaleResource(string.Format(searchSortingEnum, "NewArrivals"));
		    this.DeletePluginLocaleResource(string.Format(searchSortingEnum, "LowestPrice"));
		    this.DeletePluginLocaleResource(string.Format(searchSortingEnum, "HighestPrice"));
            #endregion

            #region Remove SmartSearch Settings
            _settingsService.DeleteSetting<SmartSearchSettings>();
			#endregion

			#region Remove SmartSearch Scheduled Tasks
			var task = FindScheduledTask();
			if (task != null)
				_scheduleTaskService.DeleteTask(task);
			#endregion

			base.Uninstall();
		}
	}
}
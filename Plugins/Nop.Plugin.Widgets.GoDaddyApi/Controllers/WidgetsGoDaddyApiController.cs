using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.Widgets.GoDaddyApi.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Widgets.GoDaddyApi.Controllers
{
    [Area(AreaNames.Admin)]
    public class WidgetsGoDaddyApiController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

        public WidgetsGoDaddyApiController(IWorkContext workContext,
            IStoreService storeService,
            IPermissionService permissionService, 
            IPictureService pictureService,
            ISettingService settingService,
            ICacheManager cacheManager,
            ILocalizationService localizationService)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._permissionService = permissionService;
            this._pictureService = pictureService;
            this._settingService = settingService;
            this._localizationService = localizationService;
        }

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var GoDaddyApiSettings = _settingService.LoadSetting<GoDaddyApiSettings>(storeScope);
            var model = new ConfigurationModel
            { 
                RootPath = GoDaddyApiSettings.RootPath,
                RootPathSandBox = GoDaddyApiSettings.RootPathSandBox,
                SandBox = GoDaddyApiSettings.SandBox,
                AccessKey = GoDaddyApiSettings.AccessKey,
                SecretKey = GoDaddyApiSettings.SecretKey, 
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                 model.RootPath_OverrideForStore = _settingService.SettingExists(GoDaddyApiSettings, x => x.RootPath, storeScope);
                 model.RootPathSandBox_OverrideForStore = _settingService.SettingExists(GoDaddyApiSettings, x => x.RootPathSandBox, storeScope);
                 model.SandBox_OverrideForStore = _settingService.SettingExists(GoDaddyApiSettings, x => x.SandBox, storeScope);
                 model.AccessKey_OverrideForStore = _settingService.SettingExists(GoDaddyApiSettings, x => x.AccessKey, storeScope);
                model.SecretKey_OverrideForStore = _settingService.SettingExists(GoDaddyApiSettings, x => x.SecretKey, storeScope);
                
            }

            return View("~/Plugins/Widgets.GoDaddyApi/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var GoDaddyApiSettings = _settingService.LoadSetting<GoDaddyApiSettings>(storeScope); 

            GoDaddyApiSettings.RootPath = model.RootPath;
            GoDaddyApiSettings.RootPathSandBox = model.RootPathSandBox;
            GoDaddyApiSettings.SandBox = model.SandBox;
            GoDaddyApiSettings.AccessKey = model.AccessKey;
            GoDaddyApiSettings.SecretKey = model.SecretKey; 
           
            ///* We do not clear cache after each setting update.
            // * This behavior can increase performance because cached settings will not be cleared 
            // * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(GoDaddyApiSettings, x => x.RootPath, model.RootPath_OverrideForStore, storeScope, false);
             _settingService.SaveSettingOverridablePerStore(GoDaddyApiSettings, x => x.RootPathSandBox, model.RootPathSandBox_OverrideForStore, storeScope, false);
             _settingService.SaveSettingOverridablePerStore(GoDaddyApiSettings, x => x.SandBox, model.SandBox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(GoDaddyApiSettings, x => x.AccessKey, model.AccessKey_OverrideForStore, storeScope, false);
             _settingService.SaveSettingOverridablePerStore(GoDaddyApiSettings, x => x.SecretKey, model.SecretKey_OverrideForStore, storeScope, false);
           
            //now clear settings cache
           _settingService.ClearCache();
 

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return Configure();
        }
    }
}
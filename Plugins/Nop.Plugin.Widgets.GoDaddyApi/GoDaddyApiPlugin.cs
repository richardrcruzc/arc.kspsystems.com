using System.Collections.Generic;
using System.IO;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;

namespace Nop.Plugin.Widgets.GoDaddyApi
{
    /// <summary>
    /// PLugin
    /// </summary>
    public class GoDaddyApiPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        public GoDaddyApiPlugin(IPictureService pictureService,
            ISettingService settingService, IWebHelper webHelper)
        {
            this._pictureService = pictureService;
            this._settingService = settingService;
            this._webHelper = webHelper;
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return new List<string> { "home_page_top" };
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/WidgetsGoDaddyApi/Configure";
        }

        /// <summary>
        /// Gets a view component for displaying plugin in public store
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <param name="viewComponentName">View component name</param>
        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            viewComponentName = "WidgetsGoDaddyApi";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            
            //settings
            var settings = new GoDaddyApiSettings
            {
                  RootPathSandBox = "https://api.ote-godaddy.com/api/v1/",
                  RootPath= "https://api.godaddy.com/api/v1/",
                  SandBox =true
 
            };
            _settingService.SaveSetting(settings);
             

                this.AddOrUpdatePluginLocaleResource("Nop.Plugins.Widgets.GoDaddyApi.ConfigurationTitle", "Widgets GoDaddy Api ");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugins.Widgets.GoDaddyApi.RootPath", "Root Path");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugins.Widgets.GoDaddyApi.RootPathSandBox", "Root Path SandBox");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugins.Widgets.GoDaddyApi.SandBox", "SandBox ?");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugins.Widgets.GoDaddyApi.AccessKey", "AccessKey");
            this.AddOrUpdatePluginLocaleResource("Nop.Plugins.Widgets.GoDaddyApi.SecretKey", "SecretKey"); 
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<GoDaddyApiSettings>();

            //locales
            this.DeletePluginLocaleResource("Nop.Plugins.Widgets.GoDaddyApi.ConfigurationTitle");
            this.DeletePluginLocaleResource("Nop.Plugins.Widgets.GoDaddyApi.RootPath");
            this.DeletePluginLocaleResource("Nop.Plugins.Widgets.GoDaddyApi.RootPathSandBox");
            this.DeletePluginLocaleResource("Nop.Plugins.Widgets.GoDaddyApi.SandBox");
            this.DeletePluginLocaleResource("Nop.Plugins.Widgets.GoDaddyApi.AccessKey");
            this.DeletePluginLocaleResource("Nop.Plugins.Widgets.GoDaddyApi.SecretKey"); 

            base.Uninstall();
        }
    }
}
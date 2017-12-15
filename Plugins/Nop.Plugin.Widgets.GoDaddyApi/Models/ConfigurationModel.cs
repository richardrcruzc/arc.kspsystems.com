using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Plugin.Widgets.GoDaddyApi.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; } 

        [NopResourceDisplayName("Nop.Plugins.Widgets.GoDaddyApi.RootPath")]
        public string RootPath { get; set; }
        public bool RootPath_OverrideForStore { get; set; }
        [NopResourceDisplayName("Nop.Plugins.Widgets.GoDaddyApi.RootPathSandBox")]
        public string RootPathSandBox { get; set; }
        public bool RootPathSandBox_OverrideForStore { get; set; }
        [NopResourceDisplayName("Nop.Plugins.Widgets.GoDaddyApi.SandBox")]
        public bool SandBox { get; set; }
        public bool SandBox_OverrideForStore { get; set; }
        [NopResourceDisplayName("Nop.Plugins.Widgets.GoDaddyApi.AccessKey")]
        public string AccessKey { get; set; }
        public bool AccessKey_OverrideForStore { get; set; }
        [NopResourceDisplayName("Nop.Plugins.Widgets.GoDaddyApi.SecretKey")]
        public string SecretKey { get; set; }
        public bool SecretKey_OverrideForStore { get; set; }
         
    }
}
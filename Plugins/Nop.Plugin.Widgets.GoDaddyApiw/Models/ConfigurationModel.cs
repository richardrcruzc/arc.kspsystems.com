

using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Plugin.Widgets.GoDaddyApi.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }
        [NopResourceDisplayName(" Nop.Plugin.Widgets.GoDaddyApi.Picture")]
        public int RootPath { get; set; }
        public bool RootPath_OverrideForStore { get; set; }
        [NopResourceDisplayName(" Nop.Plugin.Widgets.GoDaddyApi.Picture")]
        public int RootPathSandBox { get; set; }
        public bool RootPathSandBox_OverrideForStore { get; set; }
        [NopResourceDisplayName(" Nop.Plugin.Widgets.GoDaddyApi.Picture")]
        public bool SandBox { get; set; }
        public bool SandBox_OverrideForStore { get; set; }
        [NopResourceDisplayName(" Nop.Plugin.Widgets.GoDaddyApi.Picture")]
        public string AccessKey { get; set; }
        public bool AccessKey_OverrideForStore { get; set; }
        [NopResourceDisplayName(" Nop.Plugin.Widgets.GoDaddyApi.Picture")]
        public string SecretKey { get; set; }
        public bool SecretKey_OverrideForStore { get; set; }


    }
}

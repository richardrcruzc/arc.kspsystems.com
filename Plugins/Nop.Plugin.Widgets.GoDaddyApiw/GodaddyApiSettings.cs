using Nop.Core.Configuration;
 

namespace Nop.Plugin.Widgets.GoDaddyApi
{
    class GodaddyApiSettings : ISettings
    {
        public int RootPath { get; set; }
        public int RootPathSandBox { get; set; }
        public bool SandBox { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }
}

using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.GoDaddyApi
{
    public class GoDaddyApiSettings : ISettings
    { 
            public string RootPath { get; set; }
            public string RootPathSandBox { get; set; }
            public bool SandBox { get; set; }
            public string AccessKey { get; set; }
            public string SecretKey { get; set; } 
    }
}
//using Microsoft.AspNetCore.Routing;
//using Nop.Web.Framework.Mvc.Routing;

//namespace Nop.Plugin.Misc.ProductWizard
//{
//    public partial class RouteProvider : IRouteProvider
//    {
//        public void RegisterRoutes(RouteCollection routes)
//        {
//            routes.MapRoute("Plugin.Misc.ProductWizard.Configure",
//                 "Plugins/GroupsWizard/Configure",
//                 new { controller = "Groups", action = "Configure" },
//                 new[] { "Nop.Plugin.Misc.ProductWizard.Controllers" }
//            );
//           // routes.MapRoute("Plugin.Misc.ProductWizard.List",
//           //     "PluginsGroupsWizard/List",
//           //     new { controller = "Groups", action = "List" },
//           //     new[] { "Nop.Plugin.Misc.ProductWizard.Controllers" }
//           //);
//        }
//        public int Priority
//        {
//            get
//            {
//                return 0;
//            }
//        }
//    }
//}

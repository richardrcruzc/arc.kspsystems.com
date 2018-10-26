using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routing;

namespace Moco.Plugin.Search.SmartSearch
{
	public class RouteProvider : IRouteProvider
    {

        //public void RegisterRoutes(RouteCollection routes)
        //{
        //	var settingService = EngineContext.Current.Resolve<ISettingService>();
        //	SmartSearchSettings smartsearchsettings = settingService.LoadSetting<SmartSearchSettings>();

        //	if (smartsearchsettings.Enabled)
        //	{
        //		routes.Remove(routes["ProductSearch"]);

        //		routes.MapLocalizedRoute("ProductSearch",
        //									"SmartSearch/search/",
        //									new { controller = "SmartSearch", action = "Search" },
        //									new[] { "Nop.Web.Controllers" });
        //	}

        //	routes.MapRoute("Plugin.Search.SmartSearch.Configure",
        //	"Plugins/SmartSearch/Configure",
        //	new { controller = "SmartSearch", action = "Configure" },
        //	new[] { "Moco.Plugin.Search.SmartSearch.Controllers" });

        //	routes.MapLocalizedRoute("ProductFeed",
        //								"SmartSearch/Feed/{key}/",
        //								new { controller = "SmartSearch", action = "Feed" },
        //								new[] { "Nop.Web.Controllers" });

        //}

        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="routeBuilder">Route builder</param>
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {

          //  routeBuilder.MapRoute("Nop.Plugin.Misc.ProductWizard.WebhookEventsHandler", "anon/BrowseInventory.aspx",
          //new { controller = "PartsForItem", action = "BrowseInventory" });

            routeBuilder.MapRoute("ProductSearch",
        									"SmartSearch/search/",
        									new { controller = "SmartSearch", action = "Search" },
        									new[] { "Nop.Web.Controllers" });


            routeBuilder.MapRoute("Plugin.Search.SmartSearch.Configure",
        	"Plugins/SmartSearch/Configure",
        	new { controller = "SmartSearch", action = "Configure" },
        	new[] { "Moco.Plugin.Search.SmartSearch.Controllers" });


            routeBuilder.MapLocalizedRoute("ProductFeed",
        								"SmartSearch/Feed/{key}/",
        								new { controller = "SmartSearch", action = "Feed" },
        								new[] { "Nop.Web.Controllers" });


        }

        public int Priority
		{
			get { return -1; }
		}
	}
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;


namespace Nop.Plugin.Misc.ProductWizard
{
    public partial class RouteProvider : IRouteProvider
{
    /// <summary>
    /// Register routes
    /// </summary>
    /// <param name="routeBuilder">Route builder</param>
    public void RegisterRoutes(IRouteBuilder routeBuilder)
    {
        routeBuilder.MapRoute("Nop.Plugin.Misc.ProductWizard.WebhookEventsHandler", "anon/BrowserInventory",
            new { controller = "PartsForItem", action = "BrowserInventory" });

    }

    /// <summary>
    /// Gets a priority of route provider
    /// </summary>
    public int Priority
    {
        get { return -1; }
    }
}
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Nop.Web.Framework.Localization;
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
        routeBuilder.MapRoute("Nop.Plugin.Misc.ProductWizard.WebhookEventsHandler", "anon/BrowseInventory.aspx",
            new { controller = "PartsForItem", action = "BrowseInventory"  });

            routeBuilder.MapLocalizedRoute(
                "Nop.Plugin.Misc.ProductWizard.Slug",
                "{productId:min(1)}/{productName:required}",
         new { controller = "Product", action = "UrlRecordSlug" });
            routeBuilder.MapLocalizedRoute(
             "Nop.Plugin.Misc.ProductWizard.Slug0",
             "{productId:min(1)}/{productName:required}-{PartNumber}",
      new { controller = "Product", action = "UrlRecordSlug" });

            routeBuilder.MapLocalizedRoute(
                "Nop.Plugin.Misc.ProductWizard.Slug1",
                "{productId:min(1)}/{productName:required}/{PartNumber}",
         new { controller = "Product", action = "UrlRecordSlug" });

            routeBuilder.MapLocalizedRoute(
              "Nop.Plugin.Misc.ProductWizard.Slug2",
              "{productId:min(1)}/{productName:required}/{PartNumber}/{PartNumber1}",
       new { controller = "Product", action = "UrlRecordSlug" });

            routeBuilder.MapLocalizedRoute(
                "Nop.Plugin.Misc.ProductWizard.Itendetails",
                "anon/itemdetails.aspx",
         new { controller = "Product", action = "Itendetails" });

            routeBuilder.MapLocalizedRoute(
           "Nop.Plugin.Misc.ProductWizard.SearchOtro",
           "Searchfilter",
    new { controller = "catalog", action = "SearchTermAutoComplete" });


           

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

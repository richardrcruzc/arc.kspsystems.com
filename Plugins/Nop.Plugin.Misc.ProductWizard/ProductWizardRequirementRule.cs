using System;
using System.Linq;
using Nop.Core.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Plugin.Misc.ProductWizard.Data;
using Nop.Services.Common; 
using Nop.Web.Framework.Menu;
using Nop.Core;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Nop.Plugin.Misc.ProductWizard
{
    public partial class ProductWizardRequirementRule : BasePlugin, IMiscPlugin, IAdminMenuPlugin
    {

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ISettingService _settingService;
        private readonly ProductWizardObjectContext _context;
        private readonly IWebHelper _webHelper;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public ProductWizardRequirementRule(IActionContextAccessor actionContextAccessor, IUrlHelperFactory urlHelperFactory, IWebHelper webHelper, ISettingService settingService, ProductWizardObjectContext context)
        {
            this._actionContextAccessor = actionContextAccessor;
            this._settingService = settingService;
            this._context = context;
            this._webHelper = webHelper;
            this._urlHelperFactory = urlHelperFactory;
        }
        

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = "Product Groups",
                SystemName = "Product Groups",               
                ControllerName = "Groups",
                ActionName = "List",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "area", "admin" } },
                 IconClass = "fa fa-dot-circle-o"
            };

            //var  SubMenuItem = new SiteMapNode()   // add child Custom menu
            //{
            //    Title = "Manage Groups", //   Title for your Sub Menu item
            //    SystemName = "Manage Groups",
            //    ControllerName = "Groups", // Your controller Name
            //    ActionName = "List", // Action Name
            //    Visible = true,
            //    RouteValues = new RouteValueDictionary() { { "Area", "admin" } },
            //    IconClass = "fa fa-genderless"
            //};
            //menuItem.ChildNodes.Add(SubMenuItem);

            //SubMenuItem = new SiteMapNode()   // add child Custom menu
            //{
            //    Title = "Items Compatability", //   Title for your Sub Menu item
            //    SystemName = "Items Compatability",
            //    ControllerName = "Groups", // Your controller Name
            //    ActionName = "ListItemRelationships", // Action Name
            //    Visible = true,
            //    RouteValues = new RouteValueDictionary() { { "Area", "admin" } },
            //    IconClass = "fa fa-genderless"
            //};
            //menuItem.ChildNodes.Add(SubMenuItem);


            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Catalog");
            if (pluginNode != null)
                pluginNode.ChildNodes.Add(menuItem);
            else
                rootNode.ChildNodes.Add(menuItem);
        }
        //public SiteMapNode BuildMenuItem() // SiteMapNode is Class in Nop.Web.Framework.Menu
        //{
        //    var menuItemBuilder = new SiteMapNode()
        //    {
        //        Title = "ProductWizard",   // Title for your Custom Menu Item
        //        Url = "/Groups/List", // Path of the action link
        //        Visible = true,
        //        RouteValues = new RouteValueDictionary() { { "Area", "Admin" } }
        //    };

        //    //var SubMenuItem = new SiteMapNode()   // add child Custom menu
        //    //{
        //    //    Title = "Title For Menu Chile menu item", //   Title for your Sub Menu item
        //    //    ControllerName = "Your Controller Name", // Your controller Name
        //    //    ActionName = "Configure", // Action Name
        //    //    Visible = true,
        //    //    RouteValues = new RouteValueDictionary() { { "Area", "Admin" } },
        //    //};
        //    //menuItemBuilder.ChildNodes.Add(SubMenuItem);


        //    return menuItemBuilder;

        //}

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/ProductWizard/Configure";

            //var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            //return urlHelper.Action("Configure", "Groups",
            //    new { discountId = discountId, discountRequirementId = discountRequirementId }).TrimStart('/');
        }
        /// <summary>
        /// Gets a route for plugin configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName,
            out string controllerName,
            out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "Groups";
            routeValues = new RouteValueDictionary()
            {
                { "Namespaces", "Nop.Plugin.Payments.ProductWizard.Controllers" },
                { "area", "Admin" }
            };
        }


        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.ProductWizard.Fields.GroupsAdmin", "Groups Admin");
            this.AddOrUpdatePluginLocaleResource("Plugins.ProductWizard.Fields.GroupsAdmin.Hint", "Groups Admin.");
            this.AddOrUpdatePluginLocaleResource("Plugins.ProductWizard.Fields.GroupsSelection", "Groups Selection");
            this.AddOrUpdatePluginLocaleResource("Plugins.ProductWizard.Fields.GroupsSelection.Hint", "Groups Selection.");
            this.AddOrUpdatePluginLocaleResource("Plugins.ProductWizard.Fields.GroupsName", "Groups Name");
            this.AddOrUpdatePluginLocaleResource("Plugins.ProductWizard.Fields.GroupsName.Hint", "Groups Name");

            this.AddOrUpdatePluginLocaleResource("Plugins.ProductWizard.ManageGroups", "Manage Groups");
            this.AddOrUpdatePluginLocaleResource("Plugins.ProductWizard.Fields.ManageGroups.Hint", "Manage Groups");

            this.AddOrUpdatePluginLocaleResource("Plugins.ProductWizard.AddGroup", "Add Group");
            this.AddOrUpdatePluginLocaleResource("Plugins.ProductWizard.Fields.AddGroup.Hint", "AddGroup");

            _context.Install();
            base.Install();
        }

       

        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.ProductWizard.Fields.GroupsAdmin");
            this.DeletePluginLocaleResource("Plugins.ProductWizard.Fields.GroupsAdmin.Hint");
            this.DeletePluginLocaleResource("Plugins.ProductWizard.Fields.GroupsSelection");
            this.DeletePluginLocaleResource("Plugins.ProductWizard.Fields.GroupsSelection.Hint");
            this.DeletePluginLocaleResource("Plugins.ProductWizard.Fields.GroupsName");
            this.DeletePluginLocaleResource("Plugins.ProductWizard.Fields.GroupsName.Hint");
            this.DeletePluginLocaleResource("Plugins.ProductWizard.Fields.ManageGroups");
            this.DeletePluginLocaleResource("Plugins.ProductWizard.Fields.ManageGroups.Hint");

           _context.Uninstall();
            base.Uninstall();
        }
    }
}
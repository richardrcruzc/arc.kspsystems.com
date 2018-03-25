using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;

namespace Nop.Plugin.DiscountRulesShippingType
{
    public partial class ShippingTypeDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
        #region Fields

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IDiscountService _discountService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;

        #endregion

        #region Ctor

        public ShippingTypeDiscountRequirementRule(IActionContextAccessor actionContextAccessor,
            IDiscountService discountService,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory)
        {
            this._actionContextAccessor = actionContextAccessor;
            this._discountService = discountService;
            this._settingService = settingService;
            this._urlHelperFactory = urlHelperFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>Result</returns>
        public DiscountRequirementValidationResult CheckRequirement(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //invalid by default
            var result = new DiscountRequirementValidationResult();

            if (request.Customer == null)
                return result;

            //try to get saved restricted customer role identifier
            var restrictedRoleId = _settingService.GetSettingByKey<int>(string.Format(DiscountRequirementDefaults.SettingsKey, request.DiscountRequirementId));
            if (restrictedRoleId == 0)
                return result;

            //result is valid if the shipping type have been choosen and order above xxx.xx
         // result.IsValid = request.Customer.ord.CustomerShippingType.Any(role => role.Id == restrictedRoleId && role.Active);

            return result;
        }

        /// <summary>
        /// Get URL for rule configuration
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
        /// <returns>URL</returns>
        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            return urlHelper.Action("Configure", "ShippingType",
                new { discountId = discountId, discountRequirementId = discountRequirementId }).TrimStart('/');
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRulesShippingType.Fields.ShippingType", "Required Shipping Type");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRulesShippingType.Fields.ShippingType.Hint", "Discount will be applied if customer have Shipping Type.");
  


            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRulesShippingType.Fields.OrderGraterThanAmount", "Order Grater Than Amount?");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRulesShippingType.Fields.OrderGraterThanAmount.Hint", "Discount Apply to Order Grater Than xx.xx Amount.");
 

            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRulesShippingType.Fields.ExcludeTax", "Exclude Tax?");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRulesShippingType.Fields.ExcludeTax.Hint", "Discount will be applied including or excluding Tax");
 



            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //discount requirements
            var discountRequirements = _discountService.GetAllDiscountRequirements()
                .Where(discountRequirement => discountRequirement.DiscountRequirementRuleSystemName == DiscountRequirementDefaults.SystemName);
            foreach (var discountRequirement in discountRequirements)
            {
                _discountService.DeleteDiscountRequirement(discountRequirement);
            }

            //locales
            this.DeletePluginLocaleResource("Plugins.DiscountRulesShippingType.Fields.ShippingType");
            this.DeletePluginLocaleResource("Plugins.DiscountRulesShippingType.Fields.ShippingType.Hint");

            this.DeletePluginLocaleResource("Plugins.DiscountRulesShippingType.Fields.OrderGraterThanAmount");
            this.DeletePluginLocaleResource("Plugins.DiscountRulesShippingType.Fields.OrderGraterThanAmount.Hint");

            this.DeletePluginLocaleResource("Plugins.DiscountRulesShippingType.Fields.ExcludeTax");
            this.DeletePluginLocaleResource("Plugins.DiscountRulesShippingType.Fields.ExcludeTax.Hint");


            base.Uninstall();
        }

        #endregion
    }
}
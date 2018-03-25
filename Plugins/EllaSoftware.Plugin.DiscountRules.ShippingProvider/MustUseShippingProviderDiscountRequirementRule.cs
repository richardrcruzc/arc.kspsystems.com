using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Orders;
using System;
using System.Linq;

namespace EllaSoftware.Plugin.DiscountRules.ShippingProvider
{
    public partial class MustUseShippingProviderDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IDiscountService _discountService;
        private readonly IOrderTotalCalculationService _calculationService;

        public MustUseShippingProviderDiscountRequirementRule(
            IOrderTotalCalculationService calculationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IDiscountService discountService)
        {
            this._calculationService = calculationService;
            this._settingService = settingService;
            this._storeContext = storeContext;
            this._urlHelperFactory = urlHelperFactory;
            this._actionContextAccessor = actionContextAccessor;
            this._discountService = discountService;
        }

        public DiscountRequirementValidationResult CheckRequirement(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            //invalid by default
            var result = new DiscountRequirementValidationResult();

            if (request.Customer == null)
                return result;

            var restrictedToShippingProviderId = _settingService.GetSettingByKey<string>(string.Format(DiscountRequirementDefaults.SettingsKey, request.DiscountRequirementId));
            if (string.IsNullOrEmpty(restrictedToShippingProviderId))
                return result;


            var restrictedToShippingType = _settingService.GetSettingByKey<string>(string.Format(DiscountRequirementDefaults.TypeNameSettingsKey, request.DiscountRequirementId));
            if (string.IsNullOrEmpty(restrictedToShippingType))
                return result;

            var restrictedToShippingAmount = _settingService.GetSettingByKey<decimal>(string.Format(DiscountRequirementDefaults.ShippingAmountKey, request.DiscountRequirementId));
            if (restrictedToShippingAmount<=0)
                return result;

            var restrictedToShippingAmountExcludeTax = _settingService.GetSettingByKey<bool>(string.Format(DiscountRequirementDefaults.ShippingAmountExcludeTaxKey, request.DiscountRequirementId));
        

            var customerSelectedShippingOption = request.Customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
            if (customerSelectedShippingOption != null && customerSelectedShippingOption.ShippingRateComputationMethodSystemName == restrictedToShippingProviderId)
            {
                //valid
                if (customerSelectedShippingOption.Name == restrictedToShippingType)
                {
                    //is order equal or greater than
                    if (request.Customer.HasShoppingCartItems)
                    {
                        var total = request.Customer.ShoppingCartItems.AsEnumerable().Sum(x => x.Quantity * x.Product.Price);
                        if(total>=restrictedToShippingAmount)
                        result.IsValid = true;
                    }
                }
                
            }

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
            return urlHelper.Action("Configure", "DiscountRulesMustUseShippingProvider",
                new { discountId = discountId, discountRequirementId = discountRequirementId }).TrimStart('/');
        }

        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.ShippingProviders.Fields.ShippingProviderName", "Required shipping provider");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.ShippingProviders.Fields.ShippingProviderName.Hint", "Discount will be applied only for selected shipping provider");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.ShippingProviders.Fields.ShippingProvider.Select", "Select shipping provider");

            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.ShippingProviders.Fields.ShippingTypeName", "Required Shipping Type Name");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.ShippingProviders.Fields.ShippingTypeName.Hint", "Discount will be applied only for selected shipping Type Name");

            
            base.Install();
        }

        public override void Uninstall()
        {
            //discount requirements
            var discountRequirements = _discountService.GetAllDiscountRequirements()
                .Where(discountRequirement => discountRequirement.DiscountRequirementRuleSystemName == DiscountRequirementDefaults.SystemName);
            foreach (var discountRequirement in discountRequirements)
            {
                _discountService.DeleteDiscountRequirement(discountRequirement);
            }

            base.Uninstall();
        }
    }
}

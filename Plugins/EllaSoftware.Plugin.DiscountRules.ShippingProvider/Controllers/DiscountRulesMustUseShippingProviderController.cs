using EllaSoftware.Plugin.DiscountRules.ShippingProvider.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Discounts;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EllaSoftware.Plugin.DiscountRules.ShippingProvider.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class DiscountRulesMustUseShippingProviderController : BasePluginController
    {
        private readonly IDiscountService _discountService;
        private readonly ICustomerService _customerService;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IShippingService _shippingService;
        private readonly ILocalizationService _localizationService;

        public DiscountRulesMustUseShippingProviderController(
            IDiscountService discountService,
            ICustomerService customerService,
            ISettingService settingService,
            IPermissionService permissionService,
            IShippingService shippingService,
            ILocalizationService localizationService)
        {
            this._discountService = discountService;
            this._customerService = customerService;
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._shippingService = shippingService;
            this._localizationService = localizationService;
        }

        [NonAction]
        public IList<SelectListItem> AddAvailableShippingProviders(IList<SelectListItem> list, string restrictedToShippingProviderId)
        {
            var query = _shippingService.LoadAllShippingRateComputationMethods();
            foreach (var provider in query)
            {
                var descriptor = provider.PluginDescriptor;
                if (descriptor != null)
                    list.Add(new SelectListItem {
                        Text = descriptor.FriendlyName,
                        Value = descriptor.SystemName,
                        Selected = descriptor.SystemName == restrictedToShippingProviderId
                    });
            }

            return list;
        }

        public IActionResult Configure(int discountId, int? discountRequirementId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            //check whether the discount requirement exists
            if (discountRequirementId.HasValue && !discount.DiscountRequirements.Any(requirement => requirement.Id == discountRequirementId.Value))
                return Content("Failed to load requirement.");

            var restrictedToShippingProviderId = _settingService.GetSettingByKey<string>(string.Format(DiscountRequirementDefaults.SettingsKey, discountRequirementId ?? 0));

            var restrictedToShippingTypeName = _settingService.GetSettingByKey<string>(string.Format(DiscountRequirementDefaults.TypeNameSettingsKey, discountRequirementId ?? 0));

            var restrictedToShippingAmount = _settingService.GetSettingByKey<decimal>(string.Format(DiscountRequirementDefaults.ShippingAmountKey, discountRequirementId ?? 0));
            var restrictedToShippingAmountExcludeTax = _settingService.GetSettingByKey<bool>(string.Format(DiscountRequirementDefaults.ShippingAmountExcludeTaxKey, discountRequirementId ?? 0));


            var model = new RequirementModel()
            {
                ShippingTypeName = restrictedToShippingTypeName,
                RequirementId = discountRequirementId ?? 0,
                DiscountId = discountId,
                ShippingProviderName = restrictedToShippingProviderId,
                ShippingAmount = restrictedToShippingAmount,
                 ShippingAmountExcludeTax= restrictedToShippingAmountExcludeTax
            };

            //set available shipping providers
            model.AvailableShippingProviders = AddAvailableShippingProviders(model.AvailableShippingProviders, restrictedToShippingProviderId);
            model.AvailableShippingProviders.Insert(0, new SelectListItem
            {
                Text = _localizationService.GetResource("Plugins.DiscountRules.ShippingProviders.Fields.ShippingProvider.Select"),
                Value = "0"
            });

            //add a prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format(DiscountRequirementDefaults.HtmlFieldPrefix, discountRequirementId ?? 0);

            return View("~/Plugins/EllaSoftware.ShippingProvider/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Configure(int discountId, int? discountRequirementId, string shippingProviderName, string shippingTypeName, decimal ShippingAmount, bool ShippingAmountExcludeTax)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            //get the discount requirement
            var discountRequirement = discountRequirementId.HasValue
                ? discount.DiscountRequirements.FirstOrDefault(requirement => requirement.Id == discountRequirementId.Value) : null;

            //the discount requirement does not exist, so create a new one
            if (discountRequirement == null)
            {
                discountRequirement = new DiscountRequirement
                {
                    DiscountRequirementRuleSystemName = DiscountRequirementDefaults.SystemName
                };
                discount.DiscountRequirements.Add(discountRequirement);
                _discountService.UpdateDiscount(discount);
            }

            //save restricted customer role identifier
            _settingService.SetSetting(string.Format(DiscountRequirementDefaults.SettingsKey, discountRequirement.Id), shippingProviderName);

            //save restricted shippingTypeName
            _settingService.SetSetting(string.Format(DiscountRequirementDefaults.TypeNameSettingsKey, discountRequirement.Id), shippingTypeName);

            //save restricted shippingTypeName
            _settingService.SetSetting(string.Format(DiscountRequirementDefaults.ShippingAmountExcludeTaxKey, discountRequirement.Id), ShippingAmountExcludeTax);

            //save restricted shippingTypeName
            _settingService.SetSetting(string.Format(DiscountRequirementDefaults.ShippingAmountKey, discountRequirement.Id), ShippingAmount);


            return Json(new { Result = true, NewRequirementId = discountRequirement.Id });
        }
    }
}

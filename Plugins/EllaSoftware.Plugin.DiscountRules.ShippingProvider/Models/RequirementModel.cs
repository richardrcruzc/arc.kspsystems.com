using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace EllaSoftware.Plugin.DiscountRules.ShippingProvider.Models
{
    public class RequirementModel
    {
        public RequirementModel()
        {
            AvailableShippingProviders = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.DiscountRules.ShippingProviders.Fields.ShippingProviderName")]
        public string ShippingProviderName { get; set; }

        [NopResourceDisplayName("Plugins.DiscountRules.ShippingProviders.Fields.ShippingTypeName")]
        public string ShippingTypeName { get; set; }

        [NopResourceDisplayName("Plugins.DiscountRules.ShippingProviders.Fields.ShippingAmount")]
        public decimal ShippingAmount { get; set; }

        [NopResourceDisplayName("Plugins.DiscountRules.ShippingProviders.Fields.ShippingAmountExcludeTax")]
        public bool ShippingAmountExcludeTax { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }

        public IList<SelectListItem> AvailableShippingProviders { get; set; }
    }
}

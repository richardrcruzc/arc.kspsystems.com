using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.DiscountRulesShippingType.Models
{
    public class RequirementModel
    {
        public RequirementModel()
        {
            AvailableCustomerRoles = new List<SelectListItem>();
        }

        public int ExcludeTax { get; set; }
        public int OrderGraterThanAmount { get; set; }

        [NopResourceDisplayName("Plugins.DiscountRulesShippingType.Fields.ShippingType")]
        public int CustomerRoleId { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }

        public IList<SelectListItem> AvailableCustomerRoles { get; set; }
    }
}
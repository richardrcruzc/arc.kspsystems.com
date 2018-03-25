
namespace Nop.Plugin.DiscountRulesShippingType
{
    /// <summary>
    /// Represents constants for the discount requirement rule
    /// </summary>
    public static class DiscountRequirementDefaults
    {
        /// <summary>
        /// The system name of the discount requirement rule
        /// </summary>
        public const string SystemName = "DiscountRequirement.MustBeAssignedToShippingType";

        /// <summary>
        /// The key of the settings to save restricted customer roles
        /// </summary>
        public const string SettingsKey = "DiscountRequirement.MustBeAssignedToShippingType-{0}";

        /// <summary>
        /// The HTML field prefix for discount requirements
        /// </summary>
        public const string HtmlFieldPrefix = "DiscountRulesShippingTypes{0}";
    }
}

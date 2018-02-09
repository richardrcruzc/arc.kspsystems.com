
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Plugin.Misc.ProductWizard.Models
{
    public class GenericModel: BaseNopEntityModel
    { 
        public bool ExcludeGoogleFeed { get; set; }
        public string Color { get; set; }
    }
    public class ItemIdModel
    {
        public int ItemId { get; set; }
        public string ProductName { get; set; }
    }

    public class LegacyModel
    {
        public string ManufacturerName { get; set; }
        public string PartNumber { get; set; }
    }
}


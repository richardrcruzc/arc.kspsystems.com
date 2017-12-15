
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Plugin.Misc.ProductWizard.Models
{
    public class ProductNameModel : BaseNopEntityModel
    { 
        public string  ProductName { get; set; }
        public int ItemID { get; set; }
        public int ItemIDPart { get; set; }
    }
   
}


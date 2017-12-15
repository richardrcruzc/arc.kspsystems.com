using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ProductWizard.Models
{
    public partial class ProductExtModel : BaseNopEntityModel
    {
        public ProductExtModel()
        {
            UsedInModel = new List<UsedInModel>();
            LegacyIdModel = new List<LegacyIdModel>();
            CategoryModel = new List<CategoryModel>();
            PartForItem = new List<UsedInModel>();
            PartForItemId = new List<int>();
        }
        
              public bool IsCopier { get; set; }
        public string FullDescription { get; set; }
        public string PartNumber { get; set; }
        public List<UsedInModel> UsedInModel { get; set; }
        public List<LegacyIdModel> LegacyIdModel { get; set; }
        public List<CategoryModel> CategoryModel { get; set; }

        public List<UsedInModel> PartForItem { get; set; }
        public List<int> PartForItemId { get; set; }
    }

    public partial class UsedInModel : BaseNopEntityModel
    {
        public string ProductName { get; set; }
        public string SeName { get; set; } 
    }
    public partial class LegacyIdModel : BaseNopEntityModel
    {
        public string LegacyName { get; set; }
        
    }
    public partial class CategoryModel : BaseNopEntityModel
    {
        public string CatergoryName { get; set; }

    }
}


using Nop.Web.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ProductWizard.Models
{
  public  class PartsForThisItemModel: BaseNopEntityModel
    {
        
            public string ThumbImageUrl { get; set; }
        public string ProductName { get; set; }
        public string PartNumber { get; set; }
        public string Manufacturer { get; set; }
        public string Category { get; set; }
        public string CategorySeo { get; set; }
        public decimal Price { get; set; }
        public int Qty { get; set; }
        public string SeName { get; set; }
        public bool OutOfStock { get; set; }


    }
}

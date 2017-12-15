using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ProductWizard.Models
{
   public partial class ItemsCompatabilityModel
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string Sku { get; set; }
        public string CategoryName { get; set; }
        public int ItemIdPart { get; set; }
        public string ItemIdPartName { get; set; }

    }
}

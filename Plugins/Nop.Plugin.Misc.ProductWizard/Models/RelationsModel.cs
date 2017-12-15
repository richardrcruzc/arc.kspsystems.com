using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ProductWizard.Models
{
    public partial class RelationsModel
    {
        public List<String> FilterCriteria { get; set; }
        public string SearchValue { get; set; }
        public bool ParentChild { get; set; } 
    }
}

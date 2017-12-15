using Nop.Core;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ProductWizard.Domain
{
    public partial class RelationsGroupsItems: BaseEntity
    {
        public int GroupId { get; set; }
        public int ItemId { get; set; }
    //    public Product Item { get; set; }

        public string Direction { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
        public bool Deleted { get; set; }
    }
}

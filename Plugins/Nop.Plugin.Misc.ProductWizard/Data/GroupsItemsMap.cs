using Nop.Data.Mapping;
using Nop.Plugin.Misc.ProductWizard.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ProductWizard.Data
{
     
    public partial class GroupsItemsMap : NopEntityTypeConfiguration<GroupsItems>
    {
        public GroupsItemsMap()
        {
            this.ToTable("Groups-Items");
            this.HasKey(tr => tr.Id);
            //this.Property(tr => tr.Percentage).HasPrecision(18, 4);
        }
    }
}

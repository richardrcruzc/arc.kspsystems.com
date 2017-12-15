using Nop.Data.Mapping;
using Nop.Plugin.Misc.ProductWizard.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ProductWizard.Data
{ 
    public partial class GroupsMap : NopEntityTypeConfiguration<Groups>
    {
        public GroupsMap()
        {
            this.ToTable("Groups");
            this.HasKey(tr => tr.Id);
            this.Property(tr => tr.GroupName).IsRequired().HasMaxLength(255);
        }
    }
}

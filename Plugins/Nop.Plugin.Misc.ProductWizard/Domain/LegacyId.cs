using Nop.Core;

namespace Nop.Plugin.Misc.ProductWizard.Domain
{
    public partial class LegacyId : BaseEntity
    {
        
            public int ItemId { get; set; }
        public string LegacyCode { get; set; }
        public bool Deleted { get; set; }
    }
}

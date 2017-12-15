using GodaddyWrapper.Responses;
using Nop.Web.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.GoDaddyApi.Models
{
    public class PublicInfoModel : BaseNopModel
    {
        public string DomainToSearch { get; set; }

        public List<DomainAvailableResponse> DomainAvailableResponse { get; set; }
        public List<DomainSuggestionResponse> DomainSuggestionResponse { get; set; }

    }
}
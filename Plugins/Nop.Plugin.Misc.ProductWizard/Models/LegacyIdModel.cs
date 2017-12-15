using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Attributes; 
using Nop.Web.Framework.Localization;
using Nop.Plugin.Misc.ProductWizard.Validators;
using System;
using Nop.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Plugin.Misc.ProductWizard.Models
{

    public partial class LegacyIdModel : BaseNopEntityModel
    {
        public LegacyIdModel()
        {

        }



        public int itemId { get; set; }
        public string LegacyCode { get; set; }
        public bool Deleted { get; set; }


    }

}
using System.ComponentModel.DataAnnotations;
using FluentValidation.Attributes;
using Microsoft.AspNetCore.Http;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;
using Nop.Web.Validators.Common;

namespace Nop.Web.Models.Common
{
    [Validator(typeof(SupportValidator))]
    public partial class SupportModel : BaseNopModel
    {
        [NopResourceDisplayName("Support.FullName")]
        public string FullName { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Support.Email")]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        [NopResourceDisplayName("Support.Phone")]
        public string Phone { get; set; }

        [NopResourceDisplayName("Support.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Support.Copier")]
        public string Copier { get; set; }

        [NopResourceDisplayName("Support.Company")]
        public string Company { get; set; }

        [NopResourceDisplayName("Support.AccessoryModel")]
        public string AccessoryModel { get; set; }

        [NopResourceDisplayName("Support.LocationOfIssue")]
        public string LocationOfIssue { get; set; }

        [NopResourceDisplayName("Support.ErrorCode")]
        public string ErrorCode { get; set; }

        [NopResourceDisplayName("Support.HowLongHaveYouHadThisIssue")]
        public string HowLongHaveYouHadThisIssue { get; set; }

        [NopResourceDisplayName("Support.PartsSuppliesAffected")]
        public string PartsSuppliesAffected { get; set; }


        [NopResourceDisplayName("Support.Attachments")]
        public IFormFile Attachments { get; set; }


        public bool SuccessfullySent { get; set; }
        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}
using FluentValidation;
using Nop.Core.Domain.Common;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Common;

namespace Nop.Web.Validators.Common
{
    public partial class SupportValidator : BaseNopValidator<SupportModel>
    {
        public SupportValidator(ILocalizationService localizationService, CommonSettings commonSettings)
        {

            RuleFor(x => x.FullName).NotEmpty().WithMessage(localizationService.GetResource("Support.FullName.Required"));
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Support.Email.Required"));
            RuleFor(x => x.Phone).NotEmpty().WithMessage(localizationService.GetResource("Support.Phone.Required"));
            RuleFor(x => x.Description).NotEmpty().WithMessage(localizationService.GetResource("Support.Description.Required"));
            RuleFor(x => x.Copier).NotEmpty().WithMessage(localizationService.GetResource("Support.Copier.Required"));
           // RuleFor(x => x.Company).NotEmpty().WithMessage(localizationService.GetResource("Support.Company.Required"));
          //  RuleFor(x => x.AccessoryModel).NotEmpty().WithMessage(localizationService.GetResource("Support.AccessoryModel.Required"));
          //  RuleFor(x => x.LocationOfIssue).NotEmpty().WithMessage(localizationService.GetResource("Support.LocationOfIssue.Required"));
           // RuleFor(x => x.ErrorCode).NotEmpty().WithMessage(localizationService.GetResource("Support.ErrorCode.Required"));
         //   RuleFor(x => x.HowLongHaveYouHadThisIssue).NotEmpty().WithMessage(localizationService.GetResource("Support.HowLongHaveYouHadThisIssue.Required"));
         //   RuleFor(x => x.PartsSuppliesAffected).NotEmpty().WithMessage(localizationService.GetResource("Support.PartsSuppliesAffected.Required"));
            


            //RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.Email.Required"));
            //RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            //RuleFor(x => x.FullName).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.FullName.Required"));
            //if (commonSettings.SubjectFieldOnContactUsForm)
            //{
            //    RuleFor(x => x.Subject).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.Subject.Required"));
            //}
            //RuleFor(x => x.Enquiry).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.Enquiry.Required"));
        }
    }
}
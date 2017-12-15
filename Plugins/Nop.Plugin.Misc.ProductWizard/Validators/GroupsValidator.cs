using FluentValidation;
using FluentValidation.Results;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.ProductWizard.Domain;
using Nop.Plugin.Misc.ProductWizard.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;


namespace Nop.Plugin.Misc.ProductWizard.Validators
{ 

    public partial class GroupsValidator : BaseNopValidator<GroupsModel>
    {
        public GroupsValidator()
        { 

            RuleFor(x => x.GroupName).NotEmpty().WithMessage("Name is Required");
    
    
        }
    }
}

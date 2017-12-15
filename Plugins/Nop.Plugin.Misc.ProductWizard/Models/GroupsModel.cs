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
    [Validator(typeof(GroupsValidator))]
    public partial class GroupsModel : BaseNopEntityModel, ILocalizedModel<GroupsLocalizedModel>
    {
        public GroupsModel()
        {
            if (PageSize < 1)
            {
                PageSize = 5;
            }

            Locales = new List<GroupsLocalizedModel>();
        }

        
        public string GroupName { get; set; }
        public string ItemName { get; set; }
        public int ItemId { get; set; }
        public int Interval { get; set; }
        public int Percentage { get; set; }

        public IList<GroupsLocalizedModel> Locales { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.PageSize")]
        public int PageSize { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.PageSizeOptions")]
        public string PageSizeOptions { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.AllowCustomersToSelectPageSize")]
        public bool AllowCustomersToSelectPageSize { get; set; }



        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaKeywords")]
        
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaDescription")]
        
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaTitle")]
        
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.SeName")]
        
        public string SeName { get; set; }

        public DateTime CreatedOnUtc { get; set; }
         
        public DateTime UpdatedOnUtc { get; set; }

    }

    public partial class GroupsListModel : BaseNopModel
    {
        public GroupsListModel()
        {
            AvailableStores = new List<SelectListItem>();
        }
        [NopResourceDisplayName("Plugins.ProductWizard.Fields.GroupsName")]
        
        public string SearchGroupName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.List.SearchStore")]
        public int SearchStoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }

    public partial class GroupsLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        
        public string GroupName { get; set; }
        public int Interval { get; set; }
        public int Percentage { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaDescription")]
        
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.MetaTitle")]
        
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.SeName")]
        
        public string SeName { get; set; }
    }



    public partial class GroupsItemsListModel  
    {
        public GroupsItemsListModel()
        {
            //AvailableStores = new List<SelectListItem>();
        }
        public int GroupItemId { get; set; }
        public int GroupId { get; set; }
        public string Relationship { get; set; }

        public int ItemId { get; set; }
        public string ItemName { get; set; }
         
    }

    public partial class GroupsItemsModel  
    {
        public GroupsItemsModel()
        {
            //AvailableStores = new List<SelectListItem>();
        }
        public int GroupItemId { get; set; }
        public int GroupId { get; set; }
        public string Relationship { get; set; }

        public int ItemId { get; set; }
        public string ItemName { get; set; }

    }


}

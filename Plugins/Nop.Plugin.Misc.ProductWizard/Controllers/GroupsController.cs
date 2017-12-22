using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.ProductWizard.Domain;
using Nop.Plugin.Misc.ProductWizard.Infrastructure;
using Nop.Plugin.Misc.ProductWizard.Models;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Nop.Plugin.Misc.ProductWizard.Controllers
{

    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class GroupsController : BasePluginController
    {
        #region Fields
        private readonly MediaSettings _mediaSettings;
        private readonly IDbContext _dbContext;
        private readonly IRepository<Groups> _gpRepository;
        private readonly IRepository<GroupsItems> _gpiRepository;
        private readonly IRepository<RelationsGroupsItems> _rgpRepository;
        private readonly IRepository<ItemsCompatability> _iRepository;

        private readonly ICategoryService _categoryService;
        private readonly ICategoryTemplateService _categoryTemplateService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IDiscountService _discountService;
        private readonly IPermissionService _permissionService;
        private readonly IAclService _aclService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IExportManager _exportManager;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IVendorService _vendorService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IWorkContext _workContext;
        private readonly IImportManager _importManager;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Constructors

        public GroupsController(
            MediaSettings mediaSettings,
            IDbContext dbContext,
            IRepository<ItemsCompatability> iRepository,
            IRepository<Groups> gpRepository,
              IRepository<GroupsItems> gpiRepository,
                IRepository<RelationsGroupsItems> rgpRepository,
            ICategoryService categoryService, ICategoryTemplateService categoryTemplateService,
            IManufacturerService manufacturerService, IProductService productService,
            ICustomerService customerService,
            IUrlRecordService urlRecordService,
            IPictureService pictureService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IDiscountService discountService,
            IPermissionService permissionService,
            IAclService aclService,
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            IExportManager exportManager,
            IVendorService vendorService,
            ICustomerActivityService customerActivityService,
            CatalogSettings catalogSettings,
            IWorkContext workContext,
            IImportManager importManager,
            ICacheManager cacheManager)
        {
            this._mediaSettings = mediaSettings;
            this._dbContext = dbContext;
            this._iRepository = iRepository;
            this._gpRepository = gpRepository;
            this._gpiRepository = gpiRepository;
            this._rgpRepository = rgpRepository;
            this._categoryService = categoryService;
            this._categoryTemplateService = categoryTemplateService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._customerService = customerService;
            this._urlRecordService = urlRecordService;
            this._pictureService = pictureService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._localizedEntityService = localizedEntityService;
            this._discountService = discountService;
            this._permissionService = permissionService;
            this._vendorService = vendorService;
            this._aclService = aclService;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
            this._exportManager = exportManager;
            this._customerActivityService = customerActivityService;
            this._catalogSettings = catalogSettings;
            this._workContext = workContext;
            this._importManager = importManager;
            this._cacheManager = cacheManager;
        }
        #endregion

        public IActionResult Configure( )
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            
            return View("~/Plugins/Misc.ProductWizard/Views/Configure.cshtml");
        }
        

        #region List

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var model = new GroupsListModel();
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            return View("~/Plugins/Misc.ProductWizard/Views/Groups/List.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult List(DataSourceRequest command, GroupsListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();
            
            var query = _gpRepository.TableNoTracking.Where(x=>x.Deleted==false );

            query = query.OrderBy(x => x.GroupName);


            if (!string.IsNullOrEmpty(model.SearchGroupName))
                query = query.Where(x => x.GroupName.Contains(model.SearchGroupName));  

            var groups = new PagedList<Groups>(query, command.Page - 1, command.PageSize);
             
            var gridModel = new DataSourceResult
            {
                Data = groups.Select(x =>
                {
                    var groupsModel = x.ToModel();
                    //categoryModel.Breadcrumb = x.GetFormattedBreadCrumb(_categoryService);
                    return groupsModel;
                }),
                Total = groups.TotalCount
            };
            return Json(gridModel);
        } 

        public virtual IActionResult ListItemRelationships()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            var model = new ItemsCompatabilityModel { };
             
            return View("~/Plugins/Misc.ProductWizard/Views/Items/List.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult ListItemRelationships(DataSourceRequest command, ItemsCompatabilityModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();
            try
            {
                var query = _iRepository.TableNoTracking.OrderBy(x=>x.Id).Where(x => x.Deleted == false);
 
                var groups = new PagedList<ItemsCompatability>(query, command.Page - 1, command.PageSize);

                var gridModel = new DataSourceResult
                {
                    Data = groups.Select(x =>
                    {
                        var pn = "";
                        var pn1 = "";
                        var product = _productService.GetProductById(x.ItemId);
                        if (product != null) { pn = product.Name; }

                        product = _productService.GetProductById(x.ItemIdPart);
                        if (product != null) { pn1 = product.Name; }

                        var groupsModel = new ItemsCompatabilityModel { Id = x.Id, ItemName = pn, ItemId = x.ItemId, ItemIdPart = x.ItemIdPart, ItemIdPartName = pn1 };
                        //categoryModel.Breadcrumb = x.GetFormattedBreadCrumb(_categoryService);
                        return groupsModel;
                    }),
                    Total = groups.TotalCount
                };
                return Json(gridModel);
            }
            catch(Exception ex) { return Json(new DataSourceResult()); }
           
        }



        [HttpPost]
        public virtual IActionResult GetGroupsItems(DataSourceRequest command, GroupsItemsListModel model, int groupId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();

            var query = _gpiRepository.TableNoTracking.Where(x => x.Deleted == false && x.GroupId ==groupId);

            query = query.OrderBy(x => x.ItemId);
            
            var groups = new PagedList<GroupsItems>(query, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = groups.Select(x =>
                {
                    var pn = "";
                    var product = _productService.GetProductById(x.ItemId);
                    if (product != null) { pn = product.Name; }
                    var groupsModel = new GroupsItemsListModel { GroupItemId=x.Id,   ItemId = x.ItemId, GroupId = x.GroupId, ItemName = pn, Relationship= x.Relationship };
                    //categoryModel.Breadcrumb = x.GetFormattedBreadCrumb(_categoryService);
                    return groupsModel;
                }),
                Total = groups.TotalCount
            };
            return Json(gridModel);
        }

        

            [HttpPost]
        public virtual JsonResult GetItems(string text)
        {
            //var searchValue =  Request.Query["filter[filters][0][value]"];
            if (string.IsNullOrEmpty(text))
                return Json(null);

            var products = _productService.SearchProducts(keywords: text, orderBy:ProductSortingEnum.NameAsc).Select(x=> new  { ItemName = x.Name, ItemId = x.Id });

            return Json(products);

        }
        #endregion

            #region Create / Edit / Delete
            #region group
        [HttpPost]
        public virtual IActionResult Create(GroupsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();


             var group = model.ToEntity();
           group.CreatedOnUtc = DateTime.UtcNow;
            group.UpdatedOnUtc = DateTime.UtcNow;
             

                _gpRepository.Insert(group);


            return Json(group);
        }
        [HttpPost]
        public virtual IActionResult CreateItem(GroupsItemsListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var item = new GroupsItems
            {Id=1 };
            if (!string.IsNullOrEmpty(model.ItemName) && !string.IsNullOrWhiteSpace(model.ItemName))
            {
                model.ItemName = WebUtility.UrlDecode(model.ItemName);

                 model.ItemId = _productService.SearchProducts(keywords: model.ItemName, orderBy: ProductSortingEnum.NameAsc).FirstOrDefault().Id;

                item = new GroupsItems { GroupId = model.GroupId , ItemId = model.ItemId  , Relationship = model.Relationship };

                _gpiRepository.Insert(item);

                var groupsModel = new GroupsItemsListModel { GroupItemId = item.Id, ItemId = item.ItemId, GroupId = item.GroupId, ItemName = model.ItemName, Relationship = item.Relationship };

                var groupsModels = new List<GroupsItemsListModel>();
                groupsModels.Add(groupsModel);

                model.GroupItemId = item.Id;
                model.GroupId = item.GroupId;
                model.ItemId = item.ItemId;
                model.Relationship = item.Relationship;





                var gridModel = new DataSourceResult
                {
                    Data = groupsModels,

                    //Data = groups.Select(x =>
                    //{
                    //    var pn = "";
                    //    var product = _productService.GetProductById(x.ItemId);
                    //    if (product != null) { pn = product.Name; }
                    //    var groupsModel = new GroupsItemsListModel { GroupItemId = x.Id, ItemId = x.ItemId, GroupId = x.GroupId, ItemName = pn, Relationship = x.Relationship };
                    //    //categoryModel.Breadcrumb = x.GetFormattedBreadCrumb(_categoryService);
                    //    return groupsModel;
                    //}),
                    Total = 0 //groups.TotalCount
                };



                return Json(gridModel);

            }
            return Json( model );

            //return new NullJsonResult();
        }

        [HttpPost]
        public virtual IActionResult CreateItemsCompatability(ItemsCompatabilityModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

          //  model.ItemId = _productService.SearchProducts(keywords: model.ItemName, orderBy: ProductSortingEnum.NameAsc).FirstOrDefault().Id;

           // model.ItemIdPart = _productService.SearchProducts(keywords: model.ItemIdPartName, orderBy: ProductSortingEnum.NameAsc).FirstOrDefault().Id;


            var parts = new ItemsCompatability {
                 ItemId= model.ItemId,
                  ItemIdPart = model.ItemIdPart,
                 Deleted=false,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow,
        };
            


            _iRepository.Insert(parts);


            return Json(parts);
        }
        [HttpPost]
        public virtual IActionResult UpdateItemsCompatability(ItemsCompatabilityModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var entity = _iRepository.Table.Where(x => x.Id == model.Id).FirstOrDefault();
            if (entity == null)
                return Json(model);


            entity.ItemId = model.ItemId;
                entity.ItemIdPart = model.ItemIdPart;
            entity.Deleted = false;

            entity.UpdatedOnUtc = DateTime.UtcNow;
           



            _iRepository.Update(entity);


            return Json(entity);
        }

        [HttpPost]
        public virtual IActionResult DestroyItemsCompatability(ItemsCompatabilityModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();


            var itemDelete = _iRepository.Table.Where(x => x.Id == model.Id).FirstOrDefault();
            if (itemDelete != null)
            {
                itemDelete.Deleted = true;
                _iRepository.Update(itemDelete);
            }



            return new NullJsonResult();
        }


        [HttpPost]
        public virtual IActionResult UpdateItem(GroupsItemsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();


            if (!string.IsNullOrEmpty(model.ItemName) && !string.IsNullOrWhiteSpace(model.ItemName))
            {
                model.ItemName = WebUtility.UrlDecode(model.ItemName);

                model.ItemId = _productService.SearchProducts().Where(x => x.Name == model.ItemName).FirstOrDefault().Id;

                var itemUpdate = _gpiRepository.Table.Where(x => x.Id == model.GroupItemId).FirstOrDefault();
                if (itemUpdate != null)
                {
                    itemUpdate.Deleted=false;
                    itemUpdate.ItemId = model.ItemId ;
                    _gpiRepository.Update(itemUpdate);
                }
            }




            return   Json(model);
        }
        [HttpPost]
        public virtual IActionResult DestroyItem(GroupsItemsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();


            var itemDelete = _gpiRepository.Table.Where(x => x.Id == model.GroupItemId).FirstOrDefault();
            if (itemDelete != null)
            {
                itemDelete.Deleted = true;
                _gpiRepository.Update(itemDelete);
            }



            return new NullJsonResult();
        }

        [HttpPost]
        public virtual IActionResult Update(GroupsModel model )
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var entity = _gpRepository.Table.Where(x => x.Id == model.Id).FirstOrDefault();
            if (entity != null)
            {
                entity.GroupName = model.GroupName;
                entity.Interval = model.Interval;
                entity.Percentage = model.Percentage;
                entity.UpdatedOnUtc = DateTime.UtcNow;

                _gpRepository.Update(entity);
            }
            //var group = model.ToEntity();
            //group.CreatedOnUtc = DateTime.UtcNow;
            //group.UpdatedOnUtc = DateTime.UtcNow;

            //_gpRepository.Update(group);



            return Json(model);
        }


        [HttpPost]
        public virtual IActionResult Destroy(GroupsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();


            var entity = _gpRepository.Table.Where(x => x.Id == model.Id).FirstOrDefault();
            if (entity != null)
            {
                //entity.GroupName = model.GroupName;
                //entity.Interval = model.Interval;
                //entity.Percentage = model.Percentage;
                entity.UpdatedOnUtc = DateTime.UtcNow;
                entity.Deleted = true;

                _gpRepository.Update(entity);
            }


            return new NullJsonResult();
        }

        #endregion


    
        //#region Security

        ///// <summary>
        ///// Access denied view
        ///// </summary>
        ///// <returns>Access denied view</returns>
        //protected virtual IActionResult AccessDeniedView()
        //{
        //    var webHelper = EngineContext.Current.Resolve<IWebHelper>();

        //    //return Challenge();
        //    return RedirectToAction("AccessDenied", "Security", new { pageUrl = webHelper.GetRawUrl(this.Request) });
        //}

        ///// <summary>
        ///// Access denied JSON data for kendo grid
        ///// </summary>
        ///// <returns>Access denied JSON data</returns>
        //protected JsonResult AccessDeniedKendoGridJson()
        //{
        //    var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        //    return ErrorForKendoGridJson(localizationService.GetResource("Admin.AccessDenied.Description"));
        //}

        //#endregion
        #endregion
    }
}

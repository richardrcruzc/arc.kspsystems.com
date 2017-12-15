using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.ProductWizard.Domain;
using Nop.Plugin.Misc.ProductWizard.Infrastructure;
using Nop.Plugin.Misc.ProductWizard.Models;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
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
    public class RelationsController : BasePluginController
    {
        #region Fields

        private readonly VendorSettings _vendorSettings;
        private readonly IShippingService _shippingService;
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

        public RelationsController(
            IShippingService shippingService,
        VendorSettings vendorSettings,
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
            this._shippingService= shippingService;
            this._vendorSettings= vendorSettings;
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

        #region List

        public virtual IActionResult List(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var itemName = _productService.GetProductById(id).Name;


            var model = new ProductListModel
            {
                //a vendor should have access only to his products
                IsLoggedInAsVendor = _workContext.CurrentVendor != null,
                AllowVendorsToImportProducts = _vendorSettings.AllowVendorsToImportProducts,
                ItemId = id,
                ItemName = itemName,
                BelongsTo = true,
                SingleType=true,
            };

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
            foreach (var c in categories)
                model.AvailableCategories.Add(c);

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var manufacturers = SelectListHelper.GetManufacturerList(_manufacturerService, _cacheManager, true);
            foreach (var m in manufacturers)
                model.AvailableManufacturers.Add(m);

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var wh in _shippingService.GetAllWarehouses())
                model.AvailableWarehouses.Add(new SelectListItem { Text = wh.Name, Value = wh.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var vendors = SelectListHelper.GetVendorList(_vendorService, _cacheManager, true);
            foreach (var v in vendors)
                model.AvailableVendors.Add(v);

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            //"published" property
            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.All"), Value = "0" });
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.PublishedOnly"), Value = "1" });
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.UnpublishedOnly"), Value = "2" });


            return View("~/Plugins/Misc.ProductWizard/Views/Relations/List.cshtml", model);
            //return View("~/Plugins/Misc.ProductWizard/Views/Relations/List.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult ProductList(DataSourceRequest command, ProductListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            var categoryIds = new List<int> { model.SearchCategoryId };
            //include subcategories
            if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
                categoryIds.AddRange(GetChildCategoryIds(model.SearchCategoryId));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = _productService.SearchProducts(
                categoryIds: categoryIds,
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,                
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true,
                overridePublished: overridePublished
            );

            if (!string.IsNullOrEmpty(model.GoDirectlyToSku) )
                products = _productService.SearchProducts(searchSku:true,keywords: model.GoDirectlyToSku);

           var gridModel = new DataSourceResult
            {
                Data = products.Select(x =>
                {
                    var productModel = new ProductModel();
                    productModel.Id = x.Id;
                    //little performance optimization: ensure that "FullDescription" is not returned
                    productModel.FullDescription = "";
                    productModel.Name = x.Name;
                    productModel.Sku = x.Sku;
                    productModel.Price = x.Price;
                    productModel.StockQuantityStr = x.StockQuantity.ToString();
                    productModel.Published = x.Published;
                    //productModel.SeName = x.Name;
                    foreach (var cat in x.ProductCategories.ToList())
                      productModel.NameCategory += cat.Category.Name+ " ";
                    //picture
                    var defaultProductPicture = _pictureService.GetPicturesByProductId(x.Id, 1).FirstOrDefault();
                    productModel.PictureThumbnailUrl = _pictureService.GetPictureUrl(defaultProductPicture, 75, true);
                    //product type
                    productModel.ProductTypeName = x.ProductType.GetLocalizedEnum(_localizationService, _workContext);
                    //friendly stock qantity
                    //if a simple product AND "manage inventory" is "Track inventory", then display
                    if (x.ProductType == ProductType.SimpleProduct && x.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                        productModel.StockQuantityStr = x.GetTotalStockQuantity().ToString();
                    return productModel;
                }),
                Total = products.TotalCount
            };

            return Json(gridModel);
        }

        #endregion

        #region Single Relations
        [HttpPost]
        public virtual IActionResult AddSingleRelations(ICollection<int> selectedIds, int itemId, bool gridIsPartofItem, bool ItemIsPartofGrid)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                foreach (var selectedIdenId in selectedIds) {
                    var itemCompactible = new ItemsCompatability {
                        Deleted = false,
                         CreatedOnUtc = DateTime.UtcNow,
                      UpdatedOnUtc = DateTime.UtcNow,
                    };

                    if (gridIsPartofItem == true)
                    {
                        itemCompactible.ItemId = itemId;
                        itemCompactible.ItemIdPart  = selectedIdenId;
                    }
                    else if (ItemIsPartofGrid == true)
                    {
                        itemCompactible.ItemId = selectedIdenId;
                        itemCompactible.ItemIdPart = itemId;
                    }
                    _iRepository.Insert(itemCompactible);                    
                }
            } 

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual IActionResult DeleteSingleRelations(ICollection<int> selectedIds, int itemId, bool wizardBelongGrid, bool gridBelongsWizard)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                foreach (var selectedIdenId in selectedIds)
                {
                    var compact = _iRepository.Table.Where(x => x.Id == selectedIdenId ).FirstOrDefault();
                    compact.Deleted = true;
                      _iRepository.Update(compact);


                    //if (wizardBelongGrid == true)
                    //{
                    //    var compact = _iRepository.Table.Where(x => x.ItemId == selectedIdenId && x.ItemIdPart == itemId).ToList();
                    //    _iRepository.Delete(compact);

                    //}
                    //else if (gridBelongsWizard == true)
                    //{

                    //    var compact = _iRepository.Table.Where(x => x.ItemIdPart == selectedIdenId && x.ItemId == itemId).ToList();
                    //    _iRepository.Delete(compact);
                    //}

                }
            }

            return Json(new { Result = true });
        }


        [HttpPost]
        public virtual IActionResult ListItemRelationships(DataSourceRequest command, int itemId, bool wizardBelongGrid, bool gridBelongsWizard)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();
            try
            {
                var query = _iRepository.TableNoTracking.OrderBy(x => x.Id).Where(x => x.Deleted == false);

                if (gridBelongsWizard)
                    query = query.Where(x => x.ItemId == itemId);

                if (wizardBelongGrid)
                    query = query.Where(x => x.ItemIdPart == itemId);

                var groups = new PagedList<ItemsCompatability>(query, command.Page - 1, command.PageSize);

                var gridModel = new DataSourceResult
                {
                    Data = groups.Select(x =>
                    {
                        var pn = "";
                        var pn1 = "";

                        var product = _productService.GetProductById(x.ItemIdPart);
                        if (gridBelongsWizard)
                            if (product != null) { pn = product.Name; }

                        if (wizardBelongGrid)
                            product = _productService.GetProductById(x.ItemId);

                       if (product != null) { pn = product.Name; }

                            var categoryNane = "";

                        foreach (var cat in product.ProductCategories.ToList())
                            categoryNane += cat.Category.Name + " ";


                        var groupsModel = new ItemsCompatabilityModel { Id = x.Id, CategoryName=categoryNane, ItemName = pn, ItemId = x.ItemId, ItemIdPart = x.ItemIdPart, ItemIdPartName = pn1, Sku = product.Sku };
                        //categoryModel.Breadcrumb = x.GetFormattedBreadCrumb(_categoryService);
                        return groupsModel;
                    }),
                    Total = groups.TotalCount
                };
                return Json(gridModel);
            }
            catch (Exception ex) { return Json(new DataSourceResult()); }

        }
        [HttpPost]
        public virtual IActionResult DestroyItemRelationships(ItemsCompatabilityModel model)
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
        #endregion

        #region Groups 
        [HttpPost]
        public virtual IActionResult ListGroups(DataSourceRequest command, string searchGroupName)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();

            var query = _gpRepository.TableNoTracking.Where(x => x.Deleted == false);

            query = query.OrderBy(x => x.GroupName);


            if (!string.IsNullOrEmpty(searchGroupName))
                query = query.Where(x => x.GroupName.StartsWith(searchGroupName));

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

        [HttpPost]
        public virtual IActionResult AddGroupRelations(ICollection<int> selectedIds, int itemId, bool wizardBelongGroup, bool groupBelongWizard)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                foreach (var selectedIdenId in selectedIds)
                {
                    var relations = new  RelationsGroupsItems
                    {
                        GroupId = selectedIdenId,
                        ItemId = itemId,
                        Deleted = false, 
                    };

                    if (wizardBelongGroup == true)
                    {
                        relations.Direction = "B";
                    }
                    else  
                    {
                        relations.Direction = "A";
                    }
                    _rgpRepository.Insert(relations);
                }
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual IActionResult ListGroupsRelations(DataSourceRequest command, int itemId , bool currGroupBelongsWizard, bool currWizardBelongGroup)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();

            var groups = _gpRepository.TableNoTracking.Where(x => x.Deleted == false);
            var rGroups = _rgpRepository.TableNoTracking.Where(x => x.Deleted == false);


            var query = from g in groups
                        join r in rGroups on g.Id equals r.GroupId
                        where r.ItemId == itemId
                        orderby g.GroupName 
                         select new GroupsItemsModel  { ItemName = g.GroupName, GroupId= g.Id, ItemId= r.ItemId, Relationship = r.Direction,  GroupItemId=r.Id } ;

            if(currGroupBelongsWizard)
                query = query.Where(x => x.Relationship == "A").OrderBy(x=>x.ItemName);
            else
                query = query.Where(x => x.Relationship == "B").OrderBy(x => x.ItemName);

            var model = new PagedList<GroupsItemsModel>(query, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = model,
                Total = model.TotalCount
            };
            return Json(gridModel);
        }

        [HttpPost]
        public virtual IActionResult Deletegrouprelations(ICollection<int> selectedIds, int itemId, bool currGroupBelongsWizard, bool currWizardBelongGroup)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                foreach (var selectedIdenId in selectedIds)
                {
                    var relation = _gpiRepository.Table.Where(x => x.Id == selectedIdenId).FirstOrDefault();
                    relation.Deleted = true;
                    _gpiRepository.Update(relation);

                }
            }



                    return new NullJsonResult();
        }
        #endregion

        protected virtual List<int> GetChildCategoryIds(int parentCategoryId)
        {
            var categoriesIds = new List<int>();
            var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, true);
            foreach (var category in categories)
            {
                categoriesIds.Add(category.Id);
                categoriesIds.AddRange(GetChildCategoryIds(category.Id));
            }
            return categoriesIds;
        }


    }
}

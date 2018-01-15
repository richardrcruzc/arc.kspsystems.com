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
    public class PartsForItemController : BasePluginController
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

        public PartsForItemController(
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

        


        #region frontsearch



        public virtual IActionResult BrowserInventory(int rid, int cid)
        {
            ViewBag.Rid = rid;
            ViewBag.Cid = cid;

            return View("~/Plugins/Misc.ProductWizard/Views/BrowserInventory.cshtml" , rid);
        }


      [HttpPost]
        public virtual IActionResult FrontSearch(DataSourceRequest command,int rid, int cid)
        {
            var query = _productService.GetProductsByIds(new int[] {0});


            if (cid == 0)
            {

               // var products = _productService.SearchProducts(orderBy: ProductSortingEnum.NameDesc);
                //ItemsCompatability
                var ic = _iRepository.TableNoTracking.Where(x => x.ItemId == rid).Select(x => x.ItemId).ToList();
                //GroupsItems
                var gpi = _gpiRepository.TableNoTracking.Where(x => x.ItemId == rid).Select(x => x.GroupId).ToList();
                //RelationsGroupsItems>
                var rgp = _rgpRepository.TableNoTracking.Where(x => x.Direction == "B" && gpi.Contains(x.GroupId)).Select(x => x.ItemId).ToList();



                rgp.AddRange(ic);
                //var catergories
                //query = products.Where(x => ic.Contains(x.Id) || rgp.Contains(x.Id)) ;

                query = _productService.GetProductsByIds(rgp.ToArray());


            }
            else
            {

               var  tmp = _categoryService.GetProductCategoriesByCategoryId(cid).Select(x=>x.Product.Id);
                query = _productService.GetProductsByIds(tmp.ToArray());

            }

            var gridModel = new DataSourceResult
            {
                Data = query.Select(x =>
                {
                    //var pictures = _pictureService.GetPicturesByProductId(x.Id);
                    //var defaultPicture = pictures.FirstOrDefault();
                    //var thumb = _pictureService.GetPictureUrl(defaultPicture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                    //picture
                    var defaultProductPicture = _pictureService.GetPicturesByProductId(x.Id, 1).FirstOrDefault();
                    var thumb = _pictureService.GetPictureUrl(defaultProductPicture, 75, true);



                    var model = new PartsForThisItemModel
                    {
                        Id = x.Id,
                        Category = string.Join(",", x.ProductCategories.Select(y => y.Category.Name).ToArray()),
                        CategorySeo = x.ProductCategories.Select(y => y.Category.GetSeName()).FirstOrDefault(),
                        Manufacturer = string.Join(",", x.ProductManufacturers.Select(y => y.Manufacturer.Name).ToArray()),
                        PartNumber = x.Sku,
                        Price = x.Price,
                        ProductName = x.Name,
                        Qty = 1,
                        ThumbImageUrl = thumb,
                        SeName = x.GetSeName(),
                        OutOfStock = x.GetTotalStockQuantity() <= 0,

                    };
                    //categoryModel.Breadcrumb = x.GetFormattedBreadCrumb(_categoryService);
                    return model;
                }).OrderBy(y => y.Manufacturer).ThenBy(y => y.ProductName),
                Total = query.Count
            };
            return Json(gridModel);
        }
        #endregion 

       #region "Parts for this Item"
        [HttpPost]
        public virtual IActionResult PartsForThisItemList(DataSourceRequest command, int id)
        {
            string sqlPartForItem = string.Format(
                    "SELECT     ItemsCompatability.ItemIDPart, [product].Name as ProductName " +
"FROM         ItemsCompatability INNER JOIN[product] " +
"ON ItemsCompatability.ItemIDPart = [product].Id " +
"WHERE(ItemsCompatability.ItemID =  {0}) " +
"UNION " +
"SELECT[Groups-Items].ItemID, [product_1].Name " +
"FROM[Groups-Items] INNER JOIN[product] AS[product_1] " +
"ON[Groups-Items].ItemID = [product_1].id " +
"WHERE([Groups-Items].GroupID IN " +
"(SELECT     GroupID " +
"FROM[Relations-Groups-Items] " +
"WHERE(ItemID = {0}) AND(Direction = 'A'))) " +
"UNION " +
"SELECT[Relations-Groups-Items].ItemID, [product].Name " +
"FROM[product] INNER JOIN[Relations-Groups-Items] " +
"ON[product].id = [Relations-Groups-Items].ItemID " +
"WHERE([Relations-Groups-Items].Direction = 'B') " +
"AND([Relations-Groups-Items].GroupID IN " +
"(SELECT GroupID " +
"FROM          [Groups-Items] " +
"WHERE      (ItemID =  {0}))) ", id);

            var partForItem = _dbContext.SqlQuery<ProductNameModel>(sqlPartForItem).Select(x => x.ItemIDPart).ToList();
            var query = _productService.GetProductsByIds(partForItem.ToArray());
            var model =  query.Select(x => new CategoryModel
            {
                CatergorySeo = x.ProductCategories.Select(y => y.Category.GetSeName()).FirstOrDefault(),
                CatergoryName = x.ProductCategories.Select(y => y.Category.Name).FirstOrDefault()
            });

            var groupedBy = model.GroupBy(x => new {x.CatergoryName, x.CatergorySeo })
                .Select(y => new CategoryModel
                {
                    CatergorySeo = y.Key.CatergorySeo ,
                    CatergoryName = y.Key.CatergoryName 
                })
                .OrderBy(z=>z.CatergoryName)
                .ToList();
           
            return Json(model);



        }
        [HttpPost] 
        public virtual IActionResult PartsForThisItem(DataSourceRequest command, int id)
        {
            string sqlPartForItem = string.Format(
                    "SELECT     ItemsCompatability.ItemIDPart, [product].Name as ProductName " +
"FROM         ItemsCompatability INNER JOIN[product] " +
"ON ItemsCompatability.ItemIDPart = [product].Id " +
"WHERE(ItemsCompatability.ItemID =  {0}) " +
"UNION " +
"SELECT[Groups-Items].ItemID, [product_1].Name " +
"FROM[Groups-Items] INNER JOIN[product] AS[product_1] " +
"ON[Groups-Items].ItemID = [product_1].id " +
"WHERE([Groups-Items].GroupID IN " +
"(SELECT     GroupID " +
"FROM[Relations-Groups-Items] " +
"WHERE(ItemID = {0}) AND(Direction = 'A'))) " +
"UNION " +
"SELECT[Relations-Groups-Items].ItemID, [product].Name " +
"FROM[product] INNER JOIN[Relations-Groups-Items] " +
"ON[product].id = [Relations-Groups-Items].ItemID " +
"WHERE([Relations-Groups-Items].Direction = 'B') " +
"AND([Relations-Groups-Items].GroupID IN " +
"(SELECT GroupID " +
"FROM          [Groups-Items] " +
"WHERE      (ItemID =  {0}))) ", id);

            var partForItem = _dbContext.SqlQuery<ProductNameModel>(sqlPartForItem).Select(x => x.ItemIDPart).ToList();
            var query = _productService.GetProductsByIds(partForItem.ToArray());
            //query.FirstOrDefault().ProductCategories.FirstOrDefault().Category.Name
            //  var pictures = _pictureService.GetPicturesByProductId(product.Id);


            var gridModel = new DataSourceResult
            {
                Data = query.Select(x =>
                {
                    //var pictures = _pictureService.GetPicturesByProductId(x.Id);
                    //var defaultPicture = pictures.FirstOrDefault();
                    //var thumb = _pictureService.GetPictureUrl(defaultPicture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                    //picture
                    var defaultProductPicture = _pictureService.GetPicturesByProductId(x.Id, 1).FirstOrDefault();
                    var thumb = _pictureService.GetPictureUrl(defaultProductPicture, 75, true);



                    var model = new PartsForThisItemModel
                    {
                        Id = x.Id,
                        Category = string.Join(",", x.ProductCategories.Select(y => y.Category.Name).ToArray()),
                        CategorySeo = x.ProductCategories.Select(y => y.Category.GetSeName()).FirstOrDefault(),
                        Manufacturer = string.Join(",", x.ProductManufacturers.Select(y => y.Manufacturer.Name).ToArray()),
                        PartNumber = x.Sku,
                        Price = x.Price,
                        ProductName = x.Name,
                        Qty = 1,
                        ThumbImageUrl = thumb,
                        SeName = x.GetSeName(),
                        OutOfStock = x.GetTotalStockQuantity()   <= 0,

                    };
                    //categoryModel.Breadcrumb = x.GetFormattedBreadCrumb(_categoryService);
                    return model;
                }).OrderBy(y => y.Manufacturer).ThenBy(y => y.ProductName),
                Total = query.Count
            };
            return Json(gridModel);



        }

        #endregion
    }
}

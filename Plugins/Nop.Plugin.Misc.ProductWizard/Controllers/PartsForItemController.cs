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
using Nop.Services.Directory;
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
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Factories;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;

namespace Nop.Plugin.Misc.ProductWizard.Controllers
{
    public class PartsForItemController : BasePluginController
    {
        #region Fields

        private readonly IRepository<SpecificationAttributeOption> _specificationAttributeOptionRepository;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IRepository<ProductPicture> _productPictureRepository;
        private readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;

        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductModelFactory _productModelFactory;

        private readonly IStoreContext _storeContext;
        private readonly ICurrencyService _currencyService;

        private readonly IPriceFormatter _priceFormatter;
        private readonly IWebHelper _webHelper;
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
            IRepository<SpecificationAttributeOption> specificationAttributeOptionRepository,
            IRepository<AclRecord> aclRepository,
         IRepository<StoreMapping> storeMappingRepository,
         IRepository<ProductPicture> productPictureRepository,
         IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
            IRepository<LocalizedProperty> localizedPropertyRepository,
            IRepository<Product> productRepository,
            ISpecificationAttributeService specificationAttributeService,
            IProductModelFactory productModelFactory,
            IStoreContext storeContext,
            ICurrencyService currencyService,
             IPriceFormatter priceFormatter,
            IWebHelper webHelper,
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
            this._specificationAttributeOptionRepository = specificationAttributeOptionRepository;
            this._aclRepository= aclRepository;
        this._storeMappingRepository= storeMappingRepository;
         this._productPictureRepository= productPictureRepository;
         this._productSpecificationAttributeRepository= productSpecificationAttributeRepository;

            this._localizedPropertyRepository= localizedPropertyRepository;
            this._productRepository = productRepository;
            this._specificationAttributeService = specificationAttributeService;
            this._productModelFactory = productModelFactory;
            this._storeContext = storeContext;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            this._webHelper = webHelper;
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
        //public virtual IActionResult UrlRecordSlug(int productId, string productName, string partNumber)
        //{
        //    //return $"productId:{productId} productName:{productName} partNumber:{partNumber} Ok.";

        //    return RedirectToAction("ProductDetails","Product",new { productId = 45 });
        //}


        public virtual IActionResult BrowseInventory(int rid, int cid, Web.Models.Catalog.CatalogPagingFilteringModel command)
        { 
            var prodsInCategory = new List<int>(); 

            ViewBag.Rid = rid;
            ViewBag.Cid = cid;
            var allCategoryIds = new List<int>();

            var query = _productService.GetProductsByIds(new int[] { 0 });
            var category = new Category();
            if (cid == 0)
            {

                // var products = _productService.SearchProducts(orderBy: ProductSortingEnum.NameDesc);
                //ItemsCompatability
               var ic = _iRepository.TableNoTracking.Where(x => x.ItemId == rid && x.Deleted==false).Select(x => x.ItemIdPart).ToList();

                //GroupsItems
                var gpi = _gpiRepository.TableNoTracking.Where(x => x.ItemId == rid && x.Deleted==false).Select(x => x.GroupId).ToList();
             
                //RelationsGroupsItems>
                var rgp = _rgpRepository.TableNoTracking.Where(x => x.Direction == "B" && x.Deleted == false && gpi.Contains(x.GroupId)).Select(x => x.ItemId).ToList();
 
                rgp.AddRange(ic);
                //var catergories
                //query = products.Where(x => ic.Contains(x.Id) || rgp.Contains(x.Id)) ;
                // var distinct = rgp.Distinct();
                // query = _productService.GetProductsByIds(distinct.ToArray());

                prodsInCategory.AddRange(rgp);

                //var catergories

                var queryC = _productService.GetProductsByIds(rgp.ToArray()).Select(x => new { x.ProductCategories.FirstOrDefault().Category.Name, x.ProductCategories.FirstOrDefault().Category.Id }).ToList();

                //  var catergories = products.Where(x => ic.Contains(x.Id) || rgp.Contains(x.Id)).Select(x=> new { x.ProductCategories.FirstOrDefault().Category.Name, x.ProductCategories.FirstOrDefault().Category.Id}).ToList();
               // query.Insert(0, new { Name = "All Categories", Id = 0 });

                //var catergories = query.GroupBy(x => x.Name, x => x.Id, (key, g) => new { Id = key, Name = g.ToList() })
                // .Select(x => new { x.Name, x.Id }).ToList(); 


                var temp = from c in queryC
                           group c.Name by c.Id into g
                           select new { Name = g.FirstOrDefault(), Id = g.Key }; 

                var catergories = temp.Select(x => new { x.Name, x.Id }).ToList();


                allCategoryIds = catergories.Select(x => x.Id).ToList();

                if (allCategoryIds.Count > 0)
                    category = _categoryService.GetCategoryById(allCategoryIds.FirstOrDefault()); //


            }
            else
            { 

                var prodIds = _iRepository.TableNoTracking.Where(x => x.ItemId == rid && x.Deleted == false).Select(x => x.ItemIdPart).ToList();

                              
                foreach (var p in prodIds)
                {
                    var tp = _productService.GetProductById(p);
                    if (tp != null)
                    {
                        if (tp.ProductCategories.FirstOrDefault() != null && tp.ProductCategories.FirstOrDefault().Category.Id == cid)
                        {
                            prodsInCategory.Add(p);
                        }
                    }

                }
                

                query = _productService.GetProductsByIds(prodsInCategory.ToArray());
                if(query.FirstOrDefault()!=null && query.FirstOrDefault().ProductCategories.FirstOrDefault()!=null)
                category = query.FirstOrDefault().ProductCategories.FirstOrDefault().Category;
            }
            var allIds = query.Select(x => x.Id).ToList();

            //if(allCategoryIds.Count>0)
            // category = _categoryService.GetCategoryById(allCategoryIds.FirstOrDefault()); //
            //else
            //    category= query.FirstOrDefault().ProductCategories.FirstOrDefault().Category;

            if (category.Id == 0)
                return View("~/Plugins/Misc.ProductWizard/Views/BrowseInventory.cshtml", new CategoryModel());

            var model = new CategoryModel
            {
                Id = category.Id,
                Name = category.GetLocalized(x => x.Name),
                Description = category.GetLocalized(x => x.Description),
                MetaKeywords = category.GetLocalized(x => x.MetaKeywords),
                MetaDescription = category.GetLocalized(x => x.MetaDescription),
                MetaTitle = category.GetLocalized(x => x.MetaTitle),
                SeName = category.GetSeName(),
                PagingFilteringContext = new Web.Models.Catalog.CatalogPagingFilteringModel()
        };

            //sorting
            PrepareSortingOptions(model.PagingFilteringContext, command);
            //view mode
            PrepareViewModes(model.PagingFilteringContext, command);
            //page size
            PreparePageSizeOptions(model.PagingFilteringContext, command,
                category.AllowCustomersToSelectPageSize,
                category.PageSizeOptions,
                category.PageSize);

            //price ranges
            model.PagingFilteringContext.PriceRangeFilter.LoadPriceRangeFilters(category.PriceRanges, _webHelper, _priceFormatter);
            var selectedPriceRange = model.PagingFilteringContext.PriceRangeFilter.GetSelectedPriceRange(_webHelper, category.PriceRanges);
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.From.Value, _workContext.WorkingCurrency);

                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.To.Value, _workContext.WorkingCurrency);
            }


            //category breadcrumb
            if (_catalogSettings.CategoryBreadcrumbEnabled)
            {
                model.DisplayCategoryBreadcrumb = true;

                var breadcrumbCacheKey = string.Format(Web.Infrastructure.Cache.ModelCacheEventConsumer.CATEGORY_BREADCRUMB_KEY,
                    category.Id,
                    string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                    _storeContext.CurrentStore.Id,
                    _workContext.WorkingLanguage.Id);
                model.CategoryBreadcrumb = _cacheManager.Get(breadcrumbCacheKey, () =>
                    category
                    .GetCategoryBreadCrumb(_categoryService, _aclService, _storeMappingService)
                    .Select(catBr => new CategoryModel
                    {
                        Id = catBr.Id,
                        Name = catBr.GetLocalized(x => x.Name),
                        SeName = catBr.GetSeName()
                    })
                    .ToList()
                );
            }
            model.DisplayCategoryBreadcrumb = false;

            var pictureSize = _mediaSettings.CategoryThumbPictureSize;

            //subcategories
            var subCategoriesCacheKey = string.Format(Web.Infrastructure.Cache.ModelCacheEventConsumer.CATEGORY_SUBCATEGORIES_KEY,
                category.Id,
                pictureSize,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id,
                _workContext.WorkingLanguage.Id,
                _webHelper.IsCurrentConnectionSecured());

            model.SubCategories = _cacheManager.Get(subCategoriesCacheKey, () =>
                _categoryService.GetAllCategoriesByParentCategoryId(category.Id)
                .Select(x =>
                {
                    var subCatModel = new CategoryModel.SubCategoryModel
                    {
                        Id = x.Id,
                        Name = x.GetLocalized(y => y.Name),
                        SeName = x.GetSeName(),
                        Description = x.GetLocalized(y => y.Description)
                    };

                    //prepare picture model
                    var categoryPictureCacheKey = string.Format(Web.Infrastructure.Cache.ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    subCatModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(x.PictureId);
                        var pictureModel = new PictureModel
                        {
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                            Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), subCatModel.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), subCatModel.Name)
                        };
                        return pictureModel;
                    });

                    return subCatModel;
                })
                .ToList()
            );

            //featured products
            if (!_catalogSettings.IgnoreFeaturedProducts)
            {
                //We cache a value indicating whether we have featured products
                IPagedList<Product> featuredProducts = null;
                var cacheKey = string.Format(Web.Infrastructure.Cache.ModelCacheEventConsumer.CATEGORY_HAS_FEATURED_PRODUCTS_KEY, category.Id,
                    string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()), _storeContext.CurrentStore.Id);
                var hasFeaturedProductsCache = _cacheManager.Get<bool?>(cacheKey);
                if (!hasFeaturedProductsCache.HasValue)
                {
                    //no value in the cache yet
                    //let's load products and cache the result (true/false)
                    featuredProducts = _productService.SearchProducts(                        
                       categoryIds: new List<int> { category.Id },
                       storeId: _storeContext.CurrentStore.Id,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true) ;
                    featuredProducts.Where(item => query.Any(x => x.Id.Equals(item.Id)));
                   // featuredProducts.Where(x => allIds.Contains(x.Id));

                    hasFeaturedProductsCache = featuredProducts.TotalCount > 0;
                    _cacheManager.Set(cacheKey, hasFeaturedProductsCache, 60);
                }
                if (hasFeaturedProductsCache.Value && featuredProducts == null)
                {
                    //cache indicates that the category has featured products
                    //let's load them
                    featuredProducts = _productService.SearchProducts(
                      categoryIds: new List<int> { category.Id },
                       storeId: _storeContext.CurrentStore.Id,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true);
                    featuredProducts.Where(item => query.Any(x => x.Id.Equals(item.Id)));
                    //featuredProducts.Where(x => allIds.Contains(x.Id));
                }
                if (featuredProducts != null)
                {
                    model.FeaturedProducts = _productModelFactory.PrepareProductOverviewModels(featuredProducts, rid: rid).ToList();
                }
            }

            var categoryIds = new List<int>
            {
                category.Id
            };
            if (_catalogSettings.ShowProductsFromSubcategories)
            {
                //include subcategories
                categoryIds.AddRange(GetChildCategoryIds(category.Id));
            }
            //products
            IList<int> alreadyFilteredSpecOptionIds = model.PagingFilteringContext.SpecificationFilter.GetAlreadyFilteredSpecOptionIds(_webHelper);
            var productsOld = _productService.SearchProducts(out IList<int> filterableSpecificationAttributeOptionIds,
                true,
                categoryIds: allCategoryIds,
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                featuredProducts: _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false,
                priceMin: minPriceConverted,
                priceMax: maxPriceConverted,
                filteredSpecs: alreadyFilteredSpecOptionIds,
                orderBy: (ProductSortingEnum)command.OrderBy,
                pageIndex: command.PageNumber - 1,
                pageSize: command.PageSize);

            var products = SearchProductsUseLinq(ref filterableSpecificationAttributeOptionIds, true, command.PageNumber - 1, command.PageSize,
                null, prodsInCategory, 0, 0, 0,0,null,false,false, _catalogSettings.IncludeFeaturedProductsInNormalLists,
                minPriceConverted, maxPriceConverted, 0, null, false, false, false, false, false,new int[0], 0, alreadyFilteredSpecOptionIds,
                (ProductSortingEnum)command.OrderBy,false,true);




            //foreach (var p in query)
            //{
            //    products.Remove(p);

            //}
          //  products.Where(item => query.Any(x => x.Id.Equals(item.Id)));
            //products.Where(x => allIds.Contains(x.Id));

            model.Products = _productModelFactory.PrepareProductOverviewModels(products, rid: rid).ToList();

            model.PagingFilteringContext.LoadPagedList(products);

            //specs
            model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(alreadyFilteredSpecOptionIds,
                filterableSpecificationAttributeOptionIds != null ? filterableSpecificationAttributeOptionIds.ToArray() : null,
                _specificationAttributeService,
                _webHelper,
                _workContext,
                _cacheManager);

            return View("~/Plugins/Misc.ProductWizard/Views/BrowseInventory.cshtml" , model);
        }

        #region Utilities
        /// <summary>
        /// Search products using LINQ
        /// </summary>
        /// <param name="filterableSpecificationAttributeOptionIds">The specification attribute option identifiers applied to loaded products (all pages)</param>
        /// <param name="loadFilterableSpecificationAttributeOptionIds">A value indicating whether we should load the specification attribute option identifiers applied to loaded products (all pages)</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
        /// <param name="productType">Product type; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="markedAsNewOnly">A values indicating whether to load only products marked as "new"; "false" to load all records; "true" to load "marked as new" only</param>
        /// <param name="featuredProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers). 0 to load featured products only, 1 to load not featured products only, null to load all products</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="searchManufacturerPartNumber">A value indicating whether to search by a specified "keyword" in manufacturer part number</param>
        /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in product SKU</param>
        /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
        /// <param name="searchLocalizedValue">A value indicating whether to search in localizable values</param>
        /// <param name="allowedCustomerRolesIds">A list of allowed customer role identifiers (ACL)</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecs">Filtered product specification identifiers</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>Products</returns>
        protected virtual IPagedList<Product> SearchProductsUseLinq(ref IList<int> filterableSpecificationAttributeOptionIds,
            bool loadFilterableSpecificationAttributeOptionIds, int pageIndex, int pageSize, IList<int> categoryIds,
            IList<int> Ids,
            int manufacturerId, int storeId, int vendorId, int warehouseId, ProductType? productType,
            bool visibleIndividuallyOnly, bool markedAsNewOnly, bool? featuredProducts, decimal? priceMin, decimal? priceMax,
            int productTagId, string keywords, bool searchDescriptions, bool searchManufacturerPartNumber, bool searchSku,
            bool searchProductTags, bool searchLocalizedValue, int[] allowedCustomerRolesIds, int languageId,
            IList<int> filteredSpecs, ProductSortingEnum orderBy, bool showHidden, bool? overridePublished)
        {
            //products
            var query = _productRepository.Table;
            query = query.Where(p => !p.Deleted);
            if (!overridePublished.HasValue)
            {
                //process according to "showHidden"
                if (!showHidden)
                {
                    query = query.Where(p => p.Published);
                }
            }
            else if (overridePublished.Value)
            {
                //published only
                query = query.Where(p => p.Published);
            }
            else if (!overridePublished.Value)
            {
                //unpublished only
                query = query.Where(p => !p.Published);
            }
            if (visibleIndividuallyOnly)
            {
                query = query.Where(p => p.VisibleIndividually);
            }
            //The function 'CurrentUtcDateTime' is not supported by SQL Server Compact. 
            //That's why we pass the date value
            var nowUtc = DateTime.UtcNow;
            if (markedAsNewOnly)
            {
                query = query.Where(p => p.MarkAsNew);
                query = query.Where(p =>
                    (!p.MarkAsNewStartDateTimeUtc.HasValue || p.MarkAsNewStartDateTimeUtc.Value < nowUtc) &&
                    (!p.MarkAsNewEndDateTimeUtc.HasValue || p.MarkAsNewEndDateTimeUtc.Value > nowUtc));
            }
            if (productType.HasValue)
            {
                var productTypeId = (int)productType.Value;
                query = query.Where(p => p.ProductTypeId == productTypeId);
            }

            if (priceMin.HasValue)
            {
                //min price
                query = query.Where(p => p.Price >= priceMin.Value);
            }
            if (priceMax.HasValue)
            {
                //max price
                query = query.Where(p => p.Price <= priceMax.Value);
            }
            if (!showHidden)
            {
                //available dates
                query = query.Where(p =>
                    (!p.AvailableStartDateTimeUtc.HasValue || p.AvailableStartDateTimeUtc.Value < nowUtc) &&
                    (!p.AvailableEndDateTimeUtc.HasValue || p.AvailableEndDateTimeUtc.Value > nowUtc));
            }

            //searching by keyword
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                query = from p in query
                        join lp in _localizedPropertyRepository.Table on p.Id equals lp.EntityId into p_lp
                        from lp in p_lp.DefaultIfEmpty()
                        from pt in p.ProductTags.DefaultIfEmpty()
                        where (p.Name.Contains(keywords)) ||
                              (searchDescriptions && p.ShortDescription.Contains(keywords)) ||
                              (searchDescriptions && p.FullDescription.Contains(keywords)) ||
                              //manufacturer part number
                              (searchManufacturerPartNumber && p.ManufacturerPartNumber == keywords) ||
                              //SKU (exact match)
                              (searchSku && p.Sku == keywords) ||
                              //product tags (exact match)
                              (searchProductTags && pt.Name == keywords) ||
                              //localized values
                              (searchLocalizedValue && lp.LanguageId == languageId && lp.LocaleKeyGroup == "Product" &&
                               lp.LocaleKey == "Name" && lp.LocaleValue.Contains(keywords)) ||
                              (searchDescriptions && searchLocalizedValue && lp.LanguageId == languageId &&
                               lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "ShortDescription" &&
                               lp.LocaleValue.Contains(keywords)) ||
                              (searchDescriptions && searchLocalizedValue && lp.LanguageId == languageId &&
                               lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "FullDescription" &&
                               lp.LocaleValue.Contains(keywords))
                        select p;
            }

            if (!showHidden && !_catalogSettings.IgnoreAcl)
            {
                //ACL (access control list)
                query = from p in query
                        join acl in _aclRepository.Table
                            on new { c1 = p.Id, c2 = "Product" } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into p_acl
                        from acl in p_acl.DefaultIfEmpty()
                        where !p.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
                        select p;
            }

            if (storeId > 0 && !_catalogSettings.IgnoreStoreLimitations)
            {
                //Store mapping
                query = from p in query
                        join sm in _storeMappingRepository.Table
                            on new { c1 = p.Id, c2 = "Product" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into p_sm
                        from sm in p_sm.DefaultIfEmpty()
                        where !p.LimitedToStores || storeId == sm.StoreId
                        select p;
            }

            //Ids filtering
            
                if (Ids != null && Ids.Any())
            {
                query = from p in query
                        where Ids.Contains(p.Id) 
                        select p;
            }

            //category filtering
            if (categoryIds != null && categoryIds.Any())
            {
                query = from p in query
                        from pc in p.ProductCategories.Where(pc => categoryIds.Contains(pc.CategoryId))
                        where (!featuredProducts.HasValue || featuredProducts.Value == pc.IsFeaturedProduct)
                        select p;
            }

            //manufacturer filtering
            if (manufacturerId > 0)
            {
                query = from p in query
                        from pm in p.ProductManufacturers.Where(pm => pm.ManufacturerId == manufacturerId)
                        where (!featuredProducts.HasValue || featuredProducts.Value == pm.IsFeaturedProduct)
                        select p;
            }

            //vendor filtering
            if (vendorId > 0)
            {
                query = query.Where(p => p.VendorId == vendorId);
            }

            //warehouse filtering
            if (warehouseId > 0)
            {
                var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;
                query = query.Where(p =>
                        //"Use multiple warehouses" enabled
                        //we search in each warehouse
                        (p.ManageInventoryMethodId == manageStockInventoryMethodId &&
                         p.UseMultipleWarehouses &&
                         p.ProductWarehouseInventory.Any(pwi => pwi.WarehouseId == warehouseId))
                        ||
                        //"Use multiple warehouses" disabled
                        //we use standard "warehouse" property
                        ((p.ManageInventoryMethodId != manageStockInventoryMethodId ||
                          !p.UseMultipleWarehouses) &&
                         p.WarehouseId == warehouseId));
            }

            //related products filtering
            //if (relatedToProductId > 0)
            //{
            //    query = from p in query
            //            join rp in _relatedProductRepository.Table on p.Id equals rp.ProductId2
            //            where (relatedToProductId == rp.ProductId1)
            //            select p;
            //}

            //tag filtering
            if (productTagId > 0)
            {
                query = from p in query
                        from pt in p.ProductTags.Where(pt => pt.Id == productTagId)
                        select p;
            }

            //get filterable specification attribute option identifier
            if (loadFilterableSpecificationAttributeOptionIds)
            {
                var querySpecs = from p in query
                                 join psa in _productSpecificationAttributeRepository.Table on p.Id equals psa.ProductId
                                 where psa.AllowFiltering
                                 select psa.SpecificationAttributeOptionId;
                //only distinct attributes
                filterableSpecificationAttributeOptionIds = querySpecs.Distinct().ToList();
            }

            //search by specs
            if (filteredSpecs != null && filteredSpecs.Any())
            {
                var filteredAttributes = _specificationAttributeOptionRepository.Table
                    .Where(sao => filteredSpecs.Contains(sao.Id)).Select(sao => sao.SpecificationAttributeId).Distinct();

                query = query.Where(p => !filteredAttributes.Except
                (
                    _specificationAttributeOptionRepository.Table.Where(
                            sao => p.ProductSpecificationAttributes.Where(
                                    psa => psa.AllowFiltering && filteredSpecs.Contains(psa.SpecificationAttributeOptionId))
                                .Select(psa => psa.SpecificationAttributeOptionId).Contains(sao.Id))
                        .Select(sao => sao.SpecificationAttributeId).Distinct()
                ).Any());
            }

            //only distinct products (group by ID)
            //if we use standard Distinct() method, then all fields will be compared (low performance)
            //it'll not work in SQL Server Compact when searching products by a keyword)
            query = from p in query
                    group p by p.Id
                into pGroup
                    orderby pGroup.Key
                    select pGroup.FirstOrDefault();

            //sort products
            if (orderBy == ProductSortingEnum.Position && categoryIds != null && categoryIds.Any())
            {
                //category position
                var firstCategoryId = categoryIds[0];
                query = query.OrderBy(p =>
                    p.ProductCategories.FirstOrDefault(pc => pc.CategoryId == firstCategoryId).DisplayOrder);
            }
            else if (orderBy == ProductSortingEnum.Position && manufacturerId > 0)
            {
                //manufacturer position
                query =
                    query.OrderBy(p =>
                        p.ProductManufacturers.FirstOrDefault(pm => pm.ManufacturerId == manufacturerId).DisplayOrder);
            }
            else if (orderBy == ProductSortingEnum.Position)
            {
                //otherwise sort by name
                query = query.OrderBy(p => p.Name);
            }
            else if (orderBy == ProductSortingEnum.NameAsc)
            {
                //Name: A to Z
                query = query.OrderBy(p => p.Name);
            }
            else if (orderBy == ProductSortingEnum.NameDesc)
            {
                //Name: Z to A
                query = query.OrderByDescending(p => p.Name);
            }
            else if (orderBy == ProductSortingEnum.PriceAsc)
            {
                //Price: Low to High
                query = query.OrderBy(p => p.Price);
            }
            else if (orderBy == ProductSortingEnum.PriceDesc)
            {
                //Price: High to Low
                query = query.OrderByDescending(p => p.Price);
            }
            else if (orderBy == ProductSortingEnum.CreatedOn)
            {
                //creation date
                query = query.OrderByDescending(p => p.CreatedOnUtc);
            }
            else
            {
                //actually this code is not reachable
                query = query.OrderBy(p => p.Name);
            }

            var products = new PagedList<Product>(query, pageIndex, pageSize);

            //return products
            return products;
        }


        //var totalRecords = pTotalRecords.Value != DBNull.Value ? Convert.ToInt32(pTotalRecords.Value) : 0;
        //    return new PagedList<Product>(products, pageIndex, pageSize, totalRecords);

        /// <summary>
        /// Get child category identifiers
        /// </summary>
        /// <param name="parentCategoryId">Parent category identifier</param>
        /// <returns>List of child category identifiers</returns>
        protected virtual List<int> GetChildCategoryIds(int parentCategoryId)
        {
            var cacheKey = string.Format(Web.Infrastructure.Cache.ModelCacheEventConsumer.CATEGORY_CHILD_IDENTIFIERS_MODEL_KEY,
                parentCategoryId,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return _cacheManager.Get(cacheKey, () =>
            {
                var categoriesIds = new List<int>();
                var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId);
                foreach (var category in categories)
                {
                    categoriesIds.Add(category.Id);
                    categoriesIds.AddRange(GetChildCategoryIds(category.Id));
                }
                return categoriesIds;
            });
        }

        #endregion
        /// <summary>
        /// Prepare page size options
        /// </summary>
        /// <param name="pagingFilteringModel">Catalog paging filtering model</param>
        /// <param name="command">Catalog paging filtering command</param>
        /// <param name="allowCustomersToSelectPageSize">Are customers allowed to select page size?</param>
        /// <param name="pageSizeOptions">Page size options</param>
        /// <param name="fixedPageSize">Fixed page size</param>
        public virtual void PreparePageSizeOptions(Web.Models.Catalog.CatalogPagingFilteringModel pagingFilteringModel, 
            Web.Models.Catalog.CatalogPagingFilteringModel command,
            bool allowCustomersToSelectPageSize, string pageSizeOptions, int fixedPageSize)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException(nameof(pagingFilteringModel));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (command.PageNumber <= 0)
            {
                command.PageNumber = 1;
            }
            pagingFilteringModel.AllowCustomersToSelectPageSize = false;
            if (allowCustomersToSelectPageSize && pageSizeOptions != null)
            {
                var pageSizes = pageSizeOptions.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (pageSizes.Any())
                {
                    // get the first page size entry to use as the default (category page load) or if customer enters invalid value via query string
                    if (command.PageSize <= 0 || !pageSizes.Contains(command.PageSize.ToString()))
                    {
                        if (int.TryParse(pageSizes.FirstOrDefault(), out int temp))
                        {
                            if (temp > 0)
                            {
                                command.PageSize = temp;
                            }
                        }
                    }

                    var currentPageUrl = _webHelper.GetThisPageUrl(true);
                    var sortUrl = _webHelper.ModifyQueryString(currentPageUrl, "pagesize={0}", null);
                    sortUrl = _webHelper.RemoveQueryString(sortUrl, "pagenumber");

                    foreach (var pageSize in pageSizes)
                    {
                        if (!int.TryParse(pageSize, out int temp))
                        {
                            continue;
                        }
                        if (temp <= 0)
                        {
                            continue;
                        }

                        pagingFilteringModel.PageSizeOptions.Add(new SelectListItem
                        {
                            Text = pageSize,
                            Value = string.Format(sortUrl, pageSize),
                            Selected = pageSize.Equals(command.PageSize.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        });
                    }

                    if (pagingFilteringModel.PageSizeOptions.Any())
                    {
                        pagingFilteringModel.PageSizeOptions = pagingFilteringModel.PageSizeOptions.OrderBy(x => int.Parse(x.Text)).ToList();
                        pagingFilteringModel.AllowCustomersToSelectPageSize = true;

                        if (command.PageSize <= 0)
                        {
                            command.PageSize = int.Parse(pagingFilteringModel.PageSizeOptions.First().Text);
                        }
                    }
                }
            }
            else
            {
                //customer is not allowed to select a page size
                command.PageSize = fixedPageSize;
            }

            //ensure pge size is specified
            if (command.PageSize <= 0)
            {
                command.PageSize = fixedPageSize;
            }
        }

        /// <summary>
        /// Prepare view modes
        /// </summary>
        /// <param name="pagingFilteringModel">Catalog paging filtering model</param>
        /// <param name="command">Catalog paging filtering command</param>
        public virtual void PrepareViewModes(Web.Models.Catalog.CatalogPagingFilteringModel pagingFilteringModel, 
            Web.Models.Catalog.CatalogPagingFilteringModel command)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException(nameof(pagingFilteringModel));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            pagingFilteringModel.AllowProductViewModeChanging = _catalogSettings.AllowProductViewModeChanging;

            var viewMode = !string.IsNullOrEmpty(command.ViewMode)
                ? command.ViewMode
                : _catalogSettings.DefaultViewMode;
            pagingFilteringModel.ViewMode = viewMode;
            if (pagingFilteringModel.AllowProductViewModeChanging)
            {
                var currentPageUrl = _webHelper.GetThisPageUrl(true);
                //grid
                pagingFilteringModel.AvailableViewModes.Add(new SelectListItem
                {
                    Text = _localizationService.GetResource("Catalog.ViewMode.Grid"),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "viewmode=grid", null),
                    Selected = viewMode == "grid"
                });
                //list
                pagingFilteringModel.AvailableViewModes.Add(new SelectListItem
                {
                    Text = _localizationService.GetResource("Catalog.ViewMode.List"),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "viewmode=list", null),
                    Selected = viewMode == "list"
                });
            }
        }
        /// <summary>
        /// Prepare sorting options
        /// </summary>
        /// <param name="pagingFilteringModel">Catalog paging filtering model</param>
        /// <param name="command">Catalog paging filtering command</param>
        public virtual void PrepareSortingOptions(Web.Models.Catalog.CatalogPagingFilteringModel pagingFilteringModel, 
            Web.Models.Catalog.CatalogPagingFilteringModel command)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException(nameof(pagingFilteringModel));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var allDisabled = _catalogSettings.ProductSortingEnumDisabled.Count == Enum.GetValues(typeof(ProductSortingEnum)).Length;
            pagingFilteringModel.AllowProductSorting = _catalogSettings.AllowProductSorting && !allDisabled;

            var activeOptions = Enum.GetValues(typeof(ProductSortingEnum)).Cast<int>()
                .Except(_catalogSettings.ProductSortingEnumDisabled)
                .Select((idOption) => new KeyValuePair<int, int>(idOption, _catalogSettings.ProductSortingEnumDisplayOrder.TryGetValue(idOption, out int order) ? order : idOption))
                .OrderBy(x => x.Value);
            if (command.OrderBy == null)
                command.OrderBy = allDisabled ? 0 : activeOptions.First().Key;

            if (pagingFilteringModel.AllowProductSorting)
            {
                foreach (var option in activeOptions)
                {
                    var currentPageUrl = _webHelper.GetThisPageUrl(true);
                    var sortUrl = _webHelper.ModifyQueryString(currentPageUrl, "orderby=" + (option.Key).ToString(), null);
                    var sortValue = ((ProductSortingEnum)option.Key).GetLocalizedEnum(_localizationService, _workContext);
                    pagingFilteringModel.AvailableSortOptions.Add(new SelectListItem
                    {
                        Text = sortValue,
                        Value = sortUrl,
                        Selected = option.Key == command.OrderBy
                    });
                }
            }
        }
        [HttpPost]
        public virtual IActionResult FrontSearch(DataSourceRequest command, int rid, int cid)
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
                var distinct = rgp.Distinct();
                query = _productService.GetProductsByIds(distinct.ToArray());


            }
            else
            {

               var  tmp = _categoryService.GetProductCategoriesByCategoryId(cid).Select(x=>x.Product.Id);
                var distinct = tmp.Distinct();

                query = _productService.GetProductsByIds(distinct.ToArray());

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
                        ProductName = _productService.GetNameRid(x, rid:rid),
                        Qty = 1,
                        ThumbImageUrl = thumb,
                        SeName = _productService.GetUrlRid(x, rid),
                        OutOfStock = x.GetTotalStockQuantity() <= 0,
                        NewSeName = _productService.GetUrlRid(x)


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


                    var newSeName = System.Net.WebUtility.UrlDecode($"{x.Id}/{x.GetSeName()}-{x.Sku}");

                    var model = new PartsForThisItemModel
                    {
                        Id = x.Id,
                        Category = string.Join(",", x.ProductCategories.Select(y => y.Category.Name).ToArray()),
                        CategorySeo = x.ProductCategories.Select(y => y.Category.GetSeName()).FirstOrDefault(),
                        Manufacturer = string.Join(",", x.ProductManufacturers.Select(y => y.Manufacturer.Name).ToArray()),
                        PartNumber = x.Sku,
                        Price = x.Price,
                        ProductName = _productService.GetNameRid(x,0), //x.Name,
                        Qty = 1,
                        ThumbImageUrl = thumb,
                        SeName = _productService.GetUrlRid(x,0), // x.GetSeName(),
                        OutOfStock = x.GetTotalStockQuantity()   <= 0,
                        NewSeName = _productService.GetUrlRid(x),

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

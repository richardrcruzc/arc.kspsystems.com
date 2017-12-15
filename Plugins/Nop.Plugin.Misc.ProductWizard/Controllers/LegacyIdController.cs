using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
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
    public class LegacyIdController : BasePluginController
    {
        #region Fields
        private readonly IRepository<LegacyId> _liRepository;
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

        public LegacyIdController(
            IRepository<LegacyId> liRepository,
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
            this._liRepository = liRepository;
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

        public virtual IActionResult Index(int id)
        {
           
            return RedirectToAction("List");
        }

        public virtual IActionResult List(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            ViewBag.ItemId = id;

            var itemName = _productService.GetProductById(id).Name;

            ViewBag.ItemName = itemName;

            return View("~/Plugins/Misc.ProductWizard/Views/LegacyId/List.cshtml" );
        }

        [HttpPost]
        public virtual IActionResult List(DataSourceRequest command, int itemId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();

            ViewBag.ItemId = itemId;

            var itemName = _productService.GetProductById(itemId).Name;

            ViewBag.ItemName = itemName;

            var query = _liRepository.TableNoTracking               
                .Where(x=>x.Deleted==false && x.ItemId == itemId)
                 .OrderBy(x => x.LegacyCode);
             
             
            var legacys= new PagedList<LegacyId>(query, command.Page - 1, command.PageSize);
             
            var gridModel = new DataSourceResult
            {
                Data = legacys.Select(x => new LegacyIdModel { itemId = x.ItemId, Id = x.Id, LegacyCode = x.LegacyCode }),
                Total = legacys.TotalCount
            };
            return Json(gridModel);
        }





        #endregion

        #region Create / Edit / Delete
        #region LegacyId
        [HttpPost]
        public virtual IActionResult Create(LegacyIdModel model, int itemId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var legacyId = new LegacyId { LegacyCode = model.LegacyCode, ItemId= model.itemId };
             
                _liRepository.Insert(legacyId);

            var legacys = new List<LegacyId>();
            legacys.Add(legacyId);

            var gridModel = new DataSourceResult
            {
                Data = legacys.Select(x => new LegacyIdModel { itemId = x.ItemId, Id = x.Id, LegacyCode = x.LegacyCode }), 
               
                Total = 0 //groups.TotalCount
            };

            return Json(gridModel);
        }
        
        [HttpPost]
        public virtual IActionResult Update(LegacyIdModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var entity = _liRepository.Table.Where(x => x.Id == model.Id).FirstOrDefault();
            if (entity == null)
                return Json(model);
            entity.LegacyCode = model.LegacyCode;

            _liRepository.Update(entity);


            return Json(model);
        }
         
        [HttpPost]
        public virtual IActionResult Destroy(LegacyIdModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();


            var itemDelete = _liRepository.Table.Where(x => x.Id == model.Id).FirstOrDefault();
            if (itemDelete != null)
            {
                itemDelete.Deleted = true;
                _liRepository.Update(itemDelete);
            }



            return new NullJsonResult();
        }
         
        #endregion

        #endregion
         

    }
}

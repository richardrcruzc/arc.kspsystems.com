using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Data;
using Nop.Plugin.Misc.ProductWizard.Domain;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ProductWizard.Controllers
{
    public class FrontSearchController: BasePluginController
    {
        private readonly ICategoryService _categoryService;
        private readonly IDbContext _dbContext;
        private readonly IRepository<Groups> _gpRepository;
        private readonly IRepository<GroupsItems> _gpiRepository;
        private readonly IRepository<RelationsGroupsItems> _rgpRepository;
        private readonly IRepository<ItemsCompatability> _iRepository;
        private readonly IRepository<LegacyId> _lRepository;

        private readonly IManufacturerService _manufacturerService;

        private readonly IProductService _productService;

        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _cacheManager;
        private readonly ISettingService _settingService;
        private readonly IPictureService _pictureService;

        public FrontSearchController(
             ICategoryService categoryService,
        IManufacturerService manufacturerService,
            IRepository<LegacyId> lRepository,
            IDbContext dbContext,
            IProductService productService,
             IRepository<ItemsCompatability> iRepository,
            IRepository<Groups> gpRepository,
              IRepository<GroupsItems> gpiRepository,
                IRepository<RelationsGroupsItems> rgpRepository,
            IStoreContext storeContext,
            IStaticCacheManager cacheManager,
            ISettingService settingService,
            IPictureService pictureService)
        {
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._lRepository = lRepository;
            this._dbContext = dbContext;
            this._productService = productService;
            this._iRepository = iRepository;
            this._gpRepository = gpRepository;
            this._gpiRepository = gpiRepository;
            this._rgpRepository = rgpRepository;
            this._storeContext = storeContext;
            this._cacheManager = cacheManager;
            this._settingService = settingService;
            this._pictureService = pictureService;
        }

        public virtual IActionResult GetBrands()
        {
            var brands = _manufacturerService.GetAllManufacturers().OrderBy(x=>x.Name).Select(x=> new {x.Name,  x.Id });
            return Json(brands);
        }

        public virtual IActionResult GetCopiers(int id)
        {
            if (id <= 0)
                return null;

            var IsKonica = _manufacturerService.GetManufacturerById(id).Name.StartsWith("Konica Minolta");            

           var copiers = _categoryService.GetAllCategories(categoryName: "Copier").ToList();
            var accessories = _categoryService.GetAllCategories(categoryName: "Accessories").ToList();

           // copiers.AddRange(accessories);

             var caterogiesIds = copiers.Select(x =>  x.Id  ).ToList() ;

            var products = _productService.SearchProducts(categoryIds: caterogiesIds , manufacturerId: id, orderBy:  ProductSortingEnum.NameDesc);

            var model = products
                .OrderBy(x=>x.Name.Replace(x.ProductManufacturers.FirstOrDefault().Manufacturer.Name + " ", ""))
                .Select(x => new {Description=x.Name.Replace(x.ProductManufacturers.FirstOrDefault().Manufacturer.Name+" ",""), x.Id }).ToList();

            //if (IsKonica)
            //    model = products.Where(x => !x.Name.Contains("Konica Minolta")).Select(x => new { Description = x.Name, x.Id }).ToList();

            //if (IsKonica)
            //{
            //    foreach (var konica in model)
            //    {
            //        if (konica.Description.Contains("Konica Minolta"))
            //        {
            //            model.Remove(konica);
            //        }
            //    }
            //}
            return Json(model.OrderBy(x=>x.Description).ToList());
        }

        public virtual IActionResult GetCategories(int id)
        {
             //ItemsCompatability
            var ic = _iRepository.TableNoTracking.Where(x => x.ItemId == id&& x.Deleted==false).Select(x=>x.ItemIdPart).ToList();

            //GroupsItems
            var gpi = _gpiRepository.TableNoTracking.Where(x => x.ItemId == id && x.Deleted == false).Select(x=>x.GroupId).ToList();
            //RelationsGroupsItems>
            var rgp = _rgpRepository.TableNoTracking.Where(x=>x.Direction=="B" && x.Deleted == false && gpi.Contains(x.GroupId)).Select(x=>x.ItemId).ToList();

            rgp.AddRange(ic);

            //var catergories

            var query = _productService.GetProductsByIds(rgp.ToArray()).Select(x => new { x.ProductCategories.FirstOrDefault().Category.Name, x.ProductCategories.FirstOrDefault().Category.Id }).ToList();

         

            var temp = from c in query
                       group c.Name by c.Id into g
                       select new { Name = g.FirstOrDefault(), Id =g.Key }; 


            var catergories = temp.OrderBy(x => x.Name).Select(x => new { x.Name, x.Id }).ToList();

            catergories.Insert(0, new { Name = "All Categories", Id = 0 });

            return Json(catergories);
        }

    }
}


/*
 
    
            var query = _productService.GetProductsByIds(rgp.ToArray()).Select(x => new { x.ProductCategories.FirstOrDefault().Category.Name, x.ProductCategories.FirstOrDefault().Category.Id }).ToList();

          //  var catergories = products.Where(x => ic.Contains(x.Id) || rgp.Contains(x.Id)).Select(x=> new { x.ProductCategories.FirstOrDefault().Category.Name, x.ProductCategories.FirstOrDefault().Category.Id}).ToList();
            query.Insert(0, new {Name= "All Categories", Id=0 });

            var catergories = query.GroupBy(x => x.Name, x => x.Id, (key, g) => new { Id = key, Name = g.ToList() })
               .Select(x => new { x.Name, x.Id }).ToList();
            //   var catergories = _productService.GetProductsByIds(rgp.ToArray())
            //    .GroupBy(x => x.ProductCategories.FirstOrDefault().Category.Id,
            //    x => x.ProductCategories.FirstOrDefault().Category.Name, (key, g) => new { Id = key, Name = g.ToList() })

            //.Select(x => new { Name = x.Name, Id = x.Id }).ToList();

 

     */

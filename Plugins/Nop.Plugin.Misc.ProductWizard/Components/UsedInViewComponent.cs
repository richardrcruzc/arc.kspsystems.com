using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Data;
using Nop.Plugin.Misc.ProductWizard.Domain;
using Nop.Plugin.Misc.ProductWizard.Models;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Services.Seo;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ProductWizard.Components
{
    [ViewComponent(Name = "UsedInProduct")]
    public class UsedInViewComponent : NopViewComponent 
    {
        private readonly IDbContext _dbContext;
        private readonly IRepository<Groups> _gpRepository;
        private readonly IRepository<GroupsItems> _gpiRepository;
        private readonly IRepository<RelationsGroupsItems> _rgpRepository;
        private readonly IRepository<ItemsCompatability> _iRepository;
        private readonly IRepository<LegacyId> _lRepository;
        

        private readonly IProductService _productService;

        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _cacheManager;
        private readonly ISettingService _settingService;
        private readonly IPictureService _pictureService;

        public UsedInViewComponent(
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
        public IViewComponentResult Invoke(int id, int display)
        {

            var rgp = _rgpRepository.TableNoTracking.OrderBy(x => x.Id).Where(x => !x.Deleted && x.ItemId == id && x.Direction == "B").Select(x => x.GroupId);

            var ic = _iRepository.TableNoTracking.OrderBy(x => x.Id).Where(x => !x.Deleted && x.ItemIdPart == id).Select(x => x.ItemId); ;


            var gi = _gpiRepository.TableNoTracking.OrderBy(x => x.Id).Where(x => !x.Deleted && rgp.Contains(x.GroupId)).Select(x => x.ItemId);

            var gi1 = _gpiRepository.TableNoTracking.OrderBy(x => x.Id).Where(x => !x.Deleted && x.ItemId == id).Select(x => x.GroupId);

            var rgp1 = _rgpRepository.TableNoTracking.OrderBy(x => x.Id).Where(x => !x.Deleted && x.Direction == "A" && gi1.Contains(x.GroupId)).Select(x => x.ItemId); ;


            var tmp = ic.Concat(gi).Concat(rgp1).Distinct().ToArray();


                       var sqlString =string.Format(
                          "SELECT     "+
            "ItemsCompatability.ItemID, [product].Name " +
"FROM         ItemsCompatability INNER JOIN " +
"[product] ON ItemsCompatability.ItemID = product.Id " +
"WHERE(ItemsCompatability.ItemIDPart = {0}) " +
"UNION " +
"SELECT[Groups-Items].ItemID, product_1.Name " +
"FROM[Groups-Items] INNER JOIN " +
"[product] AS product_1 ON[Groups-Items].ItemID = product_1.Id " +
"WHERE([Groups-Items].GroupID IN " +
"(SELECT GroupID " +
"FROM          [Relations-Groups-Items] " +
"  WHERE      (ItemID = {0}) AND(Direction = 'B')))  " +
"  UNION SELECT[Relations-Groups-Items].ItemID, [product].Name " +
"FROM[product] INNER JOIN[Relations-Groups-Items]   ON product.Id = [Relations-Groups-Items].ItemID " +
"WHERE     ([Relations-Groups-Items].Direction = 'A') AND([Relations-Groups-Items].GroupID IN " +
"(SELECT GroupID " +
"FROM          [Groups-Items] " +
"   WHERE      (ItemID = {0}))) ", id);

            //  var relatedProducts =  _dbContext.SqlQuery<List<ProductNameModel>>(sqlString).ToList();

            var legacy = _lRepository.TableNoTracking.Where(x => x.ItemId == id);
            
            var product =  _productService.GetProductById(id);
           
            var model = new ProductExtModel();
            model.Id = id;
            model.LegacyIdModel = _lRepository.TableNoTracking.Where(x => x.ItemId == id).Select(x=> new LegacyIdModel { itemId = x.ItemId, LegacyCode=x.LegacyCode, LegacyName=x.LegacyCode }).ToList();

            if (display == 0)
            {

                model.PartNumber = product.Sku;

                var usedIn = _productService.GetProductsByIds(tmp).Select(x => new UsedInModel { ProductName = _productService.GetNameRid(x,0), Id = x.Id, SeName = _productService.GetUrlRid(x) }).ToList();

                var productCategories = string.Empty;
                foreach (var c in product.ProductCategories.ToList())
                    model.CategoryModel.Add(new CategoryModel { CatergoryName = c.Category.Name });


                model.UsedInModel = usedIn;

                return View("~/Plugins/Misc.ProductWizard/Views/UsedIn.cshtml", model);
            }
            else
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


                var isCopier = false;
                foreach (var c in product.ProductCategories.ToList())
                    if (c.Category.Name.ToLower() == "copier" || c.Category.Name.ToLower() == "Accessories")
                        isCopier = true;

                model.IsCopier = isCopier;
                model.FullDescription = product.FullDescription;
                if (!isCopier)
                {
                    model.PartForItem = _productService.GetProductsByIds(partForItem.ToArray()).Select(x => new UsedInModel { ProductName = _productService.GetNameRid(x,0), Id = x.Id, SeName = _productService.GetUrlRid(x) }).ToList();

                }
                else
                {
                    string sqlPartForItem1 = string.Format(
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

                    var partForItem1 = _dbContext.SqlQuery<ProductNameModel>(sqlPartForItem1).Select(x => x.ItemIDPart).ToList();
                    var query = _productService.GetProductsByIds(partForItem.ToArray());

                    var modelCtegory = query.Select(x => new CategoryModel
                    {
                        CatergorySeo = x.ProductCategories.Select(y => y.Category.GetSeName()).FirstOrDefault(),
                        CatergoryName = x.ProductCategories.Select(y => y.Category.Name).FirstOrDefault()
                    });

                    var groupedBy = modelCtegory.GroupBy(x => new { x.CatergoryName, x.CatergorySeo })
                        .Select(y => new CategoryModel
                        {
                            CatergorySeo = y.Key.CatergorySeo,
                            CatergoryName = y.Key.CatergoryName
                        })
                        .OrderBy(z => z.CatergoryName)
                        .ToList();
                    model.CategoryModel = groupedBy;
                }





                return View("~/Plugins/Misc.ProductWizard/Views/UsedInAdditional.cshtml", model);
            }

        }
        }
    }

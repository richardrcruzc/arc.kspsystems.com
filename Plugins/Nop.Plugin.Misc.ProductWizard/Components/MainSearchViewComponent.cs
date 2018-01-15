using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Data;
using Nop.Plugin.Misc.ProductWizard.Domain;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ProductWizard.Components
{
    public class MainSearchViewComponent : NopViewComponent
    {
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

        public MainSearchViewComponent(
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
        public IViewComponentResult Invoke()
        {
           

            return View("~/Plugins/Misc.ProductWizard/Views/MainSearch.cshtml",1);
        }
    }
}
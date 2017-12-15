using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching; 
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Widgets.GoDaddyApi.Controllers
{
    [Area(AreaNames.Admin)]
    public class GodaddyApiController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

        public GodaddyApiController(IWorkContext workContext,
           IStoreService storeService,
           IPermissionService permissionService,
           IPictureService pictureService,
           ISettingService settingService,
           ICacheManager cacheManager,
           ILocalizationService localizationService)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._permissionService = permissionService;
            this._pictureService = pictureService;
            this._settingService = settingService;
            this._localizationService = localizationService;
        }

    }
}

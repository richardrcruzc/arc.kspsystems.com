using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.Widgets.GoDaddyApi.Infrastructure.Cache;
using Nop.Plugin.Widgets.GoDaddyApi.Models;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.GoDaddyApi.Components
{
    [ViewComponent(Name = "WidgetsGoDaddyApi")]
    public class WidgetsGoDaddyApiViewComponent : NopViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _cacheManager;
        private readonly ISettingService _settingService;
        private readonly IPictureService _pictureService;

        public WidgetsGoDaddyApiViewComponent(IStoreContext storeContext, 
            IStaticCacheManager cacheManager, 
            ISettingService settingService, 
            IPictureService pictureService)
        {
            this._storeContext = storeContext;
            this._cacheManager = cacheManager;
            this._settingService = settingService;
            this._pictureService = pictureService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var GoDaddyApiSettings = _settingService.LoadSetting<GoDaddyApiSettings>(_storeContext.CurrentStore.Id);

            var model = new PublicInfoModel
            {
                //Picture1Url = GetPictureUrl(GoDaddyApiSettings.Picture1Id),
                //Text1 = GoDaddyApiSettings.Text1,
                //Link1 = GoDaddyApiSettings.Link1,

                //Picture2Url = GetPictureUrl(GoDaddyApiSettings.Picture2Id),
                //Text2 = GoDaddyApiSettings.Text2,
                //Link2 = GoDaddyApiSettings.Link2,

                //Picture3Url = GetPictureUrl(GoDaddyApiSettings.Picture3Id),
                //Text3 = GoDaddyApiSettings.Text3,
                //Link3 = GoDaddyApiSettings.Link3,

                //Picture4Url = GetPictureUrl(GoDaddyApiSettings.Picture4Id),
                //Text4 = GoDaddyApiSettings.Text4,
                //Link4 = GoDaddyApiSettings.Link4,

                //Picture5Url = GetPictureUrl(GoDaddyApiSettings.Picture5Id),
                //Text5 = GoDaddyApiSettings.Text5,
                //Link5 = GoDaddyApiSettings.Link5
            };

            //if (string.IsNullOrEmpty(model.Picture1Url) && string.IsNullOrEmpty(model.Picture2Url) &&
            //    string.IsNullOrEmpty(model.Picture3Url) && string.IsNullOrEmpty(model.Picture4Url) &&
            //    string.IsNullOrEmpty(model.Picture5Url))
            //    //no pictures uploaded
            //    return Content("");

            return View("~/Plugins/Widgets.GoDaddyApi/Views/PublicInfo.cshtml", model);
        }

        protected string GetPictureUrl(int pictureId)
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.PICTURE_URL_MODEL_KEY, pictureId);
            return _cacheManager.Get(cacheKey, () =>
            {
                //little hack here. nulls aren't cacheable so set it to ""
                var url = _pictureService.GetPictureUrl(pictureId, showDefaultPicture: false) ?? "";
                return url;
            });
        }
    }
}

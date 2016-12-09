using System.Web.Mvc;
using Nop.Web.Framework.Security;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using System.Collections.Generic;
using Nop.Services.Catalog;
using Nop.Core;
using Nop.Web.Extensions;
using Nop.Core.Domain.Media;
using Nop.Web.Infrastructure.Cache;
using Nop.Core.Caching;
using Nop.Services.Media;
using Nop.Services.Localization;

namespace Nop.Web.Controllers
{
    public partial class HomeController : BasePublicController
    {
        private readonly IManufacturerService _manufacturerService;
        private readonly IStoreContext _storeContext;
        private readonly MediaSettings _mediaSettings;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly ICacheManager _cacheManager;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;

        public HomeController(ICategoryService categoryService,
        IManufacturerService manufacturerService,
        IWorkContext workContext,
        IStoreContext storeContext,
        IWebHelper webHelper,
        MediaSettings mediaSettings,
        ICacheManager cacheManager,
        IPictureService pictureService,
        ILocalizationService localizationService
        )
        {
            this._manufacturerService = manufacturerService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._webHelper = webHelper;
            this._mediaSettings = mediaSettings;
            this._cacheManager = cacheManager;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
        }

        [NopHttpsRequirement(SslRequirement.No)]
        public ActionResult Index()
        {
            var model = new List<ManufacturerModel>();
            var manufacturers = _manufacturerService.GetAllManufacturers(storeId: _storeContext.CurrentStore.Id);
            foreach (var manufacturer in manufacturers)
            {
                var modelMan = manufacturer.ToModel();

                //prepare picture model
                int pictureSize = _mediaSettings.ManufacturerThumbPictureSize;
                var manufacturerPictureCacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_PICTURE_MODEL_KEY, manufacturer.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                modelMan.PictureModel = _cacheManager.Get(manufacturerPictureCacheKey, () =>
                {
                    var picture = _pictureService.GetPictureById(manufacturer.PictureId);
                    var pictureModel = new PictureModel
                    {
                        FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                        ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                        Title = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageLinkTitleFormat"), modelMan.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageAlternateTextFormat"), modelMan.Name)
                    };
                    return pictureModel;
                });
                model.Add(modelMan);
            }
            return View(model);
        }
    }
}

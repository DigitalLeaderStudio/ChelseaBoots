using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Topics;
using Nop.Services.Vendors;
using Nop.Web.Controllers;
using Nop.Web.Extensions;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Security;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Nop.Web.Themes.ChelseaBootsTheme.Controllers
{
	public partial class CatalogController : BasePublicController
	{
		#region Fields

		private readonly ICategoryService _categoryService;
		private readonly IManufacturerService _manufacturerService;
		private readonly IProductService _productService;
		private readonly IVendorService _vendorService;
		private readonly ICategoryTemplateService _categoryTemplateService;
		private readonly IManufacturerTemplateService _manufacturerTemplateService;
		private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
		private readonly ITaxService _taxService;
		private readonly ICurrencyService _currencyService;
		private readonly IPictureService _pictureService;
		private readonly ILocalizationService _localizationService;
		private readonly IPriceCalculationService _priceCalculationService;
		private readonly IPriceFormatter _priceFormatter;
		private readonly IWebHelper _webHelper;
		private readonly ISpecificationAttributeService _specificationAttributeService;
		private readonly IProductTagService _productTagService;
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly IAclService _aclService;
		private readonly IStoreMappingService _storeMappingService;
		private readonly IPermissionService _permissionService;
		private readonly ICustomerActivityService _customerActivityService;
		private readonly ITopicService _topicService;
		private readonly IEventPublisher _eventPublisher;
		private readonly ISearchTermService _searchTermService;
		private readonly IMeasureService _measureService;
		private readonly MediaSettings _mediaSettings;
		private readonly CatalogSettings _catalogSettings;
		private readonly VendorSettings _vendorSettings;
		private readonly BlogSettings _blogSettings;
		private readonly ForumSettings _forumSettings;
		private readonly ICacheManager _cacheManager;

		#endregion

		#region Constructors

		public CatalogController(ICategoryService categoryService,
			IManufacturerService manufacturerService,
			IProductService productService,
			IVendorService vendorService,
			ICategoryTemplateService categoryTemplateService,
			IManufacturerTemplateService manufacturerTemplateService,
			IWorkContext workContext,
			IStoreContext storeContext,
			ITaxService taxService,
			ICurrencyService currencyService,
			IPictureService pictureService,
			ILocalizationService localizationService,
			IPriceCalculationService priceCalculationService,
			IPriceFormatter priceFormatter,
			IWebHelper webHelper,
			ISpecificationAttributeService specificationAttributeService,
			IProductTagService productTagService,
			IGenericAttributeService genericAttributeService,
			IAclService aclService,
			IStoreMappingService storeMappingService,
			IPermissionService permissionService,
			ICustomerActivityService customerActivityService,
			ITopicService topicService,
			IEventPublisher eventPublisher,
			ISearchTermService searchTermService,
			IMeasureService measureService,
			MediaSettings mediaSettings,
			CatalogSettings catalogSettings,
			VendorSettings vendorSettings,
			BlogSettings blogSettings,
			ForumSettings forumSettings,
			ICacheManager cacheManager)
		{
			this._categoryService = categoryService;
			this._manufacturerService = manufacturerService;
			this._productService = productService;
			this._vendorService = vendorService;
			this._categoryTemplateService = categoryTemplateService;
			this._manufacturerTemplateService = manufacturerTemplateService;
			this._workContext = workContext;
			this._storeContext = storeContext;
			this._taxService = taxService;
			this._currencyService = currencyService;
			this._pictureService = pictureService;
			this._localizationService = localizationService;
			this._priceCalculationService = priceCalculationService;
			this._priceFormatter = priceFormatter;
			this._webHelper = webHelper;
			this._specificationAttributeService = specificationAttributeService;
			this._productTagService = productTagService;
			this._genericAttributeService = genericAttributeService;
			this._aclService = aclService;
			this._storeMappingService = storeMappingService;
			this._permissionService = permissionService;
			this._customerActivityService = customerActivityService;
			this._topicService = topicService;
			this._eventPublisher = eventPublisher;
			this._searchTermService = searchTermService;
			this._measureService = measureService;
			this._mediaSettings = mediaSettings;
			this._catalogSettings = catalogSettings;
			this._vendorSettings = vendorSettings;
			this._blogSettings = blogSettings;
			this._forumSettings = forumSettings;
			this._cacheManager = cacheManager;
		}

		#endregion

		#region Utilities

		[NonAction]
		protected virtual List<int> GetChildCategoryIds(int parentCategoryId)
		{
			string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_CHILD_IDENTIFIERS_MODEL_KEY,
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

		/// <summary>
		/// Prepare category (simple) models
		/// </summary>
		/// <param name="rootCategoryId">Root category identifier</param>
		/// <param name="loadSubCategories">A value indicating whether subcategories should be loaded</param>
		/// <param name="allCategories">All available categories; pass null to load them internally</param>
		/// <returns>Category models</returns>
		[NonAction]
		protected virtual IList<CategorySimpleModel> PrepareCategorySimpleModels(int rootCategoryId,
			bool loadSubCategories = true, IList<Category> allCategories = null)
		{
			var result = new List<CategorySimpleModel>();

			//little hack for performance optimization.
			//we know that this method is used to load top and left menu for categories.
			//it'll load all categories anyway.
			//so there's no need to invoke "GetAllCategoriesByParentCategoryId" multiple times (extra SQL commands) to load childs
			//so we load all categories at once
			//if you don't like this implementation if you can uncomment the line below (old behavior) and comment several next lines (before foreach)
			//var categories = _categoryService.GetAllCategoriesByParentCategoryId(rootCategoryId);
			if (allCategories == null)
			{
				//load categories if null passed
				//we implemeneted it this way for performance optimization - recursive iterations (below)
				//this way all categories are loaded only once
				allCategories = _categoryService.GetAllCategories(storeId: _storeContext.CurrentStore.Id);
			}
			var categories = allCategories.Where(c => c.ParentCategoryId == rootCategoryId).ToList();
			foreach (var category in categories)
			{
				var categoryModel = new CategorySimpleModel
				{
					Id = category.Id,
					Name = category.GetLocalized(x => x.Name),
					SeName = category.GetSeName(),
					IncludeInTopMenu = category.IncludeInTopMenu
				};

				//nubmer of products in each category
				if (_catalogSettings.ShowCategoryProductNumber)
				{
					string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NUMBER_OF_PRODUCTS_MODEL_KEY,
						string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
						_storeContext.CurrentStore.Id,
						category.Id);
					categoryModel.NumberOfProducts = _cacheManager.Get(cacheKey, () =>
					{
						var categoryIds = new List<int>();
						categoryIds.Add(category.Id);
						//include subcategories
						if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
							categoryIds.AddRange(GetChildCategoryIds(category.Id));
						return _productService.GetNumberOfProductsInCategory(categoryIds, _storeContext.CurrentStore.Id);
					});
				}

				if (loadSubCategories)
				{
					var subCategories = PrepareCategorySimpleModels(category.Id, loadSubCategories, allCategories);
					categoryModel.SubCategories.AddRange(subCategories);
				}
				result.Add(categoryModel);
			}

			return result;
		}

		[NonAction]
		protected virtual IEnumerable<ProductOverviewModel> PrepareProductOverviewModels(
			IEnumerable<Product> products,
			bool preparePriceModel = true,
			bool preparePictureModel = true,
			int? productThumbPictureSize = null,
			bool prepareSpecificationAttributes = false,
			bool forceRedirectionAfterAddingToCart = false)
		{
			return this.PrepareProductOverviewModels(_workContext,
				_storeContext,
				_categoryService,
				_productService,
				_specificationAttributeService,
				_priceCalculationService,
				_priceFormatter,
				_permissionService,
				_localizationService,
				_taxService,
				_currencyService,
				_pictureService,
				_measureService,
				_webHelper,
				_cacheManager,
				_catalogSettings,
				_mediaSettings,
				products,
				preparePriceModel,
				preparePictureModel,
				productThumbPictureSize,
				prepareSpecificationAttributes,
				forceRedirectionAfterAddingToCart);
		}

		private ActionResult VerifyDataAndSetAttributes<T>(T entity) where T : BaseEntity, IAclSupported, IStoreMappingSupported
		{
			var publishedPropertyValue = (bool)entity.GetType().GetProperty("Published").GetValue(entity);
			var deletedPropertyValue = (bool)entity.GetType().GetProperty("Deleted").GetValue(entity);

			if (entity == null || deletedPropertyValue)
				return InvokeHttp404();

			//Check whether the current user has a "Manage catalog" permission
			//It allows him to preview a category before publishing
			if (!publishedPropertyValue && !_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
				return InvokeHttp404();

			//ACL (access control list)
			if (!_aclService.Authorize(entity))
				return InvokeHttp404();

			//Store mapping
			if (!_storeMappingService.Authorize(entity))
				return InvokeHttp404();

			//'Continue shopping' URL
			_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
				SystemCustomerAttributeNames.LastContinueShoppingPage,
				_webHelper.GetThisPageUrl(false),
				_storeContext.CurrentStore.Id);

			return null;
		}

		private void BuildCategoryBreadcrumb(CategoryModel model, Category category)
		{
			if (_catalogSettings.CategoryBreadcrumbEnabled)
			{
				model.DisplayCategoryBreadcrumb = true;

				string breadcrumbCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_BREADCRUMB_KEY,
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
		}

		private void BuildSubCategories(CategoryModel model, Category category)
		{
			string subCategoriesCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_SUBCATEGORIES_KEY,
				category.Id,
				_mediaSettings.CategoryThumbPictureSize,
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
					var categoryPictureCacheKey = string.Format(
						ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY,
						x.Id,
						_mediaSettings.CategoryThumbPictureSize,
						true,
						_workContext.WorkingLanguage.Id,
						_webHelper.IsCurrentConnectionSecured(),
						_storeContext.CurrentStore.Id);
					subCatModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
					{
						var picture = _pictureService.GetPictureById(x.PictureId);
						var pictureModel = new PictureModel
						{
							FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
							ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.CategoryThumbPictureSize),
							Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), subCatModel.Name),
							AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), subCatModel.Name)
						};
						return pictureModel;
					});

					return subCatModel;
				})
				.ToList()
			);
		}

		private IList<ProductOverviewModel> BuildFeatureProducts(Category category, Manufacturer manufacturer)
		{
			var result = new List<ProductOverviewModel>();

			if (!_catalogSettings.IgnoreFeaturedProducts)
			{
				//We cache a value indicating whether we have featured products
				IPagedList<Product> featuredProducts = null;
				string cacheKey = string.Format(
					category != null ? ModelCacheEventConsumer.CATEGORY_HAS_FEATURED_PRODUCTS_KEY : ModelCacheEventConsumer.MANUFACTURER_HAS_FEATURED_PRODUCTS_KEY,
					category != null ? category.Id : manufacturer.Id,
					string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
					_storeContext.CurrentStore.Id);
				var hasFeaturedProductsCache = _cacheManager.Get<bool?>(cacheKey);
				if (!hasFeaturedProductsCache.HasValue)
				{
					//no value in the cache yet
					//let's load products and cache the result (true/false)
					featuredProducts = _productService.SearchProducts(
					   categoryIds: category == null ? null : new List<int> { category.Id },
					   manufacturerId: manufacturer == null ? 0 : manufacturer.Id,
					   storeId: _storeContext.CurrentStore.Id,
					   visibleIndividuallyOnly: true,
					   featuredProducts: true);
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
				}

				result = featuredProducts != null ? PrepareProductOverviewModels(featuredProducts).ToList() : result;
			}

			return result;
		}

		private PriceRange PreparePriceRangeFilter(CatalogPagingFilteringModel pagingFilter, string priceRanges)
		{
			var result = new PriceRange();

			pagingFilter.PriceRangeFilter.LoadPriceRangeFilters(priceRanges, _webHelper, _priceFormatter);
			var selectedPriceRange = pagingFilter.PriceRangeFilter.GetSelectedPriceRange(_webHelper, priceRanges);

			if (selectedPriceRange != null)
			{
				if (selectedPriceRange.From.HasValue)
				{
					result.From = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.From.Value, _workContext.WorkingCurrency);
				}
				if (selectedPriceRange.To.HasValue)
				{
					result.To = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.To.Value, _workContext.WorkingCurrency);
				}
			}

			return result;
		}

		#endregion

		#region Categories

		[NopHttpsRequirement(SslRequirement.No)]
		public ActionResult Category(int categoryId, CatalogPagingFilteringModel command)
		{
			var category = _categoryService.GetCategoryById(categoryId);

			var verificationResult = VerifyDataAndSetAttributes<Category>(category);
			if (verificationResult != null)
			{
				return verificationResult;
			}

			var model = category.ToModel();

			//sorting
			this.PrepareSortingOptions(model.PagingFilteringContext, command, _webHelper, _workContext, _localizationService, _catalogSettings);
			//view mode
			this.PrepareViewModes(model.PagingFilteringContext, command, _webHelper, _localizationService, _catalogSettings);
			//page size
			this.PreparePageSizeOptions(
				model.PagingFilteringContext,
				command,
				_webHelper,
				category.AllowCustomersToSelectPageSize,
				category.PageSizeOptions,
				category.PageSize);
			//price ranges
			var convertedPriceRange = PreparePriceRangeFilter(model.PagingFilteringContext, category.PriceRanges);

			//category breadcrumb
			BuildCategoryBreadcrumb(model, category);

			//subcategories
			BuildSubCategories(model, category);

			//featured products
			model.FeaturedProducts = BuildFeatureProducts(category, null);

			var categoryIds = new List<int>();
			categoryIds.Add(category.Id);
			if (_catalogSettings.ShowProductsFromSubcategories)
			{
				//include subcategories
				categoryIds.AddRange(GetChildCategoryIds(category.Id));
			}
			//products
			IList<int> alreadyFilteredSpecOptionIds = model.PagingFilteringContext.SpecificationFilter.GetAlreadyFilteredSpecOptionIds(_webHelper);
			IList<int> filterableSpecificationAttributeOptionIds;
			var products = _productService.SearchProducts(
				out filterableSpecificationAttributeOptionIds,
				true,
				categoryIds: categoryIds,
				storeId: _storeContext.CurrentStore.Id,
				visibleIndividuallyOnly: true,
				featuredProducts: _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false,
				priceMin: convertedPriceRange.From,
				priceMax: convertedPriceRange.To,
				filteredSpecs: alreadyFilteredSpecOptionIds,
				orderBy: (ProductSortingEnum)command.OrderBy,
				pageIndex: command.PageNumber - 1,
				pageSize: command.PageSize);
			model.Products = PrepareProductOverviewModels(products, prepareSpecificationAttributes: true).ToList();

			model.PagingFilteringContext.LoadPagedList(products);

			//specs
			model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(
				alreadyFilteredSpecOptionIds,
				filterableSpecificationAttributeOptionIds != null ? filterableSpecificationAttributeOptionIds.ToArray() : null,
				_specificationAttributeService,
				_webHelper,
				_workContext,
				_cacheManager);

			//activity log
			_customerActivityService.InsertActivity(
				"PublicStore.ViewCategory",
				_localizationService.GetResource("ActivityLog.PublicStore.ViewCategory"),
				category.Name);

			if (Request.IsAjaxRequest())
			{
				return PartialView("_CategoryTemplateInnerBody", model);
			}

			//display "edit" (manage) link
			if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageCategories))
				DisplayEditLink(Url.Action("Edit", "Category", new { id = category.Id, area = "Admin" }));

			//template
			var templateCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_TEMPLATE_MODEL_KEY, category.CategoryTemplateId);
			var templateViewPath = _cacheManager.Get(templateCacheKey, () =>
			{
				var template = _categoryTemplateService.GetCategoryTemplateById(category.CategoryTemplateId);
				if (template == null)
					template = _categoryTemplateService.GetAllCategoryTemplates().FirstOrDefault();
				if (template == null)
					throw new Exception("No default template could be loaded");
				return template.ViewPath;
			});

			return View(templateViewPath, model);
		}

		[ChildActionOnly]
		public ActionResult CategoryNavigation(int currentCategoryId, int currentProductId)
		{
			//get active category
			int activeCategoryId = 0;
			if (currentCategoryId > 0)
			{
				//category details page
				activeCategoryId = currentCategoryId;
			}
			else if (currentProductId > 0)
			{
				//product details page
				var productCategories = _categoryService.GetProductCategoriesByProductId(currentProductId);
				if (productCategories.Any())
					activeCategoryId = productCategories[0].CategoryId;
			}

			string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NAVIGATION_MODEL_KEY,
				_workContext.WorkingLanguage.Id,
				string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
				_storeContext.CurrentStore.Id);
			var cachedModel = _cacheManager.Get(cacheKey, () => PrepareCategorySimpleModels(0).ToList());

			var model = new CategoryNavigationModel
			{
				CurrentCategoryId = activeCategoryId,
				Categories = cachedModel
			};

			return PartialView(model);
		}

		[ChildActionOnly]
		public ActionResult TopMenu()
		{
			//categories
			string categoryCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_MENU_MODEL_KEY,
				_workContext.WorkingLanguage.Id,
				string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
				_storeContext.CurrentStore.Id);
			var cachedCategoriesModel = _cacheManager.Get(categoryCacheKey, () => PrepareCategorySimpleModels(0));

			//top menu topics
			string topicCacheKey = string.Format(ModelCacheEventConsumer.TOPIC_TOP_MENU_MODEL_KEY,
				_workContext.WorkingLanguage.Id,
				_storeContext.CurrentStore.Id,
				string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()));
			var cachedTopicModel = _cacheManager.Get(topicCacheKey, () =>
				_topicService.GetAllTopics(_storeContext.CurrentStore.Id)
				.Where(t => t.IncludeInTopMenu)
				.Select(t => new TopMenuModel.TopMenuTopicModel
				{
					Id = t.Id,
					Name = t.GetLocalized(x => x.Title),
					SeName = t.GetSeName()
				})
				.ToList()
			);
			var model = new TopMenuModel
			{
				Categories = cachedCategoriesModel,
				Topics = cachedTopicModel,
				NewProductsEnabled = _catalogSettings.NewProductsEnabled,
				BlogEnabled = _blogSettings.Enabled,
				ForumEnabled = _forumSettings.ForumsEnabled
			};
			return PartialView(model);
		}

		[ChildActionOnly]
		public ActionResult HomepageCategories()
		{
			var pictureSize = _mediaSettings.CategoryThumbPictureSize;

			string categoriesCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_HOMEPAGE_KEY,
				string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
				pictureSize,
				_storeContext.CurrentStore.Id,
				_workContext.WorkingLanguage.Id,
				_webHelper.IsCurrentConnectionSecured());

			var model = _cacheManager.Get(categoriesCacheKey, () =>
				_categoryService.GetAllCategoriesDisplayedOnHomePage()
				.Select(x =>
				{
					var catModel = x.ToModel();

					//prepare picture model
					var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
					catModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
					{
						var picture = _pictureService.GetPictureById(x.PictureId);
						var pictureModel = new PictureModel
						{
							FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
							ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
							Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
							AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
						};
						return pictureModel;
					});

					return catModel;
				})
				.ToList()
			);

			if (!model.Any())
				return Content("");

			return PartialView(model);
		}

		#endregion

		#region Manufacturers

		[NopHttpsRequirement(SslRequirement.No)]
		public ActionResult Manufacturer(int manufacturerId, CatalogPagingFilteringModel command)
		{
			var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);

			var verificationResult = VerifyDataAndSetAttributes<Manufacturer>(manufacturer);
			if (verificationResult != null)
			{
				return verificationResult;
			}

			var model = manufacturer.ToModel();

			//sorting
			this.PrepareSortingOptions(model.PagingFilteringContext, command, _webHelper, _workContext, _localizationService, _catalogSettings);
			//view mode
			this.PrepareViewModes(model.PagingFilteringContext, command, _webHelper, _localizationService, _catalogSettings);
			//page size
			this.PreparePageSizeOptions(
				model.PagingFilteringContext,
				command,
				_webHelper,
				manufacturer.AllowCustomersToSelectPageSize,
				manufacturer.PageSizeOptions,
				manufacturer.PageSize);

			//price ranges
			var convertedPriceRange = PreparePriceRangeFilter(model.PagingFilteringContext, manufacturer.PriceRanges);

			//featured products
			model.FeaturedProducts = BuildFeatureProducts(null, manufacturer);

			//products
			IList<int> alreadyFilteredSpecOptionIds = model.PagingFilteringContext.SpecificationFilter.GetAlreadyFilteredSpecOptionIds(_webHelper);
			IList<int> filterableSpecificationAttributeOptionIds;

			var products = _productService.SearchProducts(
				out filterableSpecificationAttributeOptionIds,
				true,
				manufacturerId: manufacturer.Id,
				storeId: _storeContext.CurrentStore.Id,
				visibleIndividuallyOnly: true,
				featuredProducts: _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false,
				priceMin: convertedPriceRange.From,
				priceMax: convertedPriceRange.To,
				filteredSpecs: alreadyFilteredSpecOptionIds,
				orderBy: (ProductSortingEnum)command.OrderBy,
				pageIndex: command.PageNumber - 1,
				pageSize: command.PageSize);
			model.Products = PrepareProductOverviewModels(products, prepareSpecificationAttributes: true).ToList();

			model.PagingFilteringContext.LoadPagedList(products);

			//specs
			model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(
				alreadyFilteredSpecOptionIds,
				filterableSpecificationAttributeOptionIds != null ? filterableSpecificationAttributeOptionIds.ToArray() : null,
				_specificationAttributeService,
				_webHelper,
				_workContext,
				_cacheManager);

			//activity log
			_customerActivityService.InsertActivity("PublicStore.ViewManufacturer", _localizationService.GetResource("ActivityLog.PublicStore.ViewManufacturer"), manufacturer.Name);

			if (Request.IsAjaxRequest())
			{
				return PartialView("_ManufacturerTemplateInnerBody", model);
			}

			//template
			var templateCacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_TEMPLATE_MODEL_KEY, manufacturer.ManufacturerTemplateId);
			var templateViewPath = _cacheManager.Get(templateCacheKey, () =>
			{
				var template = _manufacturerTemplateService.GetManufacturerTemplateById(manufacturer.ManufacturerTemplateId);
				if (template == null)
					template = _manufacturerTemplateService.GetAllManufacturerTemplates().FirstOrDefault();
				if (template == null)
					throw new Exception("No default template could be loaded");
				return template.ViewPath;
			});

			//display "edit" (manage) link
			if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
				DisplayEditLink(Url.Action("Edit", "Manufacturer", new { id = manufacturer.Id, area = "Admin" }));

			return View(templateViewPath, model);
		}

		[NopHttpsRequirement(SslRequirement.No)]
		public ActionResult ManufacturerAll()
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

		[ChildActionOnly]
		public ActionResult ManufacturerNavigation(int currentManufacturerId)
		{
			if (_catalogSettings.ManufacturersBlockItemsToDisplay == 0)
				return Content("");

			string cacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_NAVIGATION_MODEL_KEY,
				currentManufacturerId,
				_workContext.WorkingLanguage.Id,
				string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
				_storeContext.CurrentStore.Id);
			var cacheModel = _cacheManager.Get(cacheKey, () =>
			{
				var currentManufacturer = _manufacturerService.GetManufacturerById(currentManufacturerId);

				var manufacturers = _manufacturerService.GetAllManufacturers(storeId: _storeContext.CurrentStore.Id,
					pageSize: _catalogSettings.ManufacturersBlockItemsToDisplay);
				var model = new ManufacturerNavigationModel
				{
					TotalManufacturers = manufacturers.TotalCount
				};

				foreach (var manufacturer in manufacturers)
				{
					var modelMan = new ManufacturerBriefInfoModel
					{
						Id = manufacturer.Id,
						Name = manufacturer.GetLocalized(x => x.Name),
						SeName = manufacturer.GetSeName(),
						IsActive = currentManufacturer != null && currentManufacturer.Id == manufacturer.Id,
					};
					model.Manufacturers.Add(modelMan);
				}
				return model;
			});

			if (!cacheModel.Manufacturers.Any())
				return Content("");

			return PartialView(cacheModel);
		}

		#endregion

		#region Vendors

		[NopHttpsRequirement(SslRequirement.No)]
		public ActionResult Vendor(int vendorId, CatalogPagingFilteringModel command)
		{
			var vendor = _vendorService.GetVendorById(vendorId);
			if (vendor == null || vendor.Deleted || !vendor.Active)
				return InvokeHttp404();

			//Vendor is active?
			if (!vendor.Active)
				return InvokeHttp404();

			//'Continue shopping' URL
			_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
				SystemCustomerAttributeNames.LastContinueShoppingPage,
				_webHelper.GetThisPageUrl(false),
				_storeContext.CurrentStore.Id);

			var model = new VendorModel
			{
				Id = vendor.Id,
				Name = vendor.GetLocalized(x => x.Name),
				Description = vendor.GetLocalized(x => x.Description),
				MetaKeywords = vendor.GetLocalized(x => x.MetaKeywords),
				MetaDescription = vendor.GetLocalized(x => x.MetaDescription),
				MetaTitle = vendor.GetLocalized(x => x.MetaTitle),
				SeName = vendor.GetSeName(),
				AllowCustomersToContactVendors = _vendorSettings.AllowCustomersToContactVendors
			};

			//sorting
			this.PrepareSortingOptions(model.PagingFilteringContext, command, _webHelper, _workContext, _localizationService, _catalogSettings);
			//view mode
			this.PrepareViewModes(model.PagingFilteringContext, command, _webHelper, _localizationService, _catalogSettings);
			//page size
			this.PreparePageSizeOptions(
				model.PagingFilteringContext,
				command,
				_webHelper, vendor.AllowCustomersToSelectPageSize,
				vendor.PageSizeOptions,
				vendor.PageSize);

			//products
			IList<int> filterableSpecificationAttributeOptionIds;
			var products = _productService.SearchProducts(out filterableSpecificationAttributeOptionIds, true,
				vendorId: vendor.Id,
				storeId: _storeContext.CurrentStore.Id,
				visibleIndividuallyOnly: true,
				orderBy: (ProductSortingEnum)command.OrderBy,
				pageIndex: command.PageNumber - 1,
				pageSize: command.PageSize);
			model.Products = PrepareProductOverviewModels(products).ToList();

			model.PagingFilteringContext.LoadPagedList(products);

			//display "edit" (manage) link
			if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageVendors))
				DisplayEditLink(Url.Action("Edit", "Vendor", new { id = vendor.Id, area = "Admin" }));

			return View(model);
		}

		[NopHttpsRequirement(SslRequirement.No)]
		public ActionResult VendorAll()
		{
			//we don't allow viewing of vendors if "vendors" block is hidden
			if (_vendorSettings.VendorsBlockItemsToDisplay == 0)
				return RedirectToRoute("HomePage");

			var model = new List<VendorModel>();
			var vendors = _vendorService.GetAllVendors();
			foreach (var vendor in vendors)
			{
				var vendorModel = new VendorModel
				{
					Id = vendor.Id,
					Name = vendor.GetLocalized(x => x.Name),
					Description = vendor.GetLocalized(x => x.Description),
					MetaKeywords = vendor.GetLocalized(x => x.MetaKeywords),
					MetaDescription = vendor.GetLocalized(x => x.MetaDescription),
					MetaTitle = vendor.GetLocalized(x => x.MetaTitle),
					SeName = vendor.GetSeName(),
					AllowCustomersToContactVendors = _vendorSettings.AllowCustomersToContactVendors
				};
				//prepare picture model
				int pictureSize = _mediaSettings.VendorThumbPictureSize;
				var pictureCacheKey = string.Format(ModelCacheEventConsumer.VENDOR_PICTURE_MODEL_KEY, vendor.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
				vendorModel.PictureModel = _cacheManager.Get(pictureCacheKey, () =>
				{
					var picture = _pictureService.GetPictureById(vendor.PictureId);
					var pictureModel = new PictureModel
					{
						FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
						ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
						Title = string.Format(_localizationService.GetResource("Media.Vendor.ImageLinkTitleFormat"), vendorModel.Name),
						AlternateText = string.Format(_localizationService.GetResource("Media.Vendor.ImageAlternateTextFormat"), vendorModel.Name)
					};
					return pictureModel;
				});
				model.Add(vendorModel);
			}

			return View(model);
		}

		[ChildActionOnly]
		public ActionResult VendorNavigation()
		{
			if (_vendorSettings.VendorsBlockItemsToDisplay == 0)
				return Content("");

			string cacheKey = ModelCacheEventConsumer.VENDOR_NAVIGATION_MODEL_KEY;
			var cacheModel = _cacheManager.Get(cacheKey, () =>
			{
				var vendors = _vendorService.GetAllVendors(pageSize: _vendorSettings.VendorsBlockItemsToDisplay);
				var model = new VendorNavigationModel
				{
					TotalVendors = vendors.TotalCount
				};

				foreach (var vendor in vendors)
				{
					model.Vendors.Add(new VendorBriefInfoModel
					{
						Id = vendor.Id,
						Name = vendor.GetLocalized(x => x.Name),
						SeName = vendor.GetSeName(),
					});
				}
				return model;
			});

			if (!cacheModel.Vendors.Any())
				return Content("");

			return PartialView(cacheModel);
		}

		#endregion

		#region Product tags

		[ChildActionOnly]
		public ActionResult PopularProductTags()
		{
			var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCTTAG_POPULAR_MODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
			var cacheModel = _cacheManager.Get(cacheKey, () =>
			{
				var model = new PopularProductTagsModel();

				//get all tags
				var allTags = _productTagService
					.GetAllProductTags()
					//filter by current store
					.Where(x => _productTagService.GetProductCount(x.Id, _storeContext.CurrentStore.Id) > 0)
					//order by product count
					.OrderByDescending(x => _productTagService.GetProductCount(x.Id, _storeContext.CurrentStore.Id))
					.ToList();

				var tags = allTags
					.Take(_catalogSettings.NumberOfProductTags)
					.ToList();
				//sorting
				tags = tags.OrderBy(x => x.GetLocalized(y => y.Name)).ToList();

				model.TotalTags = allTags.Count;

				foreach (var tag in tags)
					model.Tags.Add(new ProductTagModel
					{
						Id = tag.Id,
						Name = tag.GetLocalized(y => y.Name),
						SeName = tag.GetSeName(),
						ProductCount = _productTagService.GetProductCount(tag.Id, _storeContext.CurrentStore.Id)
					});
				return model;
			});

			if (!cacheModel.Tags.Any())
				return Content("");

			return PartialView(cacheModel);
		}

		[NopHttpsRequirement(SslRequirement.No)]
		public ActionResult ProductsByTag(int productTagId, CatalogPagingFilteringModel command)
		{
			var productTag = _productTagService.GetProductTagById(productTagId);
			if (productTag == null)
				return InvokeHttp404();

			var model = new ProductsByTagModel
			{
				Id = productTag.Id,
				TagName = productTag.GetLocalized(y => y.Name),
				TagSeName = productTag.GetSeName()
			};

			//sorting
			this.PrepareSortingOptions(model.PagingFilteringContext, command, _webHelper, _workContext, _localizationService, _catalogSettings);
			//view mode
			this.PrepareViewModes(model.PagingFilteringContext, command, _webHelper, _localizationService, _catalogSettings);
			//page size
			this.PreparePageSizeOptions(
				model.PagingFilteringContext,
				command,
				_webHelper,
				_catalogSettings.ProductsByTagAllowCustomersToSelectPageSize,
				_catalogSettings.ProductsByTagPageSizeOptions,
				_catalogSettings.ProductsByTagPageSize);

			//products
			var products = _productService.SearchProducts(
				storeId: _storeContext.CurrentStore.Id,
				productTagId: productTag.Id,
				visibleIndividuallyOnly: true,
				orderBy: (ProductSortingEnum)command.OrderBy,
				pageIndex: command.PageNumber - 1,
				pageSize: command.PageSize);
			model.Products = PrepareProductOverviewModels(products).ToList();

			model.PagingFilteringContext.LoadPagedList(products);
			return View(model);
		}

		[NopHttpsRequirement(SslRequirement.No)]
		public ActionResult ProductTagsAll()
		{
			var model = new PopularProductTagsModel();
			model.Tags = _productTagService
				.GetAllProductTags()
				//filter by current store
				.Where(x => _productTagService.GetProductCount(x.Id, _storeContext.CurrentStore.Id) > 0)
				//sort by name
				.OrderBy(x => x.GetLocalized(y => y.Name))
				.Select(x =>
				{
					var ptModel = new ProductTagModel
					{
						Id = x.Id,
						Name = x.GetLocalized(y => y.Name),
						SeName = x.GetSeName(),
						ProductCount = _productTagService.GetProductCount(x.Id, _storeContext.CurrentStore.Id)
					};
					return ptModel;
				})
				.ToList();
			return View(model);
		}

		#endregion

		#region Searching

		[NopHttpsRequirement(SslRequirement.No)]
		[ValidateInput(false)]
		public ActionResult Search(SearchModel model, CatalogPagingFilteringModel command)
		{
			//'Continue shopping' URL
			_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
				SystemCustomerAttributeNames.LastContinueShoppingPage,
				_webHelper.GetThisPageUrl(false),
				_storeContext.CurrentStore.Id);

			if (model == null)
				model = new SearchModel();

			var searchTerms = model.q;
			if (searchTerms == null)
				searchTerms = "";
			searchTerms = searchTerms.Trim();

			//sorting
			this.PrepareSortingOptions(model.PagingFilteringContext, command, _webHelper, _workContext, _localizationService, _catalogSettings);
			//view mode
			this.PrepareViewModes(model.PagingFilteringContext, command, _webHelper, _localizationService, _catalogSettings);
			//page size
			this.PreparePageSizeOptions(
				model.PagingFilteringContext,
				command,
				_webHelper,
				_catalogSettings.SearchPageAllowCustomersToSelectPageSize,
				_catalogSettings.SearchPagePageSizeOptions,
				_catalogSettings.SearchPageProductsPerPage);
			
			string cacheKey = string.Format(ModelCacheEventConsumer.SEARCH_CATEGORIES_MODEL_KEY,
				_workContext.WorkingLanguage.Id,
				string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
				_storeContext.CurrentStore.Id);
			var categories = _cacheManager.Get(cacheKey, () =>
			{
				var categoriesModel = new List<SearchModel.CategoryModel>();
				//all categories
				var allCategories = _categoryService.GetAllCategories(storeId: _storeContext.CurrentStore.Id);
				foreach (var c in allCategories)
				{
					//generate full category name (breadcrumb)
					string categoryBreadcrumb = "";
					var breadcrumb = c.GetCategoryBreadCrumb(allCategories, _aclService, _storeMappingService);
					for (int i = 0; i <= breadcrumb.Count - 1; i++)
					{
						categoryBreadcrumb += breadcrumb[i].GetLocalized(x => x.Name);
						if (i != breadcrumb.Count - 1)
							categoryBreadcrumb += " >> ";
					}
					categoriesModel.Add(new SearchModel.CategoryModel
					{
						Id = c.Id,
						Breadcrumb = categoryBreadcrumb
					});
				}
				return categoriesModel;
			});
			if (categories.Any())
			{
				//first empty entry
				model.AvailableCategories.Add(new SelectListItem
				{
					Value = "0",
					Text = _localizationService.GetResource("Common.All")
				});
				//all other categories
				foreach (var c in categories)
				{
					model.AvailableCategories.Add(new SelectListItem
					{
						Value = c.Id.ToString(),
						Text = c.Breadcrumb,
						Selected = model.cid == c.Id
					});
				}
			}

			var manufacturers = _manufacturerService.GetAllManufacturers(storeId: _storeContext.CurrentStore.Id);
			if (manufacturers.Any())
			{
				model.AvailableManufacturers.Add(new SelectListItem
				{
					Value = "0",
					Text = _localizationService.GetResource("Common.All")
				});
				foreach (var m in manufacturers)
					model.AvailableManufacturers.Add(new SelectListItem
					{
						Value = m.Id.ToString(),
						Text = m.GetLocalized(x => x.Name),
						Selected = model.mid == m.Id
					});
			}

			model.asv = _vendorSettings.AllowSearchByVendor;
			if (model.asv)
			{
				var vendors = _vendorService.GetAllVendors();
				if (vendors.Any())
				{
					model.AvailableVendors.Add(new SelectListItem
					{
						Value = "0",
						Text = _localizationService.GetResource("Common.All")
					});
					foreach (var vendor in vendors)
						model.AvailableVendors.Add(new SelectListItem
						{
							Value = vendor.Id.ToString(),
							Text = vendor.GetLocalized(x => x.Name),
							Selected = model.vid == vendor.Id
						});
				}
			}

			IPagedList<Product> products = new PagedList<Product>(new List<Product>(), 0, 1);
			// only search if query string search keyword is set (used to avoid searching or displaying search term min length error message on /search page load)
			if (Request.Params["q"] != null)
			{
				if (searchTerms.Length < _catalogSettings.ProductSearchTermMinimumLength)
				{
					model.Warning = string.Format(_localizationService.GetResource("Search.SearchTermMinimumLengthIsNCharacters"), _catalogSettings.ProductSearchTermMinimumLength);
				}
				else
				{
					var categoryIds = new List<int>();
					int manufacturerId = 0;
					decimal? minPriceConverted = null;
					decimal? maxPriceConverted = null;
					bool searchInDescriptions = false;
					int vendorId = 0;
					if (model.adv)
					{
						//advanced search
						var categoryId = model.cid;
						if (categoryId > 0)
						{
							categoryIds.Add(categoryId);
							if (model.isc)
							{
								//include subcategories
								categoryIds.AddRange(GetChildCategoryIds(categoryId));
							}
						}

						manufacturerId = model.mid;

						//min price
						if (!string.IsNullOrEmpty(model.pf))
						{
							decimal minPrice;
							if (decimal.TryParse(model.pf, out minPrice))
								minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(minPrice, _workContext.WorkingCurrency);
						}
						//max price
						if (!string.IsNullOrEmpty(model.pt))
						{
							decimal maxPrice;
							if (decimal.TryParse(model.pt, out maxPrice))
								maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(maxPrice, _workContext.WorkingCurrency);
						}

						if (model.asv)
							vendorId = model.vid;

						searchInDescriptions = model.sid;
					}

					//var searchInProductTags = false;
					var searchInProductTags = searchInDescriptions;

					//products
					products = _productService.SearchProducts(
						categoryIds: categoryIds,
						manufacturerId: manufacturerId,
						storeId: _storeContext.CurrentStore.Id,
						visibleIndividuallyOnly: true,
						priceMin: minPriceConverted,
						priceMax: maxPriceConverted,
						keywords: searchTerms,
						searchDescriptions: searchInDescriptions,
						searchProductTags: searchInProductTags,
						languageId: _workContext.WorkingLanguage.Id,
						orderBy: (ProductSortingEnum)command.OrderBy,
						pageIndex: command.PageNumber - 1,
						pageSize: command.PageSize,
						vendorId: vendorId);
					model.Products = PrepareProductOverviewModels(products).ToList();

					model.NoResults = !model.Products.Any();

					//search term statistics
					if (!String.IsNullOrEmpty(searchTerms))
					{
						var searchTerm = _searchTermService.GetSearchTermByKeyword(searchTerms, _storeContext.CurrentStore.Id);
						if (searchTerm != null)
						{
							searchTerm.Count++;
							_searchTermService.UpdateSearchTerm(searchTerm);
						}
						else
						{
							searchTerm = new SearchTerm
							{
								Keyword = searchTerms,
								StoreId = _storeContext.CurrentStore.Id,
								Count = 1
							};
							_searchTermService.InsertSearchTerm(searchTerm);
						}
					}

					//event
					_eventPublisher.Publish(new ProductSearchEvent
					{
						SearchTerm = searchTerms,
						SearchInDescriptions = searchInDescriptions,
						CategoryIds = categoryIds,
						ManufacturerId = manufacturerId,
						WorkingLanguageId = _workContext.WorkingLanguage.Id,
						VendorId = vendorId
					});
				}
			}

			model.PagingFilteringContext.LoadPagedList(products);
			return View(model);
		}

		[ChildActionOnly]
		public ActionResult SearchBox()
		{
			var model = new SearchBoxModel
			{
				AutoCompleteEnabled = _catalogSettings.ProductSearchAutoCompleteEnabled,
				ShowProductImagesInSearchAutoComplete = _catalogSettings.ShowProductImagesInSearchAutoComplete,
				SearchTermMinimumLength = _catalogSettings.ProductSearchTermMinimumLength
			};
			return PartialView(model);
		}

		public ActionResult SearchTermAutoComplete(string term)
		{
			if (String.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
				return Content("");

			//products
			var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
				_catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;

			var products = _productService.SearchProducts(
				storeId: _storeContext.CurrentStore.Id,
				keywords: term,
				languageId: _workContext.WorkingLanguage.Id,
				visibleIndividuallyOnly: true,
				pageSize: productNumber);

			var models = PrepareProductOverviewModels(products, false, _catalogSettings.ShowProductImagesInSearchAutoComplete, _mediaSettings.AutoCompleteSearchThumbPictureSize).ToList();
			var result = (from p in models
						  select new
						  {
							  label = p.Name,
							  producturl = Url.RouteUrl("Product", new { SeName = p.SeName }),
							  productpictureurl = p.DefaultPictureModel.ImageUrl
						  })
						  .ToList();
			return Json(result, JsonRequestBehavior.AllowGet);
		}

		#endregion
	}
}
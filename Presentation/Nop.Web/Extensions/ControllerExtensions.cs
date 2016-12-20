using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Tax;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Core.Domain.Customers;
using Nop.Web.Themes.ChelseaBootsTheme.Models;

namespace Nop.Web.Extensions
{
	//here we have some methods shared between controllers
	public static class ControllerExtensions
	{
		public static IList<ProductSpecificationModel> PrepareProductSpecificationModel(this Controller controller,
			IWorkContext workContext,
			ISpecificationAttributeService specificationAttributeService,
			ICacheManager cacheManager,
			Product product)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_SPECS_MODEL_KEY, product.Id, workContext.WorkingLanguage.Id);
			return cacheManager.Get(cacheKey, () =>
				specificationAttributeService.GetProductSpecificationAttributes(product.Id, 0, null, true)
				.Select(psa =>
				{
					var m = new ProductSpecificationModel
					{
						SpecificationAttributeId = psa.SpecificationAttributeOption.SpecificationAttributeId,
						SpecificationAttributeName = psa.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(x => x.Name),
						ColorSquaresRgb = psa.SpecificationAttributeOption.ColorSquaresRgb
					};

					switch (psa.AttributeType)
					{
						case SpecificationAttributeType.Option:
							m.ValueRaw = HttpUtility.HtmlEncode(psa.SpecificationAttributeOption.GetLocalized(x => x.Name));
							break;
						case SpecificationAttributeType.CustomText:
							m.ValueRaw = HttpUtility.HtmlEncode(psa.CustomValue);
							break;
						case SpecificationAttributeType.CustomHtmlText:
							m.ValueRaw = psa.CustomValue;
							break;
						case SpecificationAttributeType.Hyperlink:
							m.ValueRaw = string.Format("<a href='{0}' target='_blank'>{0}</a>", psa.CustomValue);
							break;
						default:
							break;
					}
					return m;
				}).ToList()
			);
		}

		public static IEnumerable<ProductOverviewModel> PrepareProductOverviewModels(this Controller controller,
			IWorkContext workContext,
			IStoreContext storeContext,
			ICategoryService categoryService,
			IProductService productService,
			ISpecificationAttributeService specificationAttributeService,
			IPriceCalculationService priceCalculationService,
			IPriceFormatter priceFormatter,
			IPermissionService permissionService,
			ILocalizationService localizationService,
			ITaxService taxService,
			ICurrencyService currencyService,
			IPictureService pictureService,
			IMeasureService measureService,
			IWebHelper webHelper,
			ICacheManager cacheManager,
			CatalogSettings catalogSettings,
			MediaSettings mediaSettings,
			IEnumerable<Product> products,
			bool preparePriceModel = true,
			bool preparePictureModel = true,
			int? productThumbPictureSize = null,
			bool prepareSpecificationAttributes = false,
			bool forceRedirectionAfterAddingToCart = false,
			bool showWishButton = true,
			bool showBuyButton = true)
		{
			if (products == null)
				throw new ArgumentNullException("products");

			var wishList = GetWishListIds(workContext, storeContext);
			var models = new List<ProductOverviewModel>();
			foreach (var product in products)
			{
				var model = new ProductOverviewModel
				{
					Id = product.Id,
					Name = product.GetLocalized(x => x.Name),
					ManufacturerName = product.GetLocalized(x => x.ProductManufacturers.First().Manufacturer.Name),
					ShortDescription = product.GetLocalized(x => x.ShortDescription),
					FullDescription = product.GetLocalized(x => x.FullDescription),
					SeName = product.GetSeName(),
					ProductType = product.ProductType,
					ShowBuyButton = showBuyButton,
					WishListModel = new ProductWishListModel
					{
						IsInWishList = wishList.Contains(product.Id),
						ProductId = product.Id,
						ShowWishListButton = showWishButton
					},
					MarkAsNew = product.MarkAsNew &&
						(!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
						(!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow)
				};
				//price
				if (preparePriceModel)
				{
					#region Prepare product price

					var priceModel = new ProductOverviewModel.ProductPriceModel
					{
						ForceRedirectionAfterAddingToCart = forceRedirectionAfterAddingToCart
					};

					switch (product.ProductType)
					{
						case ProductType.GroupedProduct:
							{
								#region Grouped product

								var associatedProducts = productService.GetAssociatedProducts(product.Id, storeContext.CurrentStore.Id);

								//add to cart button (ignore "DisableBuyButton" property for grouped products)
								priceModel.DisableBuyButton = !permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart) ||
									!permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

								//add to wishlist button (ignore "DisableWishlistButton" property for grouped products)
								priceModel.DisableWishlistButton = !permissionService.Authorize(StandardPermissionProvider.EnableWishlist) ||
									!permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

								//compare products
								priceModel.DisableAddToCompareListButton = !catalogSettings.CompareProductsEnabled;
								switch (associatedProducts.Count)
								{
									case 0:
										{
											//no associated products
										}
										break;
									default:
										{
											//we have at least one associated product
											//compare products
											priceModel.DisableAddToCompareListButton = !catalogSettings.CompareProductsEnabled;
											//priceModel.AvailableForPreOrder = false;

											if (permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
											{
												//find a minimum possible price
												decimal? minPossiblePrice = null;
												Product minPriceProduct = null;
												foreach (var associatedProduct in associatedProducts)
												{
													//calculate for the maximum quantity (in case if we have tier prices)
													var tmpPrice = priceCalculationService.GetFinalPrice(associatedProduct,
														workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue);
													if (!minPossiblePrice.HasValue || tmpPrice < minPossiblePrice.Value)
													{
														minPriceProduct = associatedProduct;
														minPossiblePrice = tmpPrice;
													}
												}
												if (minPriceProduct != null && !minPriceProduct.CustomerEntersPrice)
												{
													if (minPriceProduct.CallForPrice)
													{
														priceModel.OldPrice = null;
														priceModel.Price = localizationService.GetResource("Products.CallForPrice");
													}
													else if (minPossiblePrice.HasValue)
													{
														//calculate prices
														decimal taxRate;
														decimal finalPriceBase = taxService.GetProductPrice(minPriceProduct, minPossiblePrice.Value, out taxRate);
														decimal finalPrice = currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, workContext.WorkingCurrency);

														priceModel.OldPrice = null;
														priceModel.Price = String.Format(localizationService.GetResource("Products.PriceRangeFrom"), priceFormatter.FormatPrice(finalPrice));
														priceModel.PriceValue = finalPrice;

														//PAngV baseprice (used in Germany)
														priceModel.BasePricePAngV = product.FormatBasePrice(finalPrice,
															localizationService, measureService, currencyService, workContext, priceFormatter);
													}
													else
													{
														//Actually it's not possible (we presume that minimalPrice always has a value)
														//We never should get here
														Debug.WriteLine("Cannot calculate minPrice for product #{0}", product.Id);
													}
												}
											}
											else
											{
												//hide prices
												priceModel.OldPrice = null;
												priceModel.Price = null;
											}
										}
										break;
								}

								#endregion
							}
							break;
						case ProductType.SimpleProduct:
						default:
							{
								#region Simple product

								//add to cart button
								priceModel.DisableBuyButton = product.DisableBuyButton ||
									!permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart) ||
									!permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

								//add to wishlist button
								priceModel.DisableWishlistButton = product.DisableWishlistButton ||
									!permissionService.Authorize(StandardPermissionProvider.EnableWishlist) ||
									!permissionService.Authorize(StandardPermissionProvider.DisplayPrices);
								//compare products
								priceModel.DisableAddToCompareListButton = !catalogSettings.CompareProductsEnabled;

								//rental
								priceModel.IsRental = product.IsRental;

								//pre-order
								if (product.AvailableForPreOrder)
								{
									priceModel.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
										product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow;
									priceModel.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
								}

								//prices
								if (permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
								{
									if (!product.CustomerEntersPrice)
									{
										if (product.CallForPrice)
										{
											//call for price
											priceModel.OldPrice = null;
											priceModel.Price = localizationService.GetResource("Products.CallForPrice");
										}
										else
										{
											//prices

											//calculate for the maximum quantity (in case if we have tier prices)
											decimal minPossiblePrice = priceCalculationService.GetFinalPrice(product,
												workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue);

											decimal taxRate;
											decimal oldPriceBase = taxService.GetProductPrice(product, product.OldPrice, out taxRate);
											decimal finalPriceBase = taxService.GetProductPrice(product, minPossiblePrice, out taxRate);

											decimal oldPrice = currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, workContext.WorkingCurrency);
											decimal finalPrice = currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, workContext.WorkingCurrency);

											//do we have tier prices configured?
											var tierPrices = new List<TierPrice>();
											if (product.HasTierPrices)
											{
												tierPrices.AddRange(product.TierPrices
													.OrderBy(tp => tp.Quantity)
													.ToList()
													.FilterByStore(storeContext.CurrentStore.Id)
													.FilterForCustomer(workContext.CurrentCustomer)
													.RemoveDuplicatedQuantities());
											}
											//When there is just one tier (with  qty 1), 
											//there are no actual savings in the list.
											bool displayFromMessage = tierPrices.Any() &&
												!(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1);
											if (displayFromMessage)
											{
												priceModel.OldPrice = null;
												priceModel.Price = String.Format(localizationService.GetResource("Products.PriceRangeFrom"), priceFormatter.FormatPrice(finalPrice));
												priceModel.PriceValue = finalPrice;
											}
											else
											{
												if (finalPriceBase != oldPriceBase && oldPriceBase != decimal.Zero)
												{
													priceModel.OldPrice = priceFormatter.FormatPrice(oldPrice);
													priceModel.Price = priceFormatter.FormatPrice(finalPrice);
													priceModel.PriceValue = finalPrice;
												}
												else
												{
													priceModel.OldPrice = null;
													priceModel.Price = priceFormatter.FormatPrice(finalPrice);
													priceModel.PriceValue = finalPrice;
												}
											}
											if (product.IsRental)
											{
												//rental product
												priceModel.OldPrice = priceFormatter.FormatRentalProductPeriod(product, priceModel.OldPrice);
												priceModel.Price = priceFormatter.FormatRentalProductPeriod(product, priceModel.Price);
											}


											//property for German market
											//we display tax/shipping info only with "shipping enabled" for this product
											//we also ensure this it's not free shipping
											priceModel.DisplayTaxShippingInfo = catalogSettings.DisplayTaxShippingInfoProductBoxes
												&& product.IsShipEnabled &&
												!product.IsFreeShipping;


											//PAngV baseprice (used in Germany)
											priceModel.BasePricePAngV = product.FormatBasePrice(finalPrice,
												localizationService, measureService, currencyService, workContext, priceFormatter);
										}
									}
								}
								else
								{
									//hide prices
									priceModel.OldPrice = null;
									priceModel.Price = null;
								}

								#endregion
							}
							break;
					}

					model.ProductPrice = priceModel;

					#endregion
				}

				//picture
				if (preparePictureModel)
				{
					#region Prepare product picture

					//If a size has been set in the view, we use it in priority
					int pictureSize = productThumbPictureSize.HasValue ? productThumbPictureSize.Value : mediaSettings.ProductThumbPictureSize;
					//prepare picture model
					var defaultProductPictureCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DEFAULTPICTURE_MODEL_KEY, product.Id, pictureSize, true, workContext.WorkingLanguage.Id, webHelper.IsCurrentConnectionSecured(), storeContext.CurrentStore.Id);
					model.DefaultPictureModel = cacheManager.Get(defaultProductPictureCacheKey, () =>
					{
						var picture = pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
						var pictureModel = new PictureModel
						{
							ImageUrl = pictureService.GetPictureUrl(picture, pictureSize),
							FullSizeImageUrl = pictureService.GetPictureUrl(picture)
						};
						//"title" attribute
						pictureModel.Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute)) ?
							picture.TitleAttribute :
							string.Format(localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name);
						//"alt" attribute
						pictureModel.AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute)) ?
							picture.AltAttribute :
							string.Format(localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name);

						return pictureModel;
					});

					#endregion
				}

				//specs
				if (prepareSpecificationAttributes)
				{
					model.SpecificationAttributeModels = PrepareProductSpecificationModel(controller, workContext,
						 specificationAttributeService, cacheManager, product);
				}

				//reviews
				model.ReviewOverviewModel = controller.PrepareProductReviewOverviewModel(storeContext, catalogSettings, cacheManager, product);

				models.Add(model);
			}
			return models;
		}

		public static ProductReviewOverviewModel PrepareProductReviewOverviewModel(this Controller controller,
			IStoreContext storeContext,
			CatalogSettings catalogSettings,
			ICacheManager cacheManager,
			Product product)
		{
			ProductReviewOverviewModel productReview;

			if (catalogSettings.ShowProductReviewsPerStore)
			{
				string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_REVIEWS_MODEL_KEY, product.Id, storeContext.CurrentStore.Id);

				productReview = cacheManager.Get(cacheKey, () =>
				{
					return new ProductReviewOverviewModel
					{
						RatingSum = product.ProductReviews
								.Where(pr => pr.IsApproved && pr.StoreId == storeContext.CurrentStore.Id)
								.Sum(pr => pr.Rating),
						TotalReviews = product
								.ProductReviews
								.Count(pr => pr.IsApproved && pr.StoreId == storeContext.CurrentStore.Id)
					};
				});
			}
			else
			{
				productReview = new ProductReviewOverviewModel()
				{
					RatingSum = product.ApprovedRatingSum,
					TotalReviews = product.ApprovedTotalReviews
				};
			}
			if (productReview != null)
			{
				productReview.ProductId = product.Id;
				productReview.AllowCustomerReviews = product.AllowCustomerReviews;
			}
			return productReview;
		}

		private static IList<int> GetWishListIds(
			IWorkContext workContext,
			IStoreContext storeContext)
		{
			Customer customer = workContext.CurrentCustomer;
			if (customer != null)
			{
				return customer.ShoppingCartItems
					.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
					.LimitPerStore(storeContext.CurrentStore.Id)
					.Select(sci => sci.ProductId)
					.ToList();
			}

			return null;
		}

		[NonAction]
		public static void PrepareSortingOptions(this Controller controller,
			CatalogPagingFilteringModel pagingFilteringModel,
			CatalogPagingFilteringModel command,
			IWebHelper webHelper,
			IWorkContext workContext,
			ILocalizationService localizationService,
			CatalogSettings catalogSettings)
		{
			CheckForNull(pagingFilteringModel, command);

			var allDisabled = catalogSettings.ProductSortingEnumDisabled.Count == Enum.GetValues(typeof(ProductSortingEnum)).Length;
			pagingFilteringModel.AllowProductSorting = catalogSettings.AllowProductSorting && !allDisabled;

			var activeOptions = Enum.GetValues(typeof(ProductSortingEnum)).Cast<int>()
				.Except(catalogSettings.ProductSortingEnumDisabled)
				.Select((idOption) =>
				{
					int order;
					return new KeyValuePair<int, int>(idOption,
						catalogSettings.ProductSortingEnumDisplayOrder.TryGetValue(idOption, out order) ? order : idOption);
				})
				.OrderBy(x => x.Value);

			if (command.OrderBy == null)
			{
				command.OrderBy = allDisabled ? 0 : activeOptions.First().Key;
			}

			if (pagingFilteringModel.AllowProductSorting)
			{
				foreach (var option in activeOptions)
				{
					var sortText = ((ProductSortingEnum)option.Key).GetLocalizedEnum(localizationService, workContext);
					var currentPageUrl = webHelper.GetThisPageUrl(true);

					var listItem = new SelectListItem
					{
						Text = sortText,
						Value = webHelper.ModifyQueryString(currentPageUrl, "orderby=" + (option.Key).ToString(), null),
						Selected = option.Key == command.OrderBy
					};

					pagingFilteringModel.AvailableSortOptions.Add(listItem);
				}
			}
		}

		[NonAction]
		public static void PrepareViewModes(this Controller controller,
			CatalogPagingFilteringModel pagingFilteringModel,
			CatalogPagingFilteringModel command,
			IWebHelper webHelper,
			ILocalizationService localizationService,
			CatalogSettings catalogSettings)
		{
			CheckForNull(pagingFilteringModel, command);

			pagingFilteringModel.AllowProductViewModeChanging = catalogSettings.AllowProductViewModeChanging;

			var viewMode = !string.IsNullOrEmpty(command.ViewMode)
				? command.ViewMode
				: catalogSettings.DefaultViewMode;
			pagingFilteringModel.ViewMode = viewMode;

			if (pagingFilteringModel.AllowProductViewModeChanging)
			{
				var currentPageUrl = webHelper.GetThisPageUrl(true);

				var gridSelectListItem = new SelectListItem
				{
					Text = localizationService.GetResource("Catalog.ViewMode.Grid"),
					Selected = viewMode == "grid",
					Value = webHelper.ModifyQueryString(currentPageUrl, "viewmode=grid", null)
				};
				var listSelectListItem = new SelectListItem
				{
					Text = localizationService.GetResource("Catalog.ViewMode.List"),
					Selected = viewMode == "list",
					Value = webHelper.ModifyQueryString(currentPageUrl, "viewmode=list", null)
				};

				pagingFilteringModel.AvailableViewModes.Add(gridSelectListItem);
				pagingFilteringModel.AvailableViewModes.Add(listSelectListItem);
			}

		}

		[NonAction]
		public static void PreparePageSizeOptions(this Controller controller,
			CatalogPagingFilteringModel pagingFilteringModel,
			CatalogPagingFilteringModel command,
			IWebHelper webHelper,
			bool allowCustomersToSelectPageSize,
			string pageSizeOptions,
			int fixedPageSize)
		{
			CheckForNull(pagingFilteringModel, command);

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
						int temp;
						if (int.TryParse(pageSizes.FirstOrDefault(), out temp))
						{
							if (temp > 0)
							{
								command.PageSize = temp;
							}
						}
					}

					var currentPageUrl = webHelper.GetThisPageUrl(true);
					var sortUrl = webHelper.ModifyQueryString(currentPageUrl, "pagesize={0}", null);
					sortUrl = webHelper.RemoveQueryString(sortUrl, "pagenumber");

					foreach (var pageSize in pageSizes)
					{
						int temp;
						if (!int.TryParse(pageSize, out temp))
						{
							continue;
						}
						if (temp <= 0)
						{
							continue;
						}

						var listItem = new SelectListItem
						{
							Text = pageSize,
							Value = String.Format(sortUrl, pageSize),
							Selected = pageSize.Equals(command.PageSize.ToString(), StringComparison.InvariantCultureIgnoreCase)
						};

						pagingFilteringModel.PageSizeOptions.Add(listItem);
					}

					if (pagingFilteringModel.PageSizeOptions.Any())
					{
						pagingFilteringModel.PageSizeOptions = pagingFilteringModel.PageSizeOptions.OrderBy(x => int.Parse(x.Text)).ToList();
						pagingFilteringModel.AllowCustomersToSelectPageSize = true;

						if (command.PageSize <= 0)
						{
							command.PageSize = int.Parse(pagingFilteringModel.PageSizeOptions.FirstOrDefault().Text);
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

		private static void CheckForNull(CatalogPagingFilteringModel pagingFilteringModel, CatalogPagingFilteringModel command)
		{
			if (pagingFilteringModel == null)
				throw new ArgumentNullException("pagingFilteringModel");

			if (command == null)
				throw new ArgumentNullException("command");
		}
	}
}
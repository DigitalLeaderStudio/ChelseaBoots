using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Security;
using Nop.Web.Models.Catalog;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Nop.Web.Controllers
{
	public partial class ProductController : BasePublicController
	{
		#region New products for home page

		[NopHttpsRequirement(SslRequirement.No)]
		public ActionResult HomepageNewProducts()
		{
			if (!_catalogSettings.NewProductsEnabled)
				return Content("");

			var products = _productService.SearchProducts(
				storeId: _storeContext.CurrentStore.Id,
				visibleIndividuallyOnly: false,
				markedAsNewOnly: true,
				orderBy: ProductSortingEnum.CreatedOn,
				pageSize: _catalogSettings.NewProductsNumber);

			var model = new List<ProductOverviewModel>();
			model.AddRange(PrepareProductOverviewModels(products, prepareSpecificationAttributes: true));

			return View(model);
		}

		#endregion
	}
}

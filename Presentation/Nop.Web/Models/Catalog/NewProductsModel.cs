using Nop.Web.Framework.Mvc;
using System.Collections.Generic;

namespace Nop.Web.Models.Catalog
{
	public partial class NewProductsModel : BaseNopEntityModel
	{
		public NewProductsModel()
		{
			Products = new List<ProductOverviewModel>();
			PagingFilteringContext = new CatalogPagingFilteringModel();
		}

		public CatalogPagingFilteringModel PagingFilteringContext { get; set; }

		public List<ProductOverviewModel> Products { get; set; }
	}
}
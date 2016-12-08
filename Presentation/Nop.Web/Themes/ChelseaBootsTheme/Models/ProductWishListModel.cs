using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Themes.ChelseaBootsTheme.Models
{
	public class ProductWishListModel : BaseNopModel
	{
		public int ProductId { get; set; }

		public bool IsInWishList { get; set; }

		public bool ShowWishListButton { get; set; }

	}
}
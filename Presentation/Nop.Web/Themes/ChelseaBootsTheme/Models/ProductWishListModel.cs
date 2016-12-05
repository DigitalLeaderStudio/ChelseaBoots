using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Themes.ChelseaBootsTheme.Models
{
	public class ProductWishListModel : BaseNopModel
	{
		public bool IsInWishList { get; set; }

		public int ProductId { get; set; }
	}
}
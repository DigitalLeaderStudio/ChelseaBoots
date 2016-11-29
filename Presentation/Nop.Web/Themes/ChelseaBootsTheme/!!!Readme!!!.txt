To use this theme
Nop.Web.Infrastructure.GenericUrlRouteProvider change the controller namespace to Nop.Web.Themes.ChelseaBootsTheme.Controllers:

	routes.MapLocalizedRoute("Category",
							"{SeName}",
							new { controller = "Catalog", action = "Category" },
							new[] { "Nop.Web.Controllers" });

 Change to 
		
	routes.MapLocalizedRoute("Category",
							"{SeName}",
							new { controller = "Catalog", action = "Category" },
							new[] { "Nop.Web.Themes.ChelseaBootsTheme.Controllers" });
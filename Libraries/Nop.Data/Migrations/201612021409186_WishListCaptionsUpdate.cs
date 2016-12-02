namespace Nop.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WishListCaptionsUpdate : DbMigration
    {
        public override void Up()
        {
			//EN
			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (1
					   ,'ShoppingCart.RemoveFromWishlist'
					   ,'Remove')");

			this.Sql(@"Update LocaleStringResource
				Set ResourceValue = 'To favorites'
				WHere ResourceName = 'ShoppingCart.AddToWishlist'
				and LanguageId = 1");

			this.Sql(@"Update LocaleStringResource
				Set ResourceValue = 'The product is added to the <a href=""{0}"">favorites</a>'
				Where ResourceName = 'Products.ProductHasBeenAddedToTheWishlist.Link'
				and LanguageId = 1");

			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (1
					   ,'Products.ProductHasBeenRemovedFromTheWishlist.Link'
					   ,'The product has been removed from the <a href=""{0}"">favorites</a>')");

			//RU
			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (2
					   ,'ShoppingCart.RemoveFromWishlist'
					   ,'������')");

			this.Sql(@"Update LocaleStringResource
				Set ResourceValue = '����� �������� � <a href=""{0}"">���������</a>'
				Where ResourceName = 'Products.ProductHasBeenAddedToTheWishlist.Link'
				and LanguageId = 2");

			this.Sql(@"Update LocaleStringResource
				Set ResourceValue = '� ���������'
				WHere ResourceName = 'ShoppingCart.AddToWishlist'
				and LanguageId = 2");

			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (2
					   ,'Products.ProductHasBeenRemovedFromTheWishlist.Link'
					   ,'����� ����� �� <a href=""{0}"">����������</a>')");

			//UKR
			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (3
					   ,'ShoppingCart.RemoveFromWishlist'
					   ,'�������')");

			this.Sql(@"Update LocaleStringResource
				Set ResourceValue = '� ���������'
				WHere ResourceName = 'ShoppingCart.AddToWishlist'
				and LanguageId = 3");
			
			this.Sql(@"Update LocaleStringResource
				Set ResourceValue = '����� ������ �� <a href=""{0}"">���������</a>'
				Where ResourceName = 'Products.ProductHasBeenAddedToTheWishlist.Link'
				and LanguageId = 3");

			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (3
					   ,'Products.ProductHasBeenRemovedFromTheWishlist.Link'
					   ,'����� �������� � <a href=""{0}"">���������</a>')");
        }
        
        public override void Down()
        {
			this.Sql(@"Delete from LocaleStringResource where ResourceName='ShoppingCart.RemoveFromWishlist'");
        }
    }
}

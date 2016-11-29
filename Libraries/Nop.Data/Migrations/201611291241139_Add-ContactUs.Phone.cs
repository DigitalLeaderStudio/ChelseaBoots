namespace Nop.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddContactUsPhone : DbMigration
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
					   ,'ContactUs.Phone'
					   ,'Enter your phone')");

			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (1
					   ,'ContactUs.Phone.Hint'
					   ,'Your contact phone')");
			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (1
					   ,'ContactUs.Phone.Required'
					   ,'We cannot contact you withou phone')");

			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (1
					   ,'ContactUs.Phone.WrongFormat'
					   ,'Wrong phone format')");

			//RU
			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (2
					   ,'ContactUs.Phone'
					   ,'Ваш телефон')");

			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (2
					   ,'ContactUs.Phone.Hint'
					   ,'Ваш контактний телефон')");
			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (2
					   ,'ContactUs.Phone.Required'
					   ,'Без телефона мы не сможем с Вами связаться')");

			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (2
					   ,'ContactUs.Phone.WrongFormat'
					   ,'Не правильный формат номера телефона')");

			//Ukr
			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (3
					   ,'ContactUs.Phone'
					   ,'Ваш телефон')");

			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (3
					   ,'ContactUs.Phone.Hint'
					   ,'Ваш контактний телефон')");
			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (3
					   ,'ContactUs.Phone.Required'
					   ,'Без телефону мы не зможемо зв`язатися з Вами')");

			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (3
					   ,'ContactUs.Phone.WrongFormat'
					   ,'Не правильный формат номера телефона')");


			//ContactUs.Prompt
			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (1
					   ,'ContactUs.Prompt'
					   ,'Please mail us, call us, viber us, or fill the form and send it us. We will contact you.')");

			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (2
					   ,'ContactUs.Prompt'
					   ,'Звоните нам, пишите нам на email, пишите нам на Viber или заполнити форму ниже и отправьте нам. Мы обязательно с Вами свяжемся.')");

			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (3
					   ,'ContactUs.Prompt'
					   ,'Звоните нам, пишите нам на email, пишите нам на Viber или заполнити форму ниже и отправьте нам. Мы обязательно с Вами свяжемся.')");
		}

		public override void Down()
		{
			this.Sql(@"DELETE FROM [dbo].[LocaleStringResource]
				WHERE ResourceName like 'ContactUs.Phone%'");

			this.Sql(@"DELETE FROM [dbo].[LocaleStringResource]
				WHERE ResourceName like 'ContactUs.Prompt%'");
		}
    }
}

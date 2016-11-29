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
					   ,'��� �������')");

			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (2
					   ,'ContactUs.Phone.Hint'
					   ,'��� ���������� �������')");
			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (2
					   ,'ContactUs.Phone.Required'
					   ,'��� �������� �� �� ������ � ���� ���������')");

			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (2
					   ,'ContactUs.Phone.WrongFormat'
					   ,'�� ���������� ������ ������ ��������')");

			//Ukr
			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (3
					   ,'ContactUs.Phone'
					   ,'��� �������')");

			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (3
					   ,'ContactUs.Phone.Hint'
					   ,'��� ���������� �������')");
			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (3
					   ,'ContactUs.Phone.Required'
					   ,'��� �������� �� �� ������� ��`������� � ����')");

			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (3
					   ,'ContactUs.Phone.WrongFormat'
					   ,'�� ���������� ������ ������ ��������')");


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
					   ,'������� ���, ������ ��� �� email, ������ ��� �� Viber ��� ��������� ����� ���� � ��������� ���. �� ����������� � ���� ��������.')");

			this.Sql(@"INSERT INTO [dbo].[LocaleStringResource]
					   ([LanguageId]
					   ,[ResourceName]
					   ,[ResourceValue])
				 VALUES
					   (3
					   ,'ContactUs.Prompt'
					   ,'������� ���, ������ ��� �� email, ������ ��� �� Viber ��� ��������� ����� ���� � ��������� ���. �� ����������� � ���� ��������.')");
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

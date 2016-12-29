using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Localization;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.PayInStore.Models
{
	public class ConfigurationModel : BaseNopModel, ILocalizedModel<ConfigurationModel.ConfigurationLocalizedModel>
    {
		public ConfigurationModel()
		{
			Locales = new List<ConfigurationLocalizedModel>();
		}

        public int ActiveStoreScopeConfiguration { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("Plugins.Payment.PayInStore.DescriptionText")]
        public string DescriptionText { get; set; }
        public bool DescriptionText_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payment.PayInStore.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payment.PayInStore.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

		public partial class ConfigurationLocalizedModel : ILocalizedModelLocal
		{
			public int LanguageId { get; set; }

			[AllowHtml]
			[NopResourceDisplayName("Plugins.Payment.PayInStore.DescriptionText")]
			public string DescriptionText { get; set; }
		}

		public IList<ConfigurationLocalizedModel> Locales { get; set; }
	}
}
﻿using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.PayInStore.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.PayInStore.Controllers
{
	public class PaymentPayInStoreController : BasePaymentController
	{
		#region Fields

		private readonly ILocalizationService _localizationService;
		private readonly ILanguageService _languageService;
		private readonly ISettingService _settingService;
		private readonly IStoreContext _storeContext;
		private readonly IStoreService _storeService;
		private readonly IWorkContext _workContext;

		#endregion

		#region Ctor

		public PaymentPayInStoreController(ILocalizationService localizationService,
			ILanguageService languageService,
			ISettingService settingService,
			IStoreContext storeContext,
			IStoreService storeService,
			IWorkContext workContext)
		{
			this._localizationService = localizationService;
			this._languageService = languageService;
			this._settingService = settingService;
			this._storeContext = storeContext;
			this._storeService = storeService;
			this._workContext = workContext;
		}

		#endregion

		#region Methods

		[AdminAuthorize]
		[ChildActionOnly]
		public ActionResult Configure()
		{
			//load settings for a chosen store scope
			var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var payInStorePaymentSettings = _settingService.LoadSetting<PayInStorePaymentSettings>(storeScope);

			var model = new ConfigurationModel
			{
				DescriptionText = payInStorePaymentSettings.DescriptionText,
				AdditionalFee = payInStorePaymentSettings.AdditionalFee,
				AdditionalFeePercentage = payInStorePaymentSettings.AdditionalFeePercentage,
				ActiveStoreScopeConfiguration = storeScope
			};

			//locales
			AddLocales(_languageService, model.Locales, (locale, languageId) =>
			{
				locale.DescriptionText = payInStorePaymentSettings.GetLocalizedSetting(x => x.DescriptionText, languageId, false, false);
			});

			if (storeScope > 0)
			{
				model.DescriptionText_OverrideForStore = _settingService.SettingExists(payInStorePaymentSettings, x => x.DescriptionText, storeScope);
				model.AdditionalFee_OverrideForStore = _settingService.SettingExists(payInStorePaymentSettings, x => x.AdditionalFee, storeScope);
				model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(payInStorePaymentSettings, x => x.AdditionalFeePercentage, storeScope);
			}

			return View("~/Plugins/Payments.PayInStore/Views/PaymentPayInStore/Configure.cshtml", model);
		}

		[HttpPost]
		[AdminAuthorize]
		[ChildActionOnly]
		public ActionResult Configure(ConfigurationModel model)
		{
			if (!ModelState.IsValid)
				return Configure();

			//load settings for a chosen store scope
			var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var payInStorePaymentSettings = _settingService.LoadSetting<PayInStorePaymentSettings>(storeScope);

			//save settings
			payInStorePaymentSettings.DescriptionText = model.DescriptionText;
			payInStorePaymentSettings.AdditionalFee = model.AdditionalFee;
			payInStorePaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

			/* We do not clear cache after each setting update.
			 * This behavior can increase performance because cached settings will not be cleared 
			 * and loaded from database after each update */
			_settingService.SaveSettingOverridablePerStore(payInStorePaymentSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
			_settingService.SaveSettingOverridablePerStore(payInStorePaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
			_settingService.SaveSettingOverridablePerStore(payInStorePaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

			//now clear settings cache
			_settingService.ClearCache();

			//localization. no multi-store support for localization yet.
			foreach (var localized in model.Locales)
			{
				payInStorePaymentSettings.SaveLocalizedSetting(x => x.DescriptionText,
					localized.LanguageId,
					localized.DescriptionText);
			}

			SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

			return Configure();
		}

		[ChildActionOnly]
		public ActionResult PaymentInfo()
		{
			var payInStorePaymentSettings = _settingService.LoadSetting<PayInStorePaymentSettings>(_storeContext.CurrentStore.Id);
			var model = new PaymentInfoModel
			{
				DescriptionText = payInStorePaymentSettings.GetLocalizedSetting(x => x.DescriptionText, _workContext.WorkingLanguage.Id)
			};

			return View("~/Plugins/Payments.PayInStore/Views/PaymentPayInStore/PaymentInfo.cshtml", model);
		}

		[NonAction]
		public override IList<string> ValidatePaymentForm(FormCollection form)
		{
			return new List<string>();
		}

		[NonAction]
		public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
		{
			return new ProcessPaymentRequest();
		}

		#endregion
	}
}
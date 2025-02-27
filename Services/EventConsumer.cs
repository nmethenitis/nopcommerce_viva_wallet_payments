﻿using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Payments.VivaWallet.Services
{
    public class EventConsumer :
        IConsumer<PageRenderingEvent>,
        IConsumer<ModelReceivedEvent<BaseNopModel>>
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IScheduleTaskService _scheduleTaskService;

        #endregion

        #region Ctor

        public EventConsumer(ILocalizationService localizationService,
            IPaymentPluginManager paymentPluginManager,
            IScheduleTaskService scheduleTaskService)
        {
            _localizationService = localizationService;
            _paymentPluginManager = paymentPluginManager;
            _scheduleTaskService = scheduleTaskService;
        }

        #endregion

        #region Methods


        public Task HandleEventAsync(PageRenderingEvent eventMessage)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Handle page rendering event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public void HandleEvent(PageRenderingEvent eventMessage)
        {
            if (eventMessage?.Helper?.ViewContext?.ActionDescriptor == null)
                return;

            //check whether the plugin is active
            if (!_paymentPluginManager.IsPluginActive(VivaDefaults.SystemName))
                return;

            //add js script to one page checkout
            if (eventMessage.GetRouteNames().Any(routeName => routeName.Equals("CheckoutOnePage"))) {
                eventMessage.Helper?.AddScriptParts(ResourceLocation.Footer, "https://www.vivapayments.com/web/checkout/js", excludeFromBundle: true);
            }
        }

        /// <summary>
        /// Handle model received event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public void HandleEvent(ModelReceivedEvent<BaseNopModel> eventMessage)
        {
            //whether received model is ScheduleTaskModel
            //if (!(eventMessage?.Model is ScheduleTaskModel scheduleTaskModel))
                return;

            //whether renew access token task is changed
            //var scheduleTask = _scheduleTaskService.GetTaskById(scheduleTaskModel.Id);
            //if (!scheduleTask?.Type.Equals(SquarePaymentDefaults.RenewAccessTokenTask) ?? true)
            //    return;

            ////check token renewal limit
            //var accessTokenRenewalPeriod = scheduleTaskModel.Seconds / 60 / 60 / 24;
            //if (accessTokenRenewalPeriod > SquarePaymentDefaults.AccessTokenRenewalPeriodMax)
            //{
            //    var error = string.Format(_localizationService.GetResource("Plugins.Payments.Square.AccessTokenRenewalPeriod.Error"),
            //        SquarePaymentDefaults.AccessTokenRenewalPeriodMax, SquarePaymentDefaults.AccessTokenRenewalPeriodRecommended);
            //    eventMessage.ModelState.AddModelError(string.Empty, error);
            //}
        }

        #endregion
    }
}

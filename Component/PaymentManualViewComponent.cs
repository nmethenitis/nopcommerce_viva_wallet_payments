using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.VivaWallet.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.VivaWallet.Component
{
    [ViewComponent(Name = "VivaWalletPayment")]
    public class PaymentManualViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var model = new VivaHelper();
            model.ProductName = "name1";
            return View("~/Plugins/Nop.Plugin.Payments.VivaWallet/Views/PaymentInfo.cshtml", model);
        }
    }
}

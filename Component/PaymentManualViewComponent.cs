using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.VivaWallet.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.VivaWallet.Component
{
    [ViewComponent(Name = "VivaWalletPayments")]
    public class PaymentManualViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            return View("~/Plugins/Nop.Plugin.Payments.VivaWallet/Views/PaymentInfo.cshtml", new VivaHelper());
        }
    }
}

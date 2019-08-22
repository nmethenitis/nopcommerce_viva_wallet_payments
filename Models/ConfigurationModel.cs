using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using System;

namespace Nop.Plugin.Payments.VivaWallet.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        public string VivaWalletEndPoint { get; set; }
        public bool VivaWalletEndPoint_OvverideForStore { get; set; }
        public string ApiKey { get; set; }
        public bool ApiKey_OvverideForStore { get; set; }
        public string ApiPassword { get; set; }
        public bool ApiPassword_OvverideForStore { get; set; }
        public string SourceCode { get; set; }
        public bool SourceCode_OvverideForStore { get; set; }
        public string VivaCheckoutUrl { get; set; }
        public bool VivaCheckoutUrl_OvverideForStore { get; set; }
        public Guid MerchantId { get; set; }
        public bool MerchantId_OvverideForStore { get; set; }
        public string BaseApiUrl { get; set; }
        public bool BaseApiUrl_OvverideForStore { get; set; }
        public string PaymentsUrl { get; set; }
        public bool PaymentsUrl_OvverideForStore { get; set; }
        public string PaymentsCreateOrderUrl { get; set; }
        public bool PaymentsCreateOrderUrl_OvverideForStore { get; set; }
        public string PublicKey { get; set; }
        public bool PublicKey_OvverideForStore { get; set; }
    }
}

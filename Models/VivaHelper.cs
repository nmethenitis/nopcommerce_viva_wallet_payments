using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.VivaWallet.Models
{
    public record VivaHelper : BaseNopModel
    {
        public VivaHelper()
        {

        }
        public string HiddenToken { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int ProductCredits { get; set; }
        public int OrderInDbId { get; set; }
        public bool NotTax { get; set; }
    }
}

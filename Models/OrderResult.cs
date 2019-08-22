using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.VivaWallet.Models
{
    public class OrderResult
    {
        public long OrderCode { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorText { get; set; }
        public DateTime TimeStamp { get; set; }

    }
}

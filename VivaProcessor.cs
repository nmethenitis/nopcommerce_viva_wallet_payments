using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.VivaWallet.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using RestSharp;
using RestSharp.Authenticators;

namespace Nop.Plugin.Payments.VivaWallet
{
    public class VivaProcessor : BasePlugin, IPaymentMethod
    {
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly VivaSettings _vivaSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        bool IPaymentMethod.SupportCapture => false;

        bool IPaymentMethod.SupportPartiallyRefund => false;

        bool IPaymentMethod.SupportRefund => false;

        bool IPaymentMethod.SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        public bool SkipPaymentInfo => false;

        public string PaymentMethodDescription => "Pay with your card";

        public VivaProcessor(ILocalizationService localizationService,
            IPaymentService paymentService,
            ISettingService settingService,
            IWebHelper webHelper, VivaSettings vivaSettings,
            IHttpContextAccessor httpContextAccessor)
        {
            _localizationService = localizationService;
            _paymentService = paymentService;
            _settingService = settingService;
            _webHelper = webHelper;
            _vivaSettings = vivaSettings;
            _httpContextAccessor = httpContextAccessor;
        }

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult();
        }

        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //it's not a redirection payment method. So we always return false
            return false;
        }

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        public Dictionary<string, object> DeserializeCustomValues(Order order)
        {
            throw new NotImplementedException();
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart, string paymentMethodSystemName)
        {
            return 0; // _paymentService.CalculateAdditionalFee(cart, 0, false);
        }

        public string GetMaskedCreditCardNumber(string creditCardNumber)
        {
            throw new NotImplementedException();
        }

        public RecurringPaymentType GetRecurringPaymentType(string paymentMethodSystemName)
        {
            throw new NotImplementedException();
        }

        public override void Install()
        {
            base.Install();
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //nothing
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest ProcessPaymentRequest)
        {
            ExecuteVivaPayment(ProcessPaymentRequest.OrderTotal);

            var result = new ProcessPaymentResult
            {
                NewPaymentStatus = Core.Domain.Payments.PaymentStatus.Paid
            };

            return result;
        }
       
        private void ExecuteVivaPayment(decimal amount)
        {
            var token = _httpContextAccessor.HttpContext.Request.Cookies["vvtkn"];
            var cl = new RestClient(_vivaSettings.BaseApiUrl)
            {
                Authenticator = new HttpBasicAuthenticator(
                  _vivaSettings.MerchantId.ToString(),
                  _vivaSettings.ApiKey)
            };

            var _orderCode = CreateOrder(amount * 100);

            var req = new RestRequest(_vivaSettings.PaymentsUrl, Method.POST) { RequestFormat = DataFormat.Json };
            req.AddJsonBody(new
            {
                OrderCode = _orderCode,
                SourceCode = _vivaSettings.SourceCode,
                CreditCard = new
                {
                    Token = token
                }
            });

            var res = cl.Execute<TransactionResult>(req);

            if (res.Data != null && res.Data.ErrorCode == 0 && res.Data.StatusId == "F")
            {
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("vvtkn");
            }
            else
            {
                throw new NotImplementedException();
            }
        }


        private long CreateOrder(decimal amount)
        {
            var cl = new RestClient(_vivaSettings.BaseApiUrl)
            {
                Authenticator = new HttpBasicAuthenticator(
                    _vivaSettings.MerchantId.ToString(),
                    _vivaSettings.ApiKey)
            };

            var req = new RestRequest(_vivaSettings.PaymentsCreateOrderUrl, Method.POST);

            req.AddJsonBody(new
            {
                Amount = amount,    // Amount is in cents
                SourceCode = _vivaSettings.SourceCode
            });

            try
            {
                var res = cl.Execute<OrderResult>(req);
                if (res.Data != null && res.Data.ErrorCode == 0)
                {
                    return res.Data.OrderCode;
                }
                else
                    return 0;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult
            {
                AllowStoringCreditCardNumber = false,
                Errors = new[] { "Void method not supported" }
            };

            return result;
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            return new RefundPaymentResult { Errors = new[] { "Refund method not supported" } };
        }



        public override void Uninstall()
        {
            base.Uninstall();
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/VivaWallet/Configure";
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string> { AdminWidgetZones.CustomerDetailsBlock };
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            return false;
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return 0; // _paymentService.CalculateAdditionalFee(cart, 0, false);
        }

        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }
        
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        public string GetPublicViewComponentName()
        {
            return "VivaWalletPayments";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.VivaWallet.Component;
using Nop.Plugin.Payments.VivaWallet.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
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
        private readonly ICustomerService customerService;
        private readonly IOrderProcessingService orderProcessingService;


        bool IPaymentMethod.SupportCapture => false;

        bool IPaymentMethod.SupportPartiallyRefund => false;

        bool IPaymentMethod.SupportRefund => true;

        bool IPaymentMethod.SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        public bool SkipPaymentInfo => false;

        public string PaymentMethodDescription => "Pay with your card";

        public VivaProcessor(ILocalizationService localizationService,
            IPaymentService paymentService,
            ISettingService settingService,
            IWebHelper webHelper, VivaSettings vivaSettings,
            IHttpContextAccessor httpContextAccessor,
            IOrderProcessingService orderProcessingService,
            ICustomerService customerService)
        {
            _localizationService = localizationService;
            _paymentService = paymentService;
            _settingService = settingService;
            _webHelper = webHelper;
            _vivaSettings = vivaSettings;
            _httpContextAccessor = httpContextAccessor;
            this.customerService = customerService;
        }

        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return Task.FromResult(new CancelRecurringPaymentResult());
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);

            //it's not a redirection payment method. So we always return false
            return Task.FromResult(false);
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
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



        public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //nothing
            return Task.CompletedTask;
        }

        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var customer = await customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
            var email = customer.Email;
            var fullName = $"{customer.FirstName} {customer.LastName}";

            var executeAndGetTransactionId = ExecuteVivaPayment(processPaymentRequest.OrderTotal, fullName, email, processPaymentRequest.OrderGuid.ToString());


            var result = new ProcessPaymentResult
            {
                NewPaymentStatus = Core.Domain.Payments.PaymentStatus.Paid,
                CaptureTransactionId = executeAndGetTransactionId
            };

            return result;
        }


        private string ExecuteVivaPayment(decimal amount, string customerFullName, string customerEmail, string nopTransactionCode)
        {
            var token = _httpContextAccessor.HttpContext.Request.Cookies["vvtkn"];
            var cl = new RestClient(_vivaSettings.BaseApiUrl)
            {
                Authenticator = new HttpBasicAuthenticator(
                  _vivaSettings.MerchantId.ToString(),
                  _vivaSettings.ApiKey)
            };

            var _orderCode = CreateOrder(amount * 100, customerFullName, customerEmail, nopTransactionCode);

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
                return res.Data.TransactionId.ToString();
            }
            else
            {
                throw new NotImplementedException();
            }
        }


        private long CreateOrder(decimal amount, string customerFullName, string customerEmail, string nopTransactionCode)
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
                SourceCode = _vivaSettings.SourceCode,
                RequestLang = "en",
                FullName = customerFullName,
                Email = customerEmail,
                MerchantTrns = nopTransactionCode,
                //CustomerTrns = "customer descri",
            });

            try
            {
                var res = cl.Execute<VivaResult>(req);
                if (res.Data != null && res.Data.ErrorCode == 0)
                {
                    return res.Data.OrderCode;
                }
                else
                    return 0;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult
            {
                AllowStoringCreditCardNumber = false,
                Errors = new[] { "Void method not supported" }
            };

            return Task.FromResult(result);
        }

        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            try
            {
                var amount = (int)(refundPaymentRequest.AmountToRefund * 100);
                VivaRefund(refundPaymentRequest.Order.CaptureTransactionId, amount.ToString());

                return Task.FromResult(new RefundPaymentResult
                {
                    NewPaymentStatus = refundPaymentRequest.IsPartialRefund
                        ? Core.Domain.Payments.PaymentStatus.PartiallyRefunded
                        : Core.Domain.Payments.PaymentStatus.Refunded
                });
            }
            catch (Exception)
            {
                throw;
            }

        }

        public void VivaRefund(string transactionId,string amount)
        {
            var cl = new RestClient(_vivaSettings.BaseApiUrl)
            {
                Authenticator = new HttpBasicAuthenticator(
                   _vivaSettings.MerchantId.ToString(),
                   _vivaSettings.ApiKey)
            };
           

            try
            {
                var req = new RestRequest("/api/transactions/" + transactionId + "?amount=" + amount, Method.DELETE);
                var res = cl.Execute<VivaResult>(req);
                if (!(res.Data.StatusId == "F" && res.Data.ErrorCode == 0))
                {
                    throw new Exception("Refund is not completed");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        private Dictionary<string, Dictionary<string, string>> AllResources { get; set; } =
            new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> GetResourcesEn()
        {
            var r = new Dictionary<string, string>
            {
                { "Plugins.Payments.VivaWallet.PaymentInfo.PleaseWait", "Please wait..." },
                { "Plugins.Payments.VivaWallet.PaymentInfo.Continue", "Continue" },
                { "Plugins.Payments.VivaWallet.PaymentInfo.Expiration", "Expiration"},
                { "Plugins.Payments.VivaWallet.PaymentInfo.CardNumber", "Card Number"},
                { "Plugins.Payments.VivaWallet.PaymentInfo.CardholderName", "Cardholder Name"},
                { "Plugins.Payments.VivaWallet.PaymentInfo.Description", "Pay by credit / debit card"}
            };
            return new Dictionary<string, Dictionary<string, string>>
            {
                { "en-GB", r }
            };
        }
        private Dictionary<string, Dictionary<string, string>> GetResourcesGr()
        {
            var r = new Dictionary<string, string>
            {
                { "Plugins.Payments.VivaWallet.PaymentInfo.PleaseWait", "Παρακαλώ περιμένετε..." },
                { "Plugins.Payments.VivaWallet.PaymentInfo.Continue", "Συνέχεια" },
                { "Plugins.Payments.VivaWallet.PaymentInfo.Expiration", "Ημερ. Λήξης"},
                { "Plugins.Payments.VivaWallet.PaymentInfo.CardNumber", "Αριθμός κάρτας"},
                { "Plugins.Payments.VivaWallet.PaymentInfo.CardholderName", "'Ονομ/νυμο κατόχου"},
                { "Plugins.Payments.VivaWallet.PaymentInfo.Description", "Πληρωμή με πιστωτική/χρεωστρική κάρτα"}
            };
            return new Dictionary<string, Dictionary<string, string>>
            {
                { "el-GR", r }
            };
        }

        public override async Task InstallAsync()
        {
            AllResources.Add(GetResourcesEn().First().Key, GetResourcesEn().First().Value);
            AllResources.Add(GetResourcesGr().First().Key, GetResourcesGr().First().Value);

            foreach (var item in AllResources)
            {
                foreach (var resources in item.Value)
                {
                    await _localizationService.AddOrUpdateLocaleResourceAsync(resources.Key, resources.Value, item.Key);
                }
            }

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //default is english
            var defautlResources = GetResourcesEn();

            foreach (var item in defautlResources)
            {
                await _localizationService.DeleteLocaleResourcesAsync(item.Key);
            }

            await base.UninstallAsync();
        }

        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/VivaWallet/Configure";
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string> { AdminWidgetZones.CustomerDetailsBlock };
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return 0; // _paymentService.CalculateAdditionalFee(cart, 0, false);
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            var reqeust = new ProcessPaymentRequest();

            return Task.FromResult(reqeust);
        }
        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("Plugins.Payments.VivaWallet.PaymentMethodDescription");
        }

        public Type GetPublicViewComponent()
        {
            return typeof(PaymentManualViewComponent);
        }
    }
}

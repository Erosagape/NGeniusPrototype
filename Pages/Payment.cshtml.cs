using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace Prototype.Pages
{
    public class PaymentModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly NGeniusManager _ngenius;
        public PaymentModel(IConfiguration config)
        {
            _config = config;
            _ngenius = new NGeniusManager(_config);
        }
        public void OnGet()
        {
            ViewData["Message"] = NGeniusToken.Error.message;
        }
        public void OnPostSubmit(CardPaymentParams payment)
        {
            try
            {
                string url = String.Format(_ngenius.Config.CardPaymentURL, _ngenius.Config.OutletID);
                var req = WebRequest.Create(url);
                req.Method = "POST";
                req.Headers.Add
                    (
                    "Authorization",
                    "Bearer " + NGeniusToken.Value.access_token
                    );
                CardPaymentRequest p = new CardPaymentRequest()
                {
                    order=new OrderRequest()
                    {
                        action=payment.action,
                        amount=new AmountStruct()
                        {
                            currencyCode=payment.currencyCode,
                            value=payment.amount
                        }
                    },
                    payment=new CardPayment()
                    {
                        pan=payment.pan,
                        expiry=payment.expiry,
                        cardholderName=payment.cardholderName,
                        cvv=payment.cvv
                    }
                };
                string json = JsonSerializer.Serialize(p);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                req.ContentType = "application/vnd.ni-payment.v2+json";
                req.ContentLength = bytes.Length;
                var stream = req.GetRequestStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
                var response = req.GetResponse();
                var res = response.GetResponseStream();
                var reader = new System.IO.StreamReader(res);
                var responseStr = reader.ReadToEnd();
                ViewData["Message"] = responseStr;
            }
            catch (Exception e)
            {
                ViewData["Message"] = e.Message;
            }
        }
    }
}

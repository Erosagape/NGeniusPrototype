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
            NGeniusErrorResponse err = new NGeniusErrorResponse();
            NGeniusPayment res = new NGeniusPayment();
            _ngenius.CreateCardPayment(payment, ref err, ref res);
            if (err.code == 100)
            {
                _ngenius.SaveOrder(res, ref err);
                ViewData["Message"] = "Order ID:" + res.orderReference + "<br>Payment ID:" + res.reference +"<br>" + err.message;
            } else
            {
                ViewData["Message"] = err.message;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Prototype;
namespace Prototype.Pages
{    
    public class ProjectModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly NGeniusManager _ngenius;
        public ProjectModel(IConfiguration config)
        {
            _config = config;
            _ngenius = new NGeniusManager(_config);
        }
        public string Token { get; set; }
        public string Message { get; set; }
        public OrderParams Order { get; set; }
        public void OnGet()
        {
            var token = new NGeniusTokenResponse();
            var error = new NGeniusErrorResponse();
            Token = NGeniusToken.Value.access_token;
            Message = NGeniusToken.Error.message;
        }
        public void OnPostSubmit(OrderParams order)
        {
            Token = NGeniusToken.Value.access_token;
            NGeniusOrder orderCreate=new NGeniusOrder();
            string msg = "";
            bool chk=_ngenius.CreateOrder(order,ref orderCreate,ref msg);
            var err = new NGeniusErrorResponse();
            if (chk) _ngenius.SaveOrder(orderCreate, ref err);
            Message = (chk? @"OK=<a href=""" + orderCreate._links.payment.href +@""">"+ orderCreate._links.payment.href + "</a>" :"ERROR=" + msg);
        }
    }
}

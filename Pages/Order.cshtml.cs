using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace Prototype.Pages
{
    public class OrderModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly NGeniusManager _ngenius;
        public OrderModel(IConfiguration config)
        {
            _config = config;
            _ngenius = new NGeniusManager(_config);
        }
        public class OrderQuery
        {
            [BindProperty]
            public string OrderID { get; set; }
        }
        public void OnPostSubmit(OrderQuery order)
        {
            NGeniusErrorResponse err = new NGeniusErrorResponse();
            _ngenius.UpdateOrder(_ngenius.Config.OutletID,order.OrderID ,ref err);
            ViewData["DataSource"] = _ngenius.GetOrder(_ngenius.Config.OutletID, ref err);
            ViewData["Message"] = err.message;
        }
        public void OnGet()
        {
            NGeniusErrorResponse err = new NGeniusErrorResponse();
            ViewData["DataSource"] = _ngenius.GetOrder(_ngenius.Config.OutletID,ref err);
            ViewData["Message"] = err.message;
        }
    }
}

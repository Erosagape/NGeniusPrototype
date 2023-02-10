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
        public void OnGet()
        {
            ViewData["Message"] = NGeniusToken.Error.message;
        }
    }
}

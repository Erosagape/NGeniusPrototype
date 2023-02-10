using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Prototype.Pages;
using Microsoft.Extensions.Configuration;

namespace Prototype.Pages
{
    public class IndexModel : PageModel
    {
        private readonly NGeniusManager _ngenius;
        private readonly IConfiguration _config;
        public IndexModel(IConfiguration config)
        {
            _config = config;
            _ngenius = new NGeniusManager(_config);
        }

        public void OnGet()
        {
            var token = new NGeniusTokenResponse();
            var error = new NGeniusErrorResponse();
            ViewData["Message"] =NGeniusToken.Value.access_token;
        }
    }
}

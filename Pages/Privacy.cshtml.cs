using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Prototype.Pages
{
    public class PrivacyModel : PageModel
    {
        private readonly IConfiguration _config;
        private NGeniusManager _ngenius;
        public PrivacyModel(IConfiguration config)
        {
            _config = config;
            _ngenius = new NGeniusManager(_config);
        }

        public void OnGet()
        {
            _ngenius.UpdateToken();
            ViewData["Message"] = NGeniusToken.Error.message;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
namespace Prototype.Pages
{
    public class TestOrderModel : PageModel
    {
        public class TestOrder
        {
            [BindProperty]
            public string dataPost { get; set; }
        }
        private readonly IConfiguration _config;
        private readonly NGeniusManager _ngenius;

        public TestOrderModel(IConfiguration config)
        {
            _config = config;
            _ngenius = new NGeniusManager(_config);
        }
        public void OnGet()
        {
            ViewData["Message"] = "Ready";
        }
        public void OnPostSubmit(TestOrder data)
        {
            string Message = "";
            try
            {
                var obj = JsonSerializer.Deserialize<NGeniusOrder>(data.dataPost);
                Message = JsonSerializer.Serialize(obj);
            } catch(Exception e)
            {
                Message = e.Message;
            }
            ViewData["Message"] = Message;
        }
    }
}

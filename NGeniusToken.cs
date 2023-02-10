using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Prototype.Pages
{
    public class NGeniusTokenResponse
    {
        public string access_token { get; set; }
        public int expire_in { get; set; }
        public int refresh_expires_in { get; set; }
        public string token_type { get; set; }
    }
    public static class NGeniusToken
    {
        public static bool IsGetToken { get; set; }
        public static NGeniusTokenResponse Value { get; set; }
        public static NGeniusErrorResponse Error { get; set; }
    }

}

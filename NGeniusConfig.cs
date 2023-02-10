using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Prototype
{
    public class NGeniusConfig
    {
        public string TokenRequestURL { get; set; }
        public string OrderURL { get; set; }
        public string CardPaymentURL { get; set; }
        public string APIKey { get; set; }
        public string OutletID { get; set; }
    }
    public struct NGeniusError
    {
        public string message { get; set; }
        public string localizedMessage { get; set; }
        public string errorCode { get; set; }
        public string domain { get; set; }
    }
    public class NGeniusErrorResponse
    {
        public string message { get; set; }
        public int code { get; set; }
        public NGeniusError errors { get; set; }
    }
    public class LinkStruct
    {
        public string href { get; set; }
    }
    public class AmountStruct
    {
        public string currencyCode { get; set; }
        public double value { get; set; }
    }
    public class PaymentMethodStruct
    {
        public List<string> card { get; set; }
    }
}

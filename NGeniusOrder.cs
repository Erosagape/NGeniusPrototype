using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Prototype.Pages
{
    public class OrderParams
    {
        [BindProperty]
        public string action { get; set; }
        [BindProperty]
        public string currencyCode { get; set; }
        [BindProperty]
        public double amount { get; set; }
    }
    public class NGeniusOrderLinks
    {
        public LinkStruct cancel { get; set; }
        [JsonPropertyName("cnp:payment-link")]
        public LinkStruct payment_link { get; set; }
        [JsonPropertyName("payment-authorization")]
        public LinkStruct payment_authorization { get; set; }
        public LinkStruct self { get; set; }
        [JsonPropertyName("tenant-brand")]
        public LinkStruct tenant_brand { get; set; }
        public LinkStruct payment { get; set; }
        [JsonPropertyName("merchant-brand")]
        public LinkStruct merchant_brand { get; set; }
    }
    public class OrderRequest
    {
        public string action { get; set; }
        public AmountStruct amount { get; set; }
    }    
    public class NGeniusOrderPayments
    {
        public List<NGeniusPayment> payment { get; set; }
    }
    public class NGeniusOrder
    {
        public string _id { get; set; }
        public NGeniusOrderLinks _links { get; set; }
        public string type { get; set; }
        public object merchantDefinedData { get; set; }
        public string action { get; set; }
        public AmountStruct amount { get; set; }
        public string language { get; set; }
        public object merchantAttributes { get; set; }
        public string reference { get; set; }
        public string outletId { get; set; }
        public string createDateTime { get; set; }
        public PaymentMethodStruct paymentMethods { get; set; }
        public string referrer { get; set; }
        public string formattedAmount { get; set; }
        public object formattedOrderSummary { get; set; }
        public NGeniusOrderPayments _embedded { get; set; }
    }
}

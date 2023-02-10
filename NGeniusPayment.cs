using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Prototype.Pages
{
    public class CardPaymentRequest
    {
        public OrderRequest order { get; set; }
        public CardPayment payment { get; set; }
    }
    public class CardPayment
    {
        public string pan { get; set; }
        public string expiry { get; set; }
        public string cvv { get; set; }
        public string cardholderName { get; set; }
    }
    public class CardPaymentParams
    {
        [BindProperty]
        public string action { get; set; }
        [BindProperty]
        public string currencyCode { get; set; }
        [BindProperty]
        public double amount { get; set; }
        [BindProperty]
        public string pan { get; set; }
        [BindProperty]
        public string expiry { get; set; }
        [BindProperty]
        public string cvv { get; set; }
        [BindProperty]
        public string cardholderName { get; set; }
    }

    public class NGeniusCuries
    {
        public string name { get; set; }
        public string href { get; set; }
        public bool templated { get; set; }            
    }
    public class NGeniusPaymentLinks
    {
        public LinkStruct self { get; set; }
        [JsonPropertyName("payment:card")]
        public LinkStruct payment_card { get; set; }
        public List<NGeniusCuries> curies { get; set; }
    }
    public class NGeniusPayment
    {
        public string _id { get; set; }
        public NGeniusPaymentLinks _links { get; set; }
        public string reference { get; set; }
        public string state { get; set; }
        public AmountStruct amount { get; set; }
        public string updateDateTime { get; set; }
        public string outletId { get; set; }
        public string orderReference { get; set; }
    }
}

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prototype.Pages;
using System.Net;
using System.Text.Json;

namespace Prototype
{
    public class NGeniusManager
    {
        public NGeniusConfig Config;
        public NGeniusManager(IConfiguration config)
        {
            Config = config.GetSection("NGeniusConfig").Get<NGeniusConfig>();
            if(!NGeniusToken.IsGetToken)
            {
                UpdateToken();  
            }
        }
        public void UpdateToken()
        {
            var token = new NGeniusTokenResponse();
            var err = new NGeniusErrorResponse();
            CallRequestToken(ref err, ref token);
            NGeniusToken.Error = err;
            if (err.code == 100)
            {
                NGeniusToken.Value = token;
                NGeniusToken.IsGetToken = true;
            }
        }
        public bool CreateOrder(OrderParams param,ref NGeniusOrder order, ref string err)
        {
            try
            {
                string url = String.Format(Config.OrderURL, Config.OutletID);
                var req = WebRequest.Create(url);
                req.Method = "POST";
                req.Headers.Add
                    (
                    "Authorization",
                    "Bearer " + NGeniusToken.Value.access_token
                    );
                OrderRequest p = new OrderRequest()
                {
                    action = param.action,
                    amount = new AmountStruct()
                    {
                        currencyCode = param.currencyCode,
                        value = param.amount
                    }
                };
                string json = JsonSerializer.Serialize(p);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                req.ContentType = "application/vnd.ni-payment.v2+json";
                req.ContentLength = bytes.Length;
                var stream = req.GetRequestStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
                var response = req.GetResponse();
                var res = response.GetResponseStream();
                var reader = new System.IO.StreamReader(res);
                var responseStr = reader.ReadToEnd();
                order = JsonSerializer.Deserialize<NGeniusOrder>(responseStr);
                err = order.reference;
                return true;
            }
            catch (Exception e)
            {
                err = e.Message;
                return false;
            }
        }
        private bool CallRequestToken(ref NGeniusErrorResponse err, ref NGeniusTokenResponse token)
        {
            string responseText = "";
            err = new NGeniusErrorResponse();
            token = new NGeniusTokenResponse();
            try
            {
                string url = Config.TokenRequestURL;
                var req = WebRequest.Create(url);
                req.Method = "POST";
                req.Headers.Add
                    (
                    "Authorization",
                    "Basic " + Config.APIKey
                    );
                req.ContentType = "application/vnd.ni-identity.v1+json";
                var stream = req.GetRequestStream();
                stream.Close();
                var response = req.GetResponse();
                var res = response.GetResponseStream();
                var reader = new System.IO.StreamReader(res);
                responseText = reader.ReadToEnd();
                err.code = 100;
                err.message = "Ready";
            }
            catch (Exception ex)
            {
                responseText = ex.ToString();
                err.code = 400;
                err.message = ex.Message;
                err.errors = new NGeniusError()
                {
                    errorCode = "400",
                    domain = "identity",
                    localizedMessage = ex.Message,
                    message = ex.Message
                };
            }
            bool fail = (responseText.IndexOf("error") > 0);
            if (!fail)
            {
                token = JsonSerializer.Deserialize<NGeniusTokenResponse>(responseText);
            }
            return !fail;
        }

    }

}

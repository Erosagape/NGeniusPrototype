using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
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
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
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
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
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
        public void CreateCardPayment(CardPaymentParams payment, ref NGeniusErrorResponse err, ref NGeniusPayment obj)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = String.Format(Config.CardPaymentURL, Config.OutletID);
                var req = WebRequest.Create(url);
                req.Method = "POST";
                req.Headers.Add
                    (
                    "Authorization",
                    "Bearer " + NGeniusToken.Value.access_token
                    );
                CardPaymentRequest p = new CardPaymentRequest()
                {
                    order = new OrderRequest()
                    {
                        action = payment.action,
                        amount = new AmountStruct()
                        {
                            currencyCode = payment.currencyCode,
                            value = payment.amount
                        }
                    },
                    payment = new CardPayment()
                    {
                        pan = payment.pan,
                        expiry = payment.expiry,
                        cvv = payment.cvv,
                        cardholderName = payment.cardholderName
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
                obj = JsonSerializer.Deserialize<NGeniusPayment>(responseStr);
                err.code = 100;
                err.message = obj.reference;
            }
            catch (Exception e)
            {
                err.code = 400;
                err.message = e.Message;
            }
        }
        public void UpdateOrder(string outletid,string orderid,ref NGeniusErrorResponse err)
        {
            try
            {
                using(SqlConnection cn=new SqlConnection(Config.MainConnection))
                {
                    cn.Open();
                    string sql = String.Format(@"SELECT * FROM OrderLog WHERE outletId='{0}' AND orderRef='{1}'",outletid,orderid);
                    using(SqlDataReader rd=new SqlCommand(sql, cn).ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            ServicePointManager.Expect100Continue = true;
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            string url = String.Format(Config.OrderURL, Config.OutletID) + '/' + orderid;
                            var req = WebRequest.Create(url);
                            req.Method = "GET";
                            req.Headers.Add
                                (
                                "Authorization",
                                "Bearer " + NGeniusToken.Value.access_token
                                );
                            try
                            {
                                var response = req.GetResponse();
                                var res = response.GetResponseStream();
                                var reader = new System.IO.StreamReader(res);
                                var responseText = reader.ReadToEnd();
                                var obj = JsonSerializer.Deserialize<NGeniusOrder>(responseText);
                                SaveOrder(obj,ref err);
                            }
                            catch (Exception ex)
                            {
                                err.message += @"\n[ERROR] (" + rd["orderRef"].ToString() + @")" + ex.Message;
                            }
                        }
                        rd.Close();
                    }
                    cn.Close();
                }
            } catch(Exception e)
            {
                err.code = 400;
                err.message = e.Message;
            }
        }
        public DataTable GetOrder(string outletid,ref NGeniusErrorResponse err)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection cn = new SqlConnection(Config.MainConnection))
                {
                    cn.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(String.Format("SELECT * FROM OrderLog where outletId='{0}'", outletid), cn))
                    {
                        da.Fill(dt);
                    }
                }
            } catch (Exception e)
            {
                err.code = 400;
                err.message = e.Message;
            }
            return dt;
        }
        public void SaveOrder(NGeniusOrder order,ref NGeniusErrorResponse err)
        {
            string sql = "";
            try
            {
                using (SqlConnection cn = new SqlConnection(Config.MainConnection))
                {
                    cn.Open();
                    bool isExisting = false;
                    using (SqlDataReader rd = new SqlCommand(string.Format("SELECT * FROM OrderLog where orderRef='{0}'",order.reference), cn).ExecuteReader())
                    {
                        isExisting = rd.HasRows;
                        rd.Close();
                    }
                    string fields = @"outletId,orderRef,paymentRef,responseData,requestURL,currencyCode,amountValue,transType,paymentType,paymentData,isPaymented,isCancel,createDateTime,updateDateTime,status";
                    if (!isExisting)
                    {
                        sql = @"INSERT INTO OrderLog (" + fields + ") VALUES(";
                        sql += String.Format("'{0}'",order.outletId);
                        sql += String.Format(",'{0}'", order.reference);
                        sql += String.Format(",'{0}'", order._embedded.payment[0].reference);
                        sql += String.Format(",'{0}'", JsonSerializer.Serialize<NGeniusOrder>(order).ToString());
                        sql += String.Format(",'{0}'", order._links.payment.href);
                        sql += String.Format(",'{0}'", order.amount.currencyCode);
                        sql += String.Format(",{0}", order.amount.value);
                        sql += String.Format(",'{0}'", order.action);
                        sql += String.Format(",'{0}'", order.type);
                        sql += String.Format(",'{0}'", JsonSerializer.Serialize<NGeniusPayment>(order._embedded.payment[0]).ToString());
                        sql += String.Format(",{0}", (order._embedded.payment[0].state=="STARTED"?0:1));
                        sql += String.Format(",{0}", (order._embedded.payment[0].state == "CANCELLED" ? 1 : 0));
                        sql += String.Format(",'{0}'", order.createDateTime);
                        sql += String.Format(",'{0}'", order.createDateTime);
                        sql += String.Format(",'{0}'", order._embedded.payment[0].state);
                        sql += ")";
                        err.message = "Created " + order.reference;
                    } else
                    {
                        sql = "Update OrderLog SET ";
                        sql += String.Format("outletId='{0}'", order.outletId);
                        sql += String.Format(",orderRef='{0}'", order.reference);
                        sql += String.Format(",paymentRef='{0}'", order._embedded.payment[0].reference);
                        sql += String.Format(",responseData='{0}'", JsonSerializer.Serialize<NGeniusOrder>(order).ToString());
                        sql += String.Format(",requestURL='{0}'", (order._links.payment!=null?order._links.payment.href:""));
                        sql += String.Format(",currencyCode='{0}'", order.amount.currencyCode);
                        sql += String.Format(",amountValue={0}", order.amount.value);
                        sql += String.Format(",transType='{0}'", order.action);
                        sql += String.Format(",paymentType='{0}'", order.type);
                        sql += String.Format(",paymentData='{0}'", JsonSerializer.Serialize<NGeniusPayment>(order._embedded.payment[0]).ToString());
                        sql += String.Format(",isPaymented={0}", (order._embedded.payment[0].state == "STARTED" ? 0 : 1));
                        sql += String.Format(",isCancel={0}", (order._embedded.payment[0].state == "CANCELLED" ? 1 : 0));
                        sql += String.Format(",createDateTime='{0}'", order.createDateTime);
                        sql += String.Format(",updateDateTime='{0}'", order.createDateTime);
                        sql += String.Format(",status='{0}'", order._embedded.payment[0].state);
                        sql += String.Format(" WHERE orderRef='{0}'", order.reference);
                        err.message = "Update "+ order.reference;
                    }
                    using(SqlCommand cm=new SqlCommand(sql, cn))
                    {
                        cm.CommandType = CommandType.Text;
                        cm.ExecuteNonQuery();
                        err.code = 100;                        
                    }
                    cn.Close();
                }
            } catch (Exception e)
            {
                err.code = 400;
                err.message = e.Message;
                err.errors =new NGeniusError() { errorCode="400",message = sql };
            }
        }
        public void SaveOrder(NGeniusPayment order, ref NGeniusErrorResponse err)
        {
            string sql = "";
            try
            {
                using (SqlConnection cn = new SqlConnection(Config.MainConnection))
                {
                    cn.Open();
                    bool isExisting = false;
                    using (SqlDataReader rd = new SqlCommand(string.Format("SELECT * FROM OrderLog where orderRef='{0}'", order.orderReference), cn).ExecuteReader())
                    {
                        isExisting = rd.HasRows;
                        rd.Close();
                    }
                    string fields = @"outletId,orderRef,paymentRef,responseData,requestURL,currencyCode,amountValue,transType,paymentType,paymentData,isPaymented,isCancel,createDateTime,updateDateTime,status";
                    if (!isExisting)
                    {
                        sql = @"INSERT INTO OrderLog (" + fields + ") VALUES(";
                        sql += String.Format("'{0}'", order.outletId);
                        sql += String.Format(",'{0}'", order.orderReference);
                        sql += String.Format(",'{0}'", order.reference);
                        sql += String.Format(",'{0}'", JsonSerializer.Serialize<NGeniusPayment>(order).ToString());
                        sql += String.Format(",'{0}'", order._links.self.href);
                        sql += String.Format(",'{0}'", order.amount.currencyCode);
                        sql += String.Format(",{0}", order.amount.value);
                        sql += String.Format(",'{0}'", "PAYMENT");
                        sql += String.Format(",'{0}'", "CARD");
                        sql += String.Format(",'{0}'", JsonSerializer.Serialize(order._links));
                        sql += String.Format(",{0}", (order.state == "STARTED" ? 0 : 1));
                        sql += String.Format(",{0}", (order.state == "CANCELLED" ? 1 : 0));
                        sql += String.Format(",'{0}'", order.updateDateTime);
                        sql += String.Format(",'{0}'", order.updateDateTime);
                        sql += String.Format(",'{0}'", order.state);
                        sql += ")";
                        err.message = "Created " + order.orderReference;
                    }
                    else
                    {
                        sql = "Update OrderLog SET ";
                        sql += String.Format("outletId='{0}'", order.outletId);
                        sql += String.Format(",orderRef='{0}'", order.orderReference);
                        sql += String.Format(",paymentRef='{0}'", order.reference);
                        sql += String.Format(",responseData='{0}'", JsonSerializer.Serialize<NGeniusPayment>(order).ToString());
                        sql += String.Format(",requestURL='{0}'", order._links.self.href);
                        sql += String.Format(",currencyCode='{0}'", order.amount.currencyCode);
                        sql += String.Format(",amountValue={0}", order.amount.value);
                        sql += String.Format(",transType='{0}'", "PAYMENT");
                        sql += String.Format(",paymentType='{0}'", "CARD");
                        sql += String.Format(",paymentData='{0}'", JsonSerializer.Serialize(order._links));
                        sql += String.Format(",isPaymented={0}", (order.state == "STARTED" ? 0 : 1));
                        sql += String.Format(",isCancel={0}", (order.state == "CANCELLED" ? 1 : 0));
                        sql += String.Format(",createDateTime='{0}'", order.updateDateTime);
                        sql += String.Format(",updateDateTime='{0}'", order.updateDateTime);
                        sql += String.Format(",status='{0}'", order.state);
                        sql += String.Format(" WHERE orderRef='{0}'", order.orderReference);
                        err.message = "Update " + order.orderReference;
                    }
                    using (SqlCommand cm = new SqlCommand(sql, cn))
                    {
                        cm.CommandType = CommandType.Text;
                        cm.ExecuteNonQuery();
                        err.code = 100;
                    }
                    cn.Close();
                }
            }
            catch (Exception e)
            {
                err.code = 400;
                err.message = e.Message;
                err.errors = new NGeniusError() { errorCode = "400", message = sql };
            }
        }

    }

}

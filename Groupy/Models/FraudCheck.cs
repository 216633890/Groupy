using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace Groupy.Models
{
    public class FraudCheck
    {
        //GroupyEntities storeDB = new GroupyEntities();

        public string GetCountry()
        {
            string c_code;
            String AccessKey = "demo";
            String UserIP = "2607:f1c0:100f:f000::2f4";
            //String UserIP = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(UserIP))
            {
                UserIP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            string url = "https://api.ip2location.com/v2/?ip=" + UserIP.ToString() + "&key=" + AccessKey.ToString() + "&package=WS2&addon=continent";
            WebClient client = new WebClient();
            string jsonstring = client.DownloadString(url);
            dynamic dynObj = JsonConvert.DeserializeObject(jsonstring);
            System.Web.HttpContext.Current.Session["UserCountryCode"] = dynObj.country_code;
            c_code = dynObj.country_code.ToString();

            return c_code;
        }

        public bool IsValidTrans(Order order)
        {
            bool res = false;
            string previousOrder;
            int i = 0;

            using (var sdb = new GroupyEntities())
            {
                previousOrder = (from c in sdb.Orders
                                        where c.Username == order.Username && c.IsSuccess == 1
                                        orderby c.OrderDate descending
                                        select c.Username).First();
            }

            if (!string.IsNullOrEmpty(previousOrder)) {
                if (!IsCountryMatchPreviousOrder(order.Username, order.OrderCountry)) {
                    i++;
                }
                if (!IsKnownPurchaseTimeRage(order.Username, order.OrderDate)) {
                    i++;
                }
            }

            if (i<=1) {
                res = true;
            }

            return res;
        }

        public bool IsCountryMatchPreviousOrder(string username, string ordercountry)
        {
            bool match = false;
            string previousOrderCountry;

            using (var sdb = new GroupyEntities())
            {
                previousOrderCountry = (from c in sdb.Orders
                                               where c.Username == username && c.IsSuccess == 1
                                               orderby c.OrderDate descending
                                               select c.OrderCountry).First();
            }

            if (previousOrderCountry == ordercountry) {
                match = true;
            }

            return match;
        }

        public bool IsKnownPurchaseTimeRage(string username, DateTime orderdate)
        {
            bool res = false;
            DateTime previousOrderTime;

            using (var sdb = new GroupyEntities())
            {
                previousOrderTime = (from c in sdb.Orders
                                     where c.Username == username && c.IsSuccess == 1
                                     orderby c.OrderDate descending
                                     select c.OrderDate).First();
            }

            if (int.Parse(orderdate.ToString("HH")) >= int.Parse(previousOrderTime.ToString("HH"))-3
                && int.Parse(orderdate.ToString("HH")) <= int.Parse(previousOrderTime.ToString("HH")) + 3)
            {
                res = true;
            }

            return res;
        }

        //public static double standardDeviation(this IEnumerable<double> sequence)
        //{ 
        //    double average = sequence.Average();
        //    double sum = sequence.Sum(d => Math.Pow(d - average, 2));
        //    return Math.Sqrt((sum) / (sequence.Count() - 1)); 
        //}
    }
}

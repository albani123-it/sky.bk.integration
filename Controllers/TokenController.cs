using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using sky.bk.integration.Libs;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace sky.bk.integration.Controllers
{
    public class TokenController : Controller
    {
        private BaseController bc = new BaseController();
        private lDbConn dbconn = new lDbConn();

        public JObject GetToken(string module)
        {
            JObject jOutput = new JObject();
            var WebAPIURL = dbconn.domainGetApi(module);
            string requestStr = WebAPIURL + "token";

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("username", dbconn.domainGetTokenCredential("UserName"));
            client.DefaultRequestHeaders.Add("password", dbconn.domainGetTokenCredential("Password"));
            var contentData = new StringContent("", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");

            HttpResponseMessage response = client.PostAsync(requestStr, contentData).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            jOutput = JObject.Parse(result);
            return jOutput;
        }
    }
}

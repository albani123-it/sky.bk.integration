using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using sky.bk.integration.Libs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace sky.bk.integration.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("skybkintegration/[controller]")]
    public class FileReaderController : Controller
    {
        private lDbConn dbconn = new lDbConn();
        private BaseController bc = new BaseController();
        private MessageController mc = new MessageController();
        private TokenController tc = new TokenController();

        [HttpPost("OtherDataReport")]
        public JObject OtherDataReport([FromBody]JObject json)
        {
            var data = new JObject();
            var module = "urlAPI_idcpefindo";
            JObject credential = tc.GetToken(module);
            try
            {
                var filename_otherdata = json.GetValue("filename").ToString();
                var folder = Path.GetFullPath("file/response/other/");
                var pathOTXml = folder + filename_otherdata + ".xml";
                var strXmlDataOT = "";
                using (StreamReader srOT = new StreamReader(pathOTXml))
                {
                    strXmlDataOT = srOT.ReadToEnd();
                }

                XmlDocument docxOT = new XmlDocument();
                docxOT.Load(new StringReader(strXmlDataOT));
                var joXmlOT = JsonConvert.SerializeXmlNode(docxOT);
                var pathOTJson = folder + filename_otherdata + ".json";
                var mkFolderOT = Path.GetFullPath(pathOTJson);
                
                FileInfo fiOT = new FileInfo(mkFolderOT);

                //check 1st check file jika sudah ada di delete dan generate baru
                if (fiOT.Exists)
                {
                    fiOT.Delete();
                }

                // Create a new file     
                using (FileStream fsOT = fiOT.Create())
                {
                    Byte[] txtOT = new UTF8Encoding(true).GetBytes("New file.");
                    fsOT.Write(txtOT, 0, txtOT.Length);
                    Byte[] authorOT = new UTF8Encoding(true).GetBytes("idxteam");
                    fsOT.Write(authorOT, 0, authorOT.Length);
                }
                System.IO.File.WriteAllText(mkFolderOT, joXmlOT);

                var joDataXmlOT = new JObject();
                joDataXmlOT = JObject.Parse(joXmlOT.ToString());

                data = joDataXmlOT;

                module = "urlAPI_idcpefindo";
                var pefindoid = json.GetValue("pefindoid").ToString();
                var joReq = new JObject();
                joReq.Add("pefindoid", pefindoid);
                joReq.Add("type_data", mc.GetMessage("type_data_personal"));
                joReq.Add("raw_data", joDataXmlOT);

                string outApi = "";
                outApi = bc.execExtAPIPostWithToken(module, "FileReader/OthersData", joReq.ToString(), "bearer " + credential.GetValue("access_token").ToString());
                var rtndata = JObject.Parse(outApi);

                data = rtndata;

            }
            catch (Exception ex)
            {
                data = new JObject();
                data.Add("status", mc.GetMessage("api_output_not_ok"));
                data.Add("message", ex.Message);
            }

            return data;
        }

    }
}

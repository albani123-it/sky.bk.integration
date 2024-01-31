using sky.bk.integration.Libs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace sky.bk.integration.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("skybkintegration/[controller]")]
    public class EnquiryController : Controller
    {
        private lDbConn dbconn = new lDbConn();
        private lConvert lc = new lConvert();
        private BaseController bc = new BaseController();
        private TokenController tc = new TokenController();
        private PinvokeWindowsNetworking abc = new PinvokeWindowsNetworking();
        private string prefix_req = "req_";
        private string prefix_res = "res_";
        private string prefix_smr = "smr_";
        private string prefix_cus = "cus_";
        private MessageController mc = new MessageController();


        #region proses untuk company

        [HttpPost("EnquiryBK")]
        public JObject ProcessToServerCompanyBK([FromBody]JObject json)
        {
            var filename = "";
            string allText = "";
            var resdate = "";
            var folder = "";
            var path = "";
            var reqname = "";
            string strXmlData = "";

            using (StreamReader sr = new StreamReader("file/headerconfig.json"))
            {
                allText = sr.ReadToEnd();
            }
            var joHeader = JObject.Parse(allText);

            #region process generate request & send request to webservice pefindo
            //  process generate request & send request to webservice pefindo ==> save response to xml file 
            //generate request
            var joReqData = GenerateReqData(json);
            filename = joReqData.GetValue("filename").ToString();


            // send request to web service pefindo 
            filename = joReqData.GetValue("filename").ToString();
            folder = Path.GetFullPath("file/request/");
            path = folder + prefix_req + prefix_smr + filename + joHeader.GetValue("ext").ToString();

            strXmlData = "";
            using (StreamReader sr = new StreamReader(path))
            {
                strXmlData = sr.ReadToEnd();
            }

            var mkFolder = Path.GetFullPath("file/response/");
            XmlDocument reqdocx = new XmlDocument();
            reqdocx.Load(new StringReader(strXmlData));
            var soapaction = "soap_action_smr_company";
            //var josmr_company = PostDataXml(reqdocx, soapaction);
            var josmr_company = new JObject();
            reqname = "73.093.155.7-503.000_20190902111408";
            filename = reqname;
            folder = Path.GetFullPath("file/response/");
            path = folder + prefix_res + prefix_smr + filename + joHeader.GetValue("ext").ToString();

            strXmlData = "";
            using (StreamReader sr = new StreamReader(path))
            {
                strXmlData = sr.ReadToEnd();
            }

            mkFolder = Path.GetFullPath("file/response/");

            var dataDummy1 = new JObject();
            dataDummy1.Add("status", mc.GetMessage("api_output_ok"));
            dataDummy1.Add("response", strXmlData);

            josmr_company = dataDummy1;

            var jocus_company = new JObject();
            var strRes_smr_company = "";
            if (josmr_company.GetValue("status").ToString() == mc.GetMessage("api_output_ok"))
            {
                strRes_smr_company = josmr_company.GetValue("response").ToString();

                string xmlresponse = "";
                XmlDocument resdocx_smr_company = new XmlDocument();
                resdocx_smr_company.Load(new StringReader(strRes_smr_company));

                var josmr_company_xml = JsonConvert.SerializeXmlNode(resdocx_smr_company);
                var joReqcustRpt = JObject.Parse(josmr_company_xml);
                joReqcustRpt.Add("subject_type", json.GetValue("type_data").ToString());
                joReqcustRpt.Add("npwp", json.GetValue("npwp").ToString());
                var joGenReqCustonRpt = GenerateReqCustomReport(joReqcustRpt);

                filename = joGenReqCustonRpt.GetValue("filename").ToString();
                folder = Path.GetFullPath("file/request/");
                path = folder + prefix_req + prefix_cus + filename + joHeader.GetValue("ext").ToString();

                strXmlData = "";
                using (StreamReader sr = new StreamReader(path))
                {
                    strXmlData = sr.ReadToEnd();
                }

                folder = Path.GetFullPath("file/request/");
                XmlDocument reqdocx_cusrpt = new XmlDocument();
                reqdocx_cusrpt.Load(new StringReader(strXmlData));

                //soapaction = "soap_action_cus_company";
                //jocus_company = PostDataXml(reqdocx_cusrpt, soapaction);

                if (jocus_company.GetValue("status").ToString()== mc.GetMessage("api_output_ok"))
                {
                    var strRes_cus_company = jocus_company.GetValue("response").ToString();
                    XmlDocument resdocx_cus_company = new XmlDocument();
                    resdocx_cus_company.Load(new StringReader(strRes_cus_company));
                    //save response custom report
                    folder = Path.GetFullPath("file/response/");
                    path = folder + prefix_res + prefix_cus + filename + joHeader.GetValue("ext").ToString();
                    resdocx_cus_company.Save(path);
                }
            }

            // end
            #endregion

            #region proses setelah dapat response dari biro kredit

            resdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            // proses konvert xml ke json
            folder = Path.GetFullPath("file/response/");

            //untuk testing filename disetup sebagai ='testingxmldata'
            //filename = "testingxmldata";            

            path = folder + prefix_res + prefix_cus + filename + joHeader.GetValue("ext").ToString();

            strXmlData = "";
            using (StreamReader sr = new StreamReader(path))
            {
                strXmlData = sr.ReadToEnd();
            }

            XmlDocument docx = new XmlDocument();
            docx.Load(new StringReader(strXmlData));
            var joXml = JsonConvert.SerializeXmlNode(docx);
            mkFolder = Path.GetFullPath("file/response/" + prefix_res + prefix_cus + filename + ".json");
            FileInfo fi = new FileInfo(mkFolder);

            //check 1st check file jika sudah ada di delete dan generate baru
            if (fi.Exists)
            {
                fi.Delete();
            }

            // Create a new file     
            using (FileStream fs = fi.Create())
            {
                Byte[] txt = new UTF8Encoding(true).GetBytes("New file.");
                fs.Write(txt, 0, txt.Length);
                Byte[] author = new UTF8Encoding(true).GetBytes("idxteam");
                fs.Write(author, 0, author.Length);
            }
            System.IO.File.WriteAllText(mkFolder, joXml);

            var joDataXml1 = new JObject();
            joDataXml1 = JObject.Parse(joXml.ToString());
            // end
            #endregion

            var data = new JObject();
            data.Add("status", mc.GetMessage("process_success"));
            //data.Add("reqdate", joReqData.GetValue("reqdate").ToString());
            data.Add("resdate", resdate);
            data.Add("filename", filename);
            data.Add("data", joDataXml1);
            data.Add("response", strRes_smr_company);

            return data;
        }

        public JObject GenerateReqData(JObject json)
        {
            var data = new JObject();
            var joRtnInfo = new JObject();
            var today = DateTime.Now.ToString("yyyyMMddHHmmss");
            var reqdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string allText = "";

            using (StreamReader sr = new StreamReader("file/headerconfig.json"))
            {
                allText = sr.ReadToEnd();
            }
            var joHeader = JObject.Parse(allText);
            var joData = json;
            var doc = new XmlDocument();

            #region generate req xml
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace cb5 = "http://creditinfo.com/CB5";
            XNamespace smar = "http://creditinfo.com/CB5/v5.53/SmartSearch";

            XElement root = new XElement(soapenv + "Envelope", new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "cb5", cb5.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "smar", smar.NamespaceName),
                new XElement(soapenv + "Header"),
                new XElement(soapenv + "Body",
                             new XElement(cb5 + "SmartSearchCompany",
                                          new XElement(cb5 + "query",
                                                       new XElement(smar + "InquiryReason", joHeader.GetValue("InquiryReason").ToString()),
                                                       new XElement(smar + "InquiryReasonText", joHeader.GetValue("InquiryReason").ToString()),
                                                       new XElement(smar + "Parameters",
                                                                    new XElement(smar + "CompanyName", json.GetValue("company_name").ToString()),
                                                                    new XElement(smar + "IdNumbers",
                                                                                 new XElement(smar + "IdNumberPairCompany",
                                                                                              new XElement(smar + "IdNumber", json.GetValue("npwp").ToString()),
                                                                                              new XElement(smar + "IdNumberType", "NPWP")
                                                                                              )
                                                                                 )

                                                                     )
                                                       )
                                           )
                             )
                );
            #endregion 

            var filename = "";
            var folder = Path.GetFullPath("file/request/");

            filename = joData.GetValue("npwp").ToString() + "_" + today;
            var path = folder + prefix_req + prefix_smr + filename + joHeader.GetValue("ext").ToString();

            root.Save(path);

            data.Add("filename", filename);
            data.Add("reqdate", reqdate);

            return data;
        }

        public JObject GenerateReqCustomReport(JObject json)
        {
            var jo1 = JObject.Parse(json.GetValue("s:Envelope").ToString());
            var jo2 = JObject.Parse(jo1.GetValue("s:Body").ToString());
            var jo3 = JObject.Parse(jo2.GetValue("SmartSearchCompanyResponse").ToString());
            var jo4 = JObject.Parse(jo3.GetValue("SmartSearchCompanyResult").ToString());

            var data = new JObject();
            var joRtnInfo = new JObject();
            var today = DateTime.Now.ToString("yyyyMMddHHmmss");
            var reqdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var reqdate_cusrpt = DateTime.Now.ToString("yyyy-MM-dd");
            string allText = "";

            using (StreamReader sr = new StreamReader("file/headerconfig.json"))
            {
                allText = sr.ReadToEnd();
            }
            var joHeader = JObject.Parse(allText);
            var joData = json;
            var doc = new XmlDocument();

            #region generate req xml
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace cb5 = "http://creditinfo.com/CB5";
            XNamespace cus = "http://creditinfo.com/CB5/v5.53/CustomReport";
            XNamespace arr = "http://schemas.microsoft.com/2003/10/Serialization/Arrays";

            XElement root = new XElement(soapenv + "Envelope", new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "cb5", cb5.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "cus", cus.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "arr", arr.NamespaceName),
                new XElement(soapenv + "Header"),
                new XElement(soapenv + "Body",
                             new XElement(cb5 + "GetCustomReport",
                                          new XElement(cb5 + "parameters",
                                                       new XElement(cus + "Consent", Convert.ToBoolean(joHeader.GetValue("Consent").ToString())),
                                                       new XElement(cus + "IDNumber", jo4.GetValue("a:PefindoId").ToString()),
                                                       new XElement(cus + "IDNumberType", joHeader.GetValue("IDNumberType").ToString()),
                                                       new XElement(cus + "InquiryReason", joHeader.GetValue("InquiryReason").ToString()),
                                                       new XElement(cus + "InquiryReasonText"), 
                                                       new XElement(cus + "ReportDate", reqdate_cusrpt),
                                                       new XElement(cus + "Sections",
                                                                    new XElement(arr + "string", joHeader.GetValue("Sections").ToString())
                                                                    ),
                                                       new XElement(cus + "SubjectType", joData.GetValue("subject_type").ToString())
                                                     )
                                        )
                              )
                           
                        );
            #endregion 

            var filename_custrpt = "";
            var folder = Path.GetFullPath("file/request/");

            filename_custrpt = joData.GetValue("npwp").ToString() + "_" + today;
            var path = folder + prefix_req + prefix_cus + filename_custrpt + joHeader.GetValue("ext").ToString();

            root.Save(path);

            data.Add("filename", filename_custrpt);
            data.Add("reqdate", reqdate);

            return data;
        }


        #endregion

        #region proses untuk personal
        [HttpPost("EnquiryPersonalBK")]
        public JObject ProcessToServerPersonalBK([FromBody]JObject json)
        {
            string filenamecatch = Path.GetFullPath("file/catch/catch.txt");
            if (System.IO.File.Exists(filenamecatch))
            {
                using (StreamWriter sw = System.IO.File.AppendText(filenamecatch))
                {
                    sw.WriteLine(DateTime.Now + " - idcbk_EnquiryPersonalBK start" + " - " + json.GetValue("ktp").ToString());
                    //sw.WriteLine(outApi);
                    sw.Close();
                }
            }

            var filename = "";
            var filenamecust = "";
            var pefindoid = "";
            var filename_otherdata = "";
            string allText = "";
            var resdate = "";
            var folder = "";
            var path = "";
            var address = "";
            var dob1 = "";
            var fullname = "";
            var ktp = "";
            var collectdataarray = "";
            var count_pefid = "";
            string strXmlData = "";
            var data = new JObject();
            var joReturn = new JObject();
            var numofdata = 0;
            numofdata = int.Parse(dbconn.GetCredential("numofdata").ToString(), CultureInfo.InvariantCulture.NumberFormat);

            try
            {
                Random r = new Random();
                var requestid = DateTime.Now.ToString("yyyyMMddHHMMssffffff").ToString() + r.Next().ToString();

                using (StreamReader sr = new StreamReader("file/headerconfig.json"))
                {
                    allText = sr.ReadToEnd();
                }
                var joHeader = JObject.Parse(allText);

                #region proses request smartsearch
                //  process generate request & send request to webservice pefindo ==> save response to xml file 
                //generate request
                var joReqData = GenerateReqPersonalData(json, requestid);
                filename = joReqData.GetValue("filename").ToString();

                // send request to web service pefindo 
                //var reqname = "_20190902111408";
                //reqname = json.GetValue("ktp").ToString() + reqname;
                //filename = joReqData.GetValue("filename").ToString();
                //filename = reqname;


                //folder = Path.GetFullPath("file/request/");
                folder = Path.GetFullPath("file/request/smrserch/");
                path = folder + prefix_req + prefix_smr + filename + joHeader.GetValue("ext").ToString();

                strXmlData = "";
                using (StreamReader sr = new StreamReader(path))
                {
                    strXmlData = sr.ReadToEnd();
                }

                //var mkFolder = Path.GetFullPath("file/response/");
                var mkFolder = Path.GetFullPath("file/response/smrserch/");
                XmlDocument reqdocx = new XmlDocument();
                reqdocx.Load(new StringReader(strXmlData));
                var soapaction = "soap_action_smr_personal";
                var josmr_personal = new JObject();

                //if (System.IO.File.Exists(filenamecatch))
                //{
                //    using (StreamWriter sw = System.IO.File.AppendText(filenamecatch))
                //    {
                //        sw.WriteLine(DateTime.Now + " - idcbk_sendReqSmart start" + " - " + json.GetValue("ktp").ToString());
                //        //sw.WriteLine(outApi);
                //        sw.Close();
                //    }
                //}

                ////// start remark untuk proses 1 dummy data dan unremark untuk direct langsung ke pefindo webservice
                //josmr_personal = PostDataXml(reqdocx, soapaction);
                // josmr_personal = PostDataXmlnotsoap(strXmlData);
                //////end

                //if (System.IO.File.Exists(filenamecatch))
                //{
                //    using (StreamWriter sw = System.IO.File.AppendText(filenamecatch))
                //    {
                //        sw.WriteLine(DateTime.Now + " - idcbk_sendReqSmart end" + " - " + json.GetValue("ktp").ToString());
                //        //sw.WriteLine(outApi);
                //        sw.Close();
                //    }
                //}

                //start proses 1 ini hanya untuk sebelum ada koneksi ke pefindo webservice dan unremark jika direct langsung ke pefindo webservice
                var reqname = joHeader.GetValue("dummy_smr").ToString();
                filename = reqname;
                folder = Path.GetFullPath("file/response/smrserch/");
                path = folder + prefix_res + prefix_smr + filename + joHeader.GetValue("ext").ToString();

                strXmlData = "";
                using (StreamReader sr = new StreamReader(path))
                {
                    strXmlData = sr.ReadToEnd();
                }

                mkFolder = Path.GetFullPath("file/response/smrserch/");
                var dataDummy1 = new JObject();
                dataDummy1.Add("status", mc.GetMessage("api_output_ok"));
                dataDummy1.Add("response", strXmlData);
                josmr_personal = dataDummy1;
                //end


                //var ip = dbconn.GetCredential("urlNetworkCredentialy");
                //var pathserverother = dbconn.GetCredential("pathserverother");
                //var username = dbconn.GetCredential("username");
                //var pass = dbconn.GetCredential("pass");
                //string aaa = abc.connectToRemote(ip, username, pass);

                //string local = @"" + path + "";
                //string remote = @"" + pathserverother + "custrpt\\" + "" + "";

                //if (System.IO.File.Exists(remote))
                //{
                //    System.IO.File.Delete(remote);

                //    System.IO.File.Copy(local, remote);
                //}
                //else
                //{
                //    System.IO.File.Copy(local, remote);
                //}
                //abc.disconnectRemote(ip);

                #endregion

                #region proses response smartsearch
                var jocus_personal = new JObject();
                var strRes_smr_company = "";
                var joReqcustRpt = new JObject();
                if (josmr_personal.GetValue("status").ToString() == mc.GetMessage("api_output_ok"))
                {
                    strRes_smr_company = josmr_personal.GetValue("response").ToString();
                    string xmlresponse = "";
                    XmlDocument resdocx_smr_company = new XmlDocument();
                    resdocx_smr_company.Load(new StringReader(strRes_smr_company));

                    folder = Path.GetFullPath("file/response/smrserch/");
                    path = folder + prefix_res + prefix_smr + filename + joHeader.GetValue("ext").ToString();
                    resdocx_smr_company.Save(path);

                    var josmr_company_xml = JsonConvert.SerializeXmlNode(resdocx_smr_company);
                    joReqcustRpt = JObject.Parse(josmr_company_xml);
                    joReqcustRpt.Add("subject_type", json.GetValue("type_data").ToString());
                    joReqcustRpt.Add("ktp", json.GetValue("ktp").ToString());
                    joReqcustRpt.Add("ttable", json.GetValue("ttable").ToString());
                    joReqcustRpt.Add("type_data", json.GetValue("type_data").ToString());
                    joReqcustRpt.Add("reqdate", joReqData.GetValue("reqdate").ToString());
                    joReqcustRpt.Add("usrid", json.GetValue("usrid").ToString());
                    joReqcustRpt.Add("type", json.GetValue("type").ToString());

                    var jo1 = JObject.Parse(joReqcustRpt.GetValue("s:Envelope").ToString());
                    var jo2 = JObject.Parse(jo1.GetValue("s:Body").ToString());
                    var jo3 = JObject.Parse(jo2.GetValue("SmartSearchIndividualResponse").ToString());
                    var jo4 = JObject.Parse(jo3.GetValue("SmartSearchIndividualResult").ToString());

                    var jo5 = JObject.Parse(jo4.GetValue("a:IndividualRecords").ToString());
                    var indivrecored = jo5.GetValue("a:SearchIndividualRecord").ToString();
                    var checkTypeObj = CheckDataObject(indivrecored);
                    if (checkTypeObj.GetValue("object_type").ToString() == "JObject")
                    {
                        var jo6 = JObject.Parse(jo5.GetValue("a:SearchIndividualRecord").ToString());
                        pefindoid = jo6.GetValue("a:PefindoId").ToString();
                        if (pefindoid == "{\r\n  \"@i:nil\": \"true\"\r\n}")
                        {
                            pefindoid = "";
                        }
                        else
                        {
                            address = jo6.GetValue("a:Address").ToString();
                            if (address == "{\r\n  \"@i:nil\": \"true\"\r\n}")
                            {
                                address = "";
                            }

                            dob1 = jo6.GetValue("a:DateOfBirth").ToString();
                            if (dob1 == "{\r\n  \"@i:nil\": \"true\"\r\n}")
                            {
                                dob1 = "";
                            }
                            else
                            {
                                dob1 = Convert.ToDateTime(jo6.GetValue("a:DateOfBirth")).ToString("yyyy-MM-dd");
                            }

                            fullname = jo6.GetValue("a:FullName").ToString();
                            if (fullname == "{\r\n  \"@i:nil\": \"true\"\r\n}")
                            {
                                fullname = "";
                            }

                            ktp = jo6.GetValue("a:KTP").ToString();
                            if (ktp == "{\r\n  \"@i:nil\": \"true\"\r\n}")
                            {
                                ktp = "";
                            }

                            joReturn = this.chkjarowinkler(ktp, fullname, dob1, address, json.GetValue("ttable").ToString());
                            var threshold = float.Parse(dbconn.GetCredential("thresholdSingle").ToString(), CultureInfo.InvariantCulture.NumberFormat);

                            var jR = float.Parse(joReturn.GetValue("retrundata").ToString());
                            if (jR >= threshold)
                            {
                                pefindoid = jo6.GetValue("a:PefindoId").ToString();
                                count_pefid = "1";
                            }
                            else
                            {
                                pefindoid = "";
                                this.updatecounterpefid(json.GetValue("ttable").ToString());
                            }
                        }
                    }
                    else if (checkTypeObj.GetValue("object_type").ToString() == "JArray")
                    {
                        var jaData = JArray.Parse(jo5.GetValue("a:SearchIndividualRecord").ToString());
                        if (jaData[0]["a:PefindoId"].ToString() != "")
                        {
                            for (int a = 0; a < jaData.Count; a++)
                            {
                                address = jaData[a]["a:Address"].ToString();
                                if (address == "{\r\n  \"@i:nil\": \"true\"\r\n}")
                                {
                                    address = "";
                                }
                                else
                                {
                                    address = jaData[a]["a:Address"].ToString();
                                }

                                dob1 = jaData[a]["a:DateOfBirth"].ToString();
                                if (dob1 == "{\r\n  \"@i:nil\": \"true\"\r\n}")
                                {
                                    dob1 = "";
                                }
                                else
                                {
                                    dob1 = Convert.ToDateTime(jaData[a]["a:DateOfBirth"]).ToString("yyyy-MM-dd");
                                }

                                fullname = jaData[a]["a:FullName"].ToString();
                                if (fullname == "{\r\n  \"@i:nil\": \"true\"\r\n}")
                                {
                                    fullname = "";
                                }
                                else
                                {
                                    fullname = jaData[a]["a:FullName"].ToString();
                                }

                                ktp = jaData[a]["a:KTP"].ToString();
                                if (ktp == "{\r\n  \"@i:nil\": \"true\"\r\n}")
                                {
                                    ktp = "";
                                }
                                else
                                {
                                    ktp = jaData[a]["a:KTP"].ToString();
                                }

                                int xx = a + 1;
                                joReturn = this.chkjarowinkler(ktp, fullname, dob1, address, json.GetValue("ttable").ToString());
                                collectdataarray += joReturn.GetValue("retrundata").ToString() + "-" + xx + "|";
                            }

                            collectdataarray = collectdataarray.TrimEnd('|');

                            // start
                            int count = 0;
                            bool checkthreshold = false;
                            //float[] arr = collectdataarray.Split('|').Select(float.Parse).ToArray();
                            string[] arr = collectdataarray.Split('|');
                            Array.Sort(arr); Array.Reverse(arr);
                            float max;
                            var responethreshold = float.Parse(dbconn.GetCredential("threshold").ToString(), CultureInfo.InvariantCulture.NumberFormat);

                            //check jika tidak ada <= threshold 
                            for (int x = 0; x < arr.Count(); x++)
                            {
                                //var result1 = arr[x].Replace("-", "");
                                string[] zzzz = arr[x].Split("-");
                                var xxxx = zzzz[0];
                                var result = float.Parse(xxxx);
                                if (result >= responethreshold)
                                {
                                    checkthreshold = true;
                                    if (numofdata > 0 && x < numofdata)
                                    {
                                        var result5 = arr[x].Substring(arr[x].LastIndexOf('-') + 1);
                                        int zz = Int32.Parse(result5);
                                        pefindoid += jaData[zz - 1]["a:PefindoId"].ToString() + "|";

                                    }
                                    count++;
                                    //break;
                                }
                            }

                            //end
                            if (checkthreshold == true)
                            {
                                pefindoid = pefindoid.TrimEnd('|');
                                count_pefid = count.ToString();
                            }
                            else
                            {
                                this.updatecounterpefid(json.GetValue("ttable").ToString());
                                var jo11 = JObject.Parse(joReqcustRpt.GetValue("s:Envelope").ToString());
                                var jo12 = JObject.Parse(jo11.GetValue("a:PefindoId").ToString());
                            }

                        }
                        else
                        {
                            //pefindoid = null;
                            pefindoid = "";
                        }
                    } //jarray close
                    joReqcustRpt.Add("pefindoid", pefindoid);
                    joReqcustRpt.Add("ctr", count_pefid);
                    joReqcustRpt.Add("identity",json.GetValue("identity"));
                    joReqcustRpt.Add("cust_code", json.GetValue("cust_code"));
                }
                #endregion

                if (string.IsNullOrEmpty(pefindoid) == false)
                {
                    data.Add("status", mc.GetMessage("process_success"));
                    data.Add("numofdata",numofdata);
                    data.Add("data", joReqcustRpt);
                }
                else
                {
                    data.Add("status", "invalid");
                    data.Add("Message", mc.GetMessage("process_not_success"));
                }

                if (System.IO.File.Exists(filenamecatch))
                {
                    using (StreamWriter sw = System.IO.File.AppendText(filenamecatch))
                    {
                        sw.WriteLine(DateTime.Now + " - idcbk_EnquiryPersonalBK end" + " - " + json.GetValue("ktp").ToString());
                        //sw.WriteLine(outApi);
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                data.Add("status", "invalid");
                data.Add("Message", ex.Message);
            }

            return data;
        }
        #endregion

        //[HttpPost("EnquiryPersonalBKDet")]
        //public async Task ProcessToServerPersonalBKDetailTest([FromBody]JObject joReqcustRpt)
        //{
        //    //await this.ProcessToServerPersonalBKDetail(joReqcustRpt);
        //    var ctr = Int32.Parse(joReqcustRpt.GetValue("ctr").ToString());
        //    var numofdata = Int32.Parse(joReqcustRpt.GetValue("numofdata").ToString());

        //    int xctr = ctr;
        //    if (ctr > numofdata) xctr = numofdata;

        //    var pefindoid = joReqcustRpt.GetValue("pefindoid").ToString();
        //    String[] arr = pefindoid.Split("|");

        //    var tasks = new List<Task>();
        //    for (int i = 0; i < xctr; i++)
        //    {
        //        joReqcustRpt["pefindoid"] = arr[i].ToString();
        //        var task = this.ProcessToServerPersonalBKDetail(joReqcustRpt);
        //        tasks.Add(task);
        //    }

        //    await Task.WhenAll(tasks);

        //    //summary
        //}

        [HttpPost("EnquiryPersonalBKDetSingle")]
        //Prosess Single data ProcessToServerPersonalBKDetail([FromBody]JObject joReqcustRpt)
        public void ProcessToServerPersonalBKDetailSingle([FromBody]JObject joReqcustRpt)
        {

            var filename = "";
            var filenamecust = "";
            var pefindoid = joReqcustRpt.GetValue("pefindoid").ToString();
            var filename_otherdata = "";
            string allText = "";
            var resdate = "";
            var folder = "";
            var path = "";
            var address = "";
            var dob1 = "";
            var fullname = "";
            var ktp = "";
            var collectdataarray = "";
            var mkFolder = "";
            var soapaction = "soap_action_smr_personal";
            var count_pefid = joReqcustRpt.GetValue("ctr").ToString();
            var numofdata = joReqcustRpt.GetValue("numofdata").ToString();
            string strXmlData = "";
            var data = new JObject();
            var joReturn = new JObject();

            try
            {
                //await Task.Run(() =>
                //{
                //await Task.Delay(1000); //in milliseconds
                #region pindahanz
                using (StreamReader sr = new StreamReader("file/headerconfig.json"))
                {
                    allText = sr.ReadToEnd();
                }
                var joHeader = JObject.Parse(allText);

                var reqname = joHeader.GetValue("dummy_smr").ToString();
                var jocus_personal = new JObject();

                Random r = new Random();
                var requestid = DateTime.Now.ToString("yyyyMMddHHMMssffffff").ToString() + r.Next().ToString();

                #region proses request customrpt ke pefindo
                var joGenReqCustonRpt = GenerateReqPersonalCustomReport(joReqcustRpt, pefindoid, requestid);
                filenamecust = joGenReqCustonRpt.GetValue("filename").ToString();

                //folder = Path.GetFullPath("file/request/");
                folder = Path.GetFullPath("file/request/custrpt/");
                path = folder + prefix_req + prefix_cus + filenamecust + joHeader.GetValue("ext").ToString();

                strXmlData = "";
                using (StreamReader sr = new StreamReader(path))
                {
                    strXmlData = sr.ReadToEnd();
                }

                //folder = Path.GetFullPath("file/request/");
                folder = Path.GetFullPath("file/request/custrpt/");
                XmlDocument reqdocx_cusrpt = new XmlDocument();
                reqdocx_cusrpt.Load(new StringReader(strXmlData));


                var dtresult = this.insertlogreqandres(joReqcustRpt.GetValue("cust_code").ToString(), joReqcustRpt.GetValue("ktp").ToString(), "cust_rpt", pefindoid, "0", "0");
                //// start remark untuk proses 2 dummy data dan unremark jika direct langnsung ke pefindo webservice
                //jocus_personal = PostDataXml(reqdocx_cusrpt, soapaction);
                //jocus_personal = PostDataXmlnotsoap(strXmlData);
                ////end
                //this.insertlogreqandres(joReqcustRpt.GetValue("cust_code").ToString(), joReqcustRpt.GetValue("ktp").ToString(), "cust_rpt", pefindoid, "1", dtresult[0]["id"].ToString());

                //  start proses 2 ini jika pakai dumm dan remark jika pakai dummy data
                if (pefindoid == "3947031")
                {
                    reqname = joHeader.GetValue("dummy_cus_3947031").ToString();
                }
                else if (pefindoid == "56007193")
                {
                    reqname = joHeader.GetValue("dummy_cus_56007193").ToString();
                }
                else if (pefindoid == "8307233")
                {
                    reqname = joHeader.GetValue("dummy_cus_8307233").ToString();
                }
                else if (pefindoid == "15268858")
                {
                    reqname = joHeader.GetValue("dummy_cus_15268858").ToString();
                }
                else if (pefindoid == "4728031")
                {
                    reqname = joHeader.GetValue("dummy_cus_4728031").ToString();
                }
                else
                {
                    reqname = joHeader.GetValue("dummy_cus").ToString();
                }

                //reqname = joHeader.GetValue("dummy_cus").ToString();
                //reqname = json.GetValue("ktp").ToString() + reqname;
                filenamecust = reqname;
                //folder = Path.GetFullPath("file/response/");
                folder = Path.GetFullPath("file/response/custrpt/");
                path = folder + prefix_res + prefix_cus + filenamecust + joHeader.GetValue("ext").ToString();

                strXmlData = "";
                using (StreamReader sr = new StreamReader(path))
                {
                    strXmlData = sr.ReadToEnd();
                }

                //mkFolder = Path.GetFullPath("file/response/");
                mkFolder = Path.GetFullPath("file/response/custrpt/");
                var dataDummy2 = new JObject();
                dataDummy2.Add("status", mc.GetMessage("api_output_ok"));
                dataDummy2.Add("response", strXmlData);
                jocus_personal = dataDummy2;
                //end

                if (jocus_personal.GetValue("status").ToString() == mc.GetMessage("api_output_ok"))
                {
                    var strRes_cus_company = jocus_personal.GetValue("response").ToString();
                    //strRes_cus_company = strRes_cus_company.Replace("UTF-8", "UTF-8 without BOM");
                    XmlDocument resdocx_cus_company = new XmlDocument();
                    resdocx_cus_company.Load(new StringReader(strRes_cus_company));
                    //save response custom report
                    //folder = Path.GetFullPath("file/response/");
                    folder = Path.GetFullPath("file/response/custrpt/");
                    path = folder + prefix_res + prefix_cus + filenamecust + joHeader.GetValue("ext").ToString();
                    resdocx_cus_company.Save(path);

                    // save to credential server 
                    var fileNamecust1 = prefix_res + prefix_cus + filenamecust + joHeader.GetValue("ext").ToString();
                    var ip = dbconn.GetCredential("urlNetworkCredentialy");
                    var pathserverother = dbconn.GetCredential("pathserverother_bk");
                    var username = dbconn.GetCredential("username");
                    var pass = dbconn.GetCredential("pass");
                    string a = abc.connectToRemote(ip, username, pass);

                    string local = @"" + path + "";
                    string remote = @"" + pathserverother + "custrpt\\" + fileNamecust1 + "";

                    if (System.IO.File.Exists(remote))
                    {
                        System.IO.File.Delete(remote);

                        System.IO.File.Copy(local, remote);
                    }
                    else
                    {
                        System.IO.File.Copy(local, remote);
                    }
                    abc.disconnectRemote(ip);

                    var pathaip = dbconn.GetCredential("pathserverother_bk").ToString() + "custrpt\\" + fileNamecust1;
                    this.Insertfilemapbk("Custom", joReqcustRpt.GetValue("ttable").ToString(), joReqcustRpt.GetValue("ktp").ToString(), pefindoid, pathaip, count_pefid, numofdata);
                }
                #endregion

                // other data 
                var joother_data = new JObject();
                var strRes_smr_otherdata = "";
                if (jocus_personal.GetValue("status").ToString() == mc.GetMessage("api_output_ok"))
                {
                    strRes_smr_otherdata = jocus_personal.GetValue("response").ToString();

                    XmlDocument resdocx_smr_other = new XmlDocument();
                    resdocx_smr_other.Load(new StringReader(strRes_smr_otherdata));

                    var josmr_other_xml = JsonConvert.SerializeXmlNode(resdocx_smr_other);
                    var joReqotherRpt = JObject.Parse(josmr_other_xml);
                    joReqotherRpt.Add("subject_type", joReqcustRpt.GetValue("type_data").ToString());
                    joReqotherRpt.Add("ktp", joReqcustRpt.GetValue("ktp").ToString());
                    var joGenReqOtherdata = GenerateReqPersonalOtherData(joReqotherRpt, pefindoid, requestid);

                    filename = joGenReqOtherdata.GetValue("filename").ToString();
                    folder = Path.GetFullPath("file/request/other/");
                    path = folder + prefix_req + "other_" + filename + joHeader.GetValue("ext").ToString();

                    strXmlData = "";
                    using (StreamReader sr = new StreamReader(path))
                    {
                        strXmlData = sr.ReadToEnd();
                    }

                    folder = Path.GetFullPath("file/request/other/");
                    XmlDocument reqdocx_otherdata = new XmlDocument();
                    reqdocx_otherdata.Load(new StringReader(strXmlData));


                    var dtresult1 =  this.insertlogreqandres(joReqcustRpt.GetValue("cust_code").ToString(), joReqcustRpt.GetValue("ktp").ToString(), "other_data", pefindoid, "0", "0");
                    //// start remark untuk proses 2 dummy data dan unremark jika direct langnsung ke pefindo webservice
                    //joother_data = PostDataXml(reqdocx_otherdata, soapaction);
                    //joother_data = PostDataXmlnotsoap(strXmlData);
                    //this.insertlogreqandres(joReqcustRpt.GetValue("cust_code").ToString(), joReqcustRpt.GetValue("ktp").ToString(), "other_data", pefindoid, "1", dtresult1[0]["id"].ToString());



                    //  start proses 2 ini jika pakai dumm dan remark jika pakai dummy data
                    //if (pefindoid == "3947031")
                    //{
                    //    reqname = joHeader.GetValue("dummy_otd_3947031").ToString();
                    //}
                    //else
                    //{
                    //    reqname = joHeader.GetValue("dummy_otd_56007193").ToString();
                    //}

                    reqname = joHeader.GetValue("dummy_otd").ToString();
                    //reqname = json.GetValue("ktp").ToString() + reqname;
                    filename = reqname;
                    folder = Path.GetFullPath("file/response/other/");
                    //folder = Path.GetFullPath("file/response/");
                    path = folder + prefix_res + "other_" + filename + joHeader.GetValue("ext").ToString();

                    strXmlData = "";
                    using (StreamReader sr = new StreamReader(path))
                    {
                        strXmlData = sr.ReadToEnd();
                    }

                    //mkFolder = Path.GetFullPath("file/response/");
                    mkFolder = Path.GetFullPath("file/response/other/");

                    var dataDummy3 = new JObject();
                    dataDummy3.Add("status", mc.GetMessage("api_output_ok"));
                    dataDummy3.Add("response", strXmlData);

                    joother_data = dataDummy3;
                    // end 


                    if (joother_data.GetValue("status").ToString() == mc.GetMessage("api_output_ok"))
                    {

                        var strRes_other_data = joother_data.GetValue("response").ToString();

                        XmlDocument resdocx_other_data = new XmlDocument();
                        resdocx_other_data.Load(new StringReader(strRes_other_data));
                        //save response other data
                        //folder = Path.GetFullPath("file/response/");
                        folder = Path.GetFullPath("file/response/other/");

                        var today = DateTime.Now.ToString("yyyyMMddHHmmss");
                        //filename_otherdata = joReqcustRpt.GetValue("ktp").ToString() + "_" + today;
                        filename_otherdata = joReqcustRpt.GetValue("ktp").ToString() + "_" + pefindoid.ToString() + today.ToString();
                        //path = folder + prefix_res + "other_" + filename + joHeader.GetValue("ext").ToString();
                        path = folder + prefix_res + "other_" + filename_otherdata + joHeader.GetValue("ext").ToString();
                        resdocx_other_data.Save(path);

                        // save to credential server 
                        var fileNameother1 = prefix_res + "other_" + filename_otherdata + joHeader.GetValue("ext").ToString();
                        var ip = dbconn.GetCredential("urlNetworkCredentialy");
                        var pathserverother = dbconn.GetCredential("pathserverother_bk");
                        var username = dbconn.GetCredential("username");
                        var pass = dbconn.GetCredential("pass");
                        string a = abc.connectToRemote(ip, username, pass);

                        string local = @"" + path + "";
                        string remote = @"" + pathserverother + "otherdata\\" + fileNameother1 + "";

                        if (System.IO.File.Exists(remote))
                        {
                            System.IO.File.Delete(remote);

                            System.IO.File.Copy(local, remote);
                        }
                        else
                        {
                            System.IO.File.Copy(local, remote);
                        }


                        abc.disconnectRemote(ip);
                        var pathaipother = dbconn.GetCredential("pathserverother_bk").ToString() + "otherdata\\" + fileNameother1;

                        //this.Insertfilemapbk("otherdata", joReqcustRpt.GetValue("ttable").ToString(), joReqcustRpt.GetValue("ktp").ToString(), pefindoid, pathaipother, count_pefid, numofdata);

                        // end
                    }
                }

                // end


                // pdf rpt
                #region prosess async pdf rpt dan other data parsing
                var joReq = new JObject();
                joReq.Add("pefindoid", pefindoid);
                joReq.Add("joother_data", joother_data.GetValue("status").ToString());
                joReq.Add("response", joother_data.GetValue("response").ToString());
                joReq.Add("requestid", requestid);
                joReq.Add("type_data", joReqcustRpt.GetValue("type_data").ToString());
                joReq.Add("ktp", joReqcustRpt.GetValue("ktp").ToString());
                joReq.Add("ttable", joReqcustRpt.GetValue("ttable").ToString());
                joReq.Add("ext", joHeader.GetValue("ext").ToString());
                joReq.Add("numofdata", numofdata); //add utk ke pdf
                joReq.Add("dummy_pdf", joHeader.GetValue("dummy_pdf").ToString());
                //Processpdfrpt(joReq, count_pefid);

                #endregion

                #region proses convert response xml ke json

                resdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // proses convert xml ke json
                //folder = Path.GetFullPath("file/response/");
                folder = Path.GetFullPath("file/response/custrpt/");

                //start untuk testing filename disetup sebagai ='1123433_20190805161923' dan jika direct langsung di remark
                //reqname = joHeader.GetValue("dummy_cus").ToString();
                ////reqname = json.GetValue("ktp").ToString() + reqname;
                //filename = reqname;
                //end

                path = folder + prefix_res + prefix_cus + filenamecust + joHeader.GetValue("ext").ToString();

                strXmlData = "";
                using (StreamReader sr = new StreamReader(path))
                {
                    strXmlData = sr.ReadToEnd();
                }

                XmlDocument docx = new XmlDocument();
                docx.Load(new StringReader(strXmlData));
                var joXml = JsonConvert.SerializeXmlNode(docx);
                //mkFolder = Path.GetFullPath("file/response/" + prefix_res + prefix_cus + filename + ".json");
                mkFolder = Path.GetFullPath("file/response/custrpt/" + prefix_res + prefix_cus + filenamecust + ".json");
                FileInfo fi = new FileInfo(mkFolder);

                //check 1st check file jika sudah ada di delete dan generate baru
                if (fi.Exists)
                {
                    fi.Delete();
                }

                // Create a new file     
                //using (FileStream fs = fi.Create())
                //{
                //    Byte[] txt = new UTF8Encoding(true).GetBytes("New file.");
                //    fs.Write(txt, 0, txt.Length);
                //    Byte[] author = new UTF8Encoding(true).GetBytes("idxteam");
                //    fs.Write(author, 0, author.Length);
                //}
                //System.IO.File.WriteAllText(mkFolder, joXml);

                //var joDataXml1 = new JObject();
                //joDataXml1 = JObject.Parse(joXml.ToString());



                System.IO.File.WriteAllText(mkFolder, joXml);

                var joDataXml1 = new JObject();
                joDataXml1 = JObject.Parse(joXml.ToString());
                // end
                #endregion


                #region prosess insert otherdata 

                var module = "urlAPI_skybkintegration";
                string outApi = "";
                var joReq_reader = new JObject();
                var namefileotherdata = prefix_res + "other_" + filename_otherdata;
                joReq_reader.Add("pefindoid", pefindoid);
                joReq_reader.Add("filename", namefileotherdata);
                JObject credential = tc.GetToken(module);
                //outApi = bc.execExtAPIPostWithToken(module, "FileReader/OtherDataReport", joReq_reader.ToString(), "bearer " + credential.GetValue("access_token").ToString());
                //var riquestdata = JObject.Parse(outApi);
                #endregion

                data.Add("status", mc.GetMessage("process_success"));
                data.Add("reqdate", joReqcustRpt.GetValue("reqdate").ToString());
                data.Add("resdate", resdate);
                data.Add("filename", filenamecust);
                data.Add("data", joDataXml1);
                data.Add("type_data", joReqcustRpt.GetValue("type_data").ToString());
                data.Add("identity", joReqcustRpt.GetValue("identity").ToString());
                data.Add("cust_code", joReqcustRpt.GetValue("cust_code").ToString());
                data.Add("usrid", joReqcustRpt.GetValue("usrid").ToString());
                data.Add("ktp", joReqcustRpt.GetValue("ktp").ToString());
                data.Add("type", joReqcustRpt.GetValue("type").ToString());
                data.Add("pefindoid", pefindoid);
                #endregion

                #region dari idc.pefindo
                module = "urlAPI_skypefindo";
                credential = tc.GetToken(module);
                data = JObject.Parse(bc.execExtAPIPostWithToken(module, "Enquiry/ProcessToDatabasePersonalBK", data.ToString(), "bearer " + credential.GetValue("access_token").ToString()));
                #endregion



                //data.Add("status", mc.GetMessage("process_success"));
                //data.Add("numofdata",data.GetValue("numofdata"));

                //});

            }
            catch (Exception ex)
            {
                //data.Add("status", "invalid");
                data["status"] = "invalid";
                data.Add("Message", ex.Message);
            }

            //return data;
        }

        [HttpPost("EnquiryPersonalBKDetMultiple")]
        //private async Task ProcessToServerPersonalBKDetail([FromBody]JObject joReqcustRpt)
        public async void ProcessToServerPersonalBKDetailMultiple([FromBody]JObject joReqcustRpt)
        //public JObject ProcessToServerPersonalBKDetail([FromBody]JObject joReqcustRpt)
        {

            var filename = "";
            var filenamecust = "";
            var pefindoid = joReqcustRpt.GetValue("pefindoid").ToString();
            var filename_otherdata = "";
            string allText = "";
            var resdate = "";
            var folder = "";
            var path = "";
            var address = "";
            var dob1 = "";
            var fullname = "";
            var ktp = "";
            var collectdataarray = "";
            var mkFolder = "";
            var count_pefid = joReqcustRpt.GetValue("ctr").ToString();
            var numofdata = joReqcustRpt.GetValue("numofdata").ToString();
            string strXmlData = "";
            var data = new JObject();
            var joReturn = new JObject();

            try
            {
                //await Task.Run(() =>
                //{
                await Task.Delay(1000); //in milliseconds
                #region pindahanz
                using (StreamReader sr = new StreamReader("file/headerconfig.json"))
                    {
                        allText = sr.ReadToEnd();
                    }
                    var joHeader = JObject.Parse(allText);

                    var reqname = joHeader.GetValue("dummy_smr").ToString();
                    var jocus_personal = new JObject();

                    Random r = new Random();
                    var requestid = DateTime.Now.ToString("yyyyMMddHHMMssffffff").ToString() + r.Next().ToString();

                    #region proses request customrpt ke pefindo
                    var joGenReqCustonRpt = GenerateReqPersonalCustomReport(joReqcustRpt, pefindoid, requestid);
                    filenamecust = joGenReqCustonRpt.GetValue("filename").ToString();

                    //folder = Path.GetFullPath("file/request/");
                    folder = Path.GetFullPath("file/request/custrpt/");
                    path = folder + prefix_req + prefix_cus + filenamecust + joHeader.GetValue("ext").ToString();

                    strXmlData = "";
                    using (StreamReader sr = new StreamReader(path))
                    {
                        strXmlData = sr.ReadToEnd();
                    }

                    //folder = Path.GetFullPath("file/request/");
                    folder = Path.GetFullPath("file/request/custrpt/");
                    XmlDocument reqdocx_cusrpt = new XmlDocument();
                    reqdocx_cusrpt.Load(new StringReader(strXmlData));


                var dtresult =  this.insertlogreqandres(joReqcustRpt.GetValue("cust_code").ToString(), joReqcustRpt.GetValue("ktp").ToString(), "cust_rpt", pefindoid, "0","0");
                //// start remark untuk proses 2 dummy data dan unremark jika direct langnsung ke pefindo webservice
                //jocus_personal = PostDataXmlnotsoap(strXmlData);
                ////end
                //this.insertlogreqandres(joReqcustRpt.GetValue("cust_code").ToString(), joReqcustRpt.GetValue("ktp").ToString(), "cust_rpt", pefindoid, "1", dtresult[0]["id"].ToString());

                //  start proses 2 ini jika pakai dumm dan remark jika pakai dummy data
                if (pefindoid == "2152216")
                {
                    reqname = joHeader.GetValue("dummy_cus_2152216").ToString();
                }
                else if (pefindoid == "15354680")
                {
                    reqname = joHeader.GetValue("dummy_cus_15354680").ToString();
                }
                else if (pefindoid == "8307233")
                {
                    reqname = joHeader.GetValue("dummy_cus_8307233").ToString();
                }
                else if (pefindoid == "15268858")
                {
                    reqname = joHeader.GetValue("dummy_cus_15268858").ToString();
                }
                else if (pefindoid == "4728031")
                {
                    reqname = joHeader.GetValue("dummy_cus_4728031").ToString();
                }
                else
                {
                    reqname = joHeader.GetValue("dummy_cus").ToString();
                }

                //reqname = joHeader.GetValue("dummy_cus").ToString();
                //reqname = json.GetValue("ktp").ToString() + reqname;
                filenamecust = reqname;
                //folder = Path.GetFullPath("file/response/");
                folder = Path.GetFullPath("file/response/custrpt/");
                path = folder + prefix_res + prefix_cus + filenamecust + joHeader.GetValue("ext").ToString();

                strXmlData = "";
                using (StreamReader sr = new StreamReader(path))
                {
                    strXmlData = sr.ReadToEnd();
                }

                //mkFolder = Path.GetFullPath("file/response/");
                mkFolder = Path.GetFullPath("file/response/custrpt/");
                var dataDummy2 = new JObject();
                dataDummy2.Add("status", mc.GetMessage("api_output_ok"));
                dataDummy2.Add("response", strXmlData);
                jocus_personal = dataDummy2;
                //end

                if (jocus_personal.GetValue("status").ToString() == mc.GetMessage("api_output_ok"))
                    {
                        var strRes_cus_company = jocus_personal.GetValue("response").ToString();
                        //strRes_cus_company = strRes_cus_company.Replace("UTF-8", "UTF-8 without BOM");
                        XmlDocument resdocx_cus_company = new XmlDocument();
                        resdocx_cus_company.Load(new StringReader(strRes_cus_company));
                        //save response custom report
                        //folder = Path.GetFullPath("file/response/");
                        folder = Path.GetFullPath("file/response/custrpt/");
                        path = folder + prefix_res + prefix_cus + filenamecust + joHeader.GetValue("ext").ToString();
                        resdocx_cus_company.Save(path);

                        // save to credential server 
                        var fileNamecust1 = prefix_res + prefix_cus + filenamecust + joHeader.GetValue("ext").ToString();
                        var ip = dbconn.GetCredential("urlNetworkCredentialy");
                        var pathserverother = dbconn.GetCredential("pathserverother_bk");
                        var username = dbconn.GetCredential("username");
                        var pass = dbconn.GetCredential("pass");
                        string a = abc.connectToRemote(ip, username, pass);

                        string local = @"" + path + "";
                        string remote = @"" + pathserverother + "custrpt\\" + fileNamecust1 + "";

                        if (System.IO.File.Exists(remote))
                        {
                            System.IO.File.Delete(remote);

                            System.IO.File.Copy(local, remote);
                        }
                        else
                        {
                            System.IO.File.Copy(local, remote);
                        }
                        abc.disconnectRemote(ip);

                        var pathaip = dbconn.GetCredential("pathserverother_bk").ToString() + "custrpt\\" + fileNamecust1;
                        this.Insertfilemapbk("Custom", joReqcustRpt.GetValue("ttable").ToString(), joReqcustRpt.GetValue("ktp").ToString(), pefindoid, pathaip, count_pefid, numofdata);
                    }
                    #endregion

                    // other data 
                    var joother_data = new JObject();
                    var strRes_smr_otherdata = "";
                    if (jocus_personal.GetValue("status").ToString() == mc.GetMessage("api_output_ok"))
                    {
                        strRes_smr_otherdata = jocus_personal.GetValue("response").ToString();

                        XmlDocument resdocx_smr_other = new XmlDocument();
                        resdocx_smr_other.Load(new StringReader(strRes_smr_otherdata));

                        var josmr_other_xml = JsonConvert.SerializeXmlNode(resdocx_smr_other);
                        var joReqotherRpt = JObject.Parse(josmr_other_xml);
                        joReqotherRpt.Add("subject_type", joReqcustRpt.GetValue("type_data").ToString());
                        joReqotherRpt.Add("ktp", joReqcustRpt.GetValue("ktp").ToString());
                        var joGenReqOtherdata = GenerateReqPersonalOtherData(joReqotherRpt, pefindoid, requestid);

                        filename = joGenReqOtherdata.GetValue("filename").ToString();
                        folder = Path.GetFullPath("file/request/other/");
                        path = folder + prefix_req + "other_" + filename + joHeader.GetValue("ext").ToString();

                        strXmlData = "";
                        using (StreamReader sr = new StreamReader(path))
                        {
                            strXmlData = sr.ReadToEnd();
                        }

                        folder = Path.GetFullPath("file/request/other/");
                        XmlDocument reqdocx_otherdata = new XmlDocument();
                        reqdocx_otherdata.Load(new StringReader(strXmlData));

                    //// start remark untuk proses 2 dummy data dan unremark jika direct langnsung ke pefindo webservice
                    var dtresult2 = this.insertlogreqandres(joReqcustRpt.GetValue("cust_code").ToString(), joReqcustRpt.GetValue("ktp").ToString(), "other_data", pefindoid, "0", "0");
                    //joother_data = PostDataXmlnotsoap(strXmlData);
                    //this.insertlogreqandres(joReqcustRpt.GetValue("cust_code").ToString(), joReqcustRpt.GetValue("ktp").ToString(), "other_data", pefindoid, "1", dtresult2[0]["id"].ToString());



                    //  start proses 2 ini jika pakai dumm dan remark jika pakai dummy data

                    //if (pefindoid == "3947031")
                    //{
                    //    reqname = joHeader.GetValue("dummy_otd_3947031").ToString();
                    //}
                    //else
                    //{
                    //    reqname = joHeader.GetValue("dummy_otd_56007193").ToString();
                    //}

                    reqname = joHeader.GetValue("dummy_otd").ToString();
                    //reqname = json.GetValue("ktp").ToString() + reqname;
                    filename = reqname;
                    folder = Path.GetFullPath("file/response/other/");
                    //folder = Path.GetFullPath("file/response/");
                    path = folder + prefix_res + "other_" + filename + joHeader.GetValue("ext").ToString();

                    strXmlData = "";
                    using (StreamReader sr = new StreamReader(path))
                    {
                        strXmlData = sr.ReadToEnd();
                    }

                    //mkFolder = Path.GetFullPath("file/response/");
                    mkFolder = Path.GetFullPath("file/response/other/");

                    var dataDummy3 = new JObject();
                    dataDummy3.Add("status", mc.GetMessage("api_output_ok"));
                    dataDummy3.Add("response", strXmlData);

                    joother_data = dataDummy3;
                    // end 


                    if (joother_data.GetValue("status").ToString() == mc.GetMessage("api_output_ok"))
                        {

                            var strRes_other_data = joother_data.GetValue("response").ToString();

                            XmlDocument resdocx_other_data = new XmlDocument();
                            resdocx_other_data.Load(new StringReader(strRes_other_data));
                            //save response other data
                            //folder = Path.GetFullPath("file/response/");
                            folder = Path.GetFullPath("file/response/other/");

                            var today = DateTime.Now.ToString("yyyyMMddHHmmss");
                            //filename_otherdata = joReqcustRpt.GetValue("ktp").ToString() + "_" + today;
                            filename_otherdata = joReqcustRpt.GetValue("ktp").ToString() + "_" + pefindoid.ToString() + today.ToString();
                            //path = folder + prefix_res + "other_" + filename + joHeader.GetValue("ext").ToString();
                            path = folder + prefix_res + "other_" + filename_otherdata + joHeader.GetValue("ext").ToString();
                                resdocx_other_data.Save(path);

                            // save to credential server 
                            var fileNameother1 = prefix_res + "other_" + filename_otherdata + joHeader.GetValue("ext").ToString();
                            var ip = dbconn.GetCredential("urlNetworkCredentialy");
                            var pathserverother = dbconn.GetCredential("pathserverother_bk");
                            var username = dbconn.GetCredential("username");
                            var pass = dbconn.GetCredential("pass");
                            string a = abc.connectToRemote(ip, username, pass);

                            string local = @"" + path + "";
                            string remote = @"" + pathserverother + "otherdata\\" + fileNameother1 + "";

                            if (System.IO.File.Exists(remote))
                            {
                                System.IO.File.Delete(remote);

                                System.IO.File.Copy(local, remote);
                            }
                            else
                            {
                                System.IO.File.Copy(local, remote);
                            }


                            abc.disconnectRemote(ip);
                            var pathaipother = dbconn.GetCredential("pathserverother_bk").ToString() + "otherdata\\" + fileNameother1;

                            this.Insertfilemapbk("otherdata", joReqcustRpt.GetValue("ttable").ToString(), joReqcustRpt.GetValue("ktp").ToString(), pefindoid, pathaipother, count_pefid, numofdata);

                            // end
                        }
                    }

                    // end


                    // pdf rpt
                    #region prosess async pdf rpt dan other data parsing
                    var joReq = new JObject();
                    joReq.Add("pefindoid", pefindoid);
                    joReq.Add("joother_data", joother_data.GetValue("status").ToString());
                    joReq.Add("response", joother_data.GetValue("response").ToString());
                    joReq.Add("requestid", requestid);
                    joReq.Add("type_data", joReqcustRpt.GetValue("type_data").ToString());
                    joReq.Add("ktp", joReqcustRpt.GetValue("ktp").ToString());
                    joReq.Add("ttable", joReqcustRpt.GetValue("ttable").ToString());
                    joReq.Add("ext", joHeader.GetValue("ext").ToString());
                    joReq.Add("numofdata", numofdata); //add utk ke pdf
                    joReq.Add("dummy_pdf", joHeader.GetValue("dummy_pdf").ToString());
                    Processpdfrpt(joReq, count_pefid);

                    #endregion

                    #region proses convert response xml ke json

                    resdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    // proses convert xml ke json
                    //folder = Path.GetFullPath("file/response/");
                    folder = Path.GetFullPath("file/response/custrpt/");

                    //start untuk testing filename disetup sebagai ='1123433_20190805161923' dan jika direct langsung di remark
                    //reqname = joHeader.GetValue("dummy_cus").ToString();
                    ////reqname = json.GetValue("ktp").ToString() + reqname;
                    //filename = reqname;
                    //end

                    path = folder + prefix_res + prefix_cus + filenamecust + joHeader.GetValue("ext").ToString();

                    strXmlData = "";
                    using (StreamReader sr = new StreamReader(path))
                    {
                        strXmlData = sr.ReadToEnd();
                    }

                    XmlDocument docx = new XmlDocument();
                    docx.Load(new StringReader(strXmlData));
                    var joXml = JsonConvert.SerializeXmlNode(docx);
                    //mkFolder = Path.GetFullPath("file/response/" + prefix_res + prefix_cus + filename + ".json");
                    mkFolder = Path.GetFullPath("file/response/custrpt/" + prefix_res + prefix_cus + filenamecust + ".json");
                    FileInfo fi = new FileInfo(mkFolder);

                    //check 1st check file jika sudah ada di delete dan generate baru
                    if (fi.Exists)
                    {
                        fi.Delete();
                    }

                    // Create a new file     
                    //using (FileStream fs = fi.Create())
                    //{
                    //    Byte[] txt = new UTF8Encoding(true).GetBytes("New file.");
                    //    fs.Write(txt, 0, txt.Length);
                    //    Byte[] author = new UTF8Encoding(true).GetBytes("idxteam");
                    //    fs.Write(author, 0, author.Length);
                    //}
                    //System.IO.File.WriteAllText(mkFolder, joXml);

                    //var joDataXml1 = new JObject();
                    //joDataXml1 = JObject.Parse(joXml.ToString());



                    System.IO.File.WriteAllText(mkFolder, joXml);

                    var joDataXml1 = new JObject();
                    joDataXml1 = JObject.Parse(joXml.ToString());
                    // end
                    #endregion


                    #region prosess insert otherdata 

                    var module = "urlAPI_idcbkintegration";
                    string outApi = "";
                    var joReq_reader = new JObject();
                    var namefileotherdata = prefix_res + "other_" + filename_otherdata;
                    joReq_reader.Add("pefindoid", pefindoid);
                    joReq_reader.Add("filename", namefileotherdata);
                    JObject credential = tc.GetToken(module);
                    outApi = bc.execExtAPIPostWithToken(module, "FileReader/OtherDataReport", joReq_reader.ToString(), "bearer " + credential.GetValue("access_token").ToString());
                    var riquestdata = JObject.Parse(outApi);
                    #endregion

                    data.Add("status", mc.GetMessage("process_success"));
                    data.Add("reqdate", joReqcustRpt.GetValue("reqdate").ToString());
                    data.Add("resdate", resdate);
                    data.Add("filename", filenamecust);
                    data.Add("data", joDataXml1);
                    data.Add("type_data", joReqcustRpt.GetValue("type_data").ToString());
                    data.Add("identity", joReqcustRpt.GetValue("identity").ToString());
                    data.Add("cust_code", joReqcustRpt.GetValue("cust_code").ToString());
                    data.Add("usrid", joReqcustRpt.GetValue("usrid").ToString());
                    data.Add("ktp", joReqcustRpt.GetValue("ktp").ToString());
                    data.Add("type", joReqcustRpt.GetValue("type").ToString());
                    data.Add("pefindoid", pefindoid);
                    #endregion

                    #region dari idc.pefindo
                    module = "urlAPI_idcpefindo";
                    credential = tc.GetToken(module);
                    data = JObject.Parse(bc.execExtAPIPostWithToken(module, "Enquiry/ProcessToDatabasePersonalBK", data.ToString(), "bearer " + credential.GetValue("access_token").ToString()));
                    #endregion

                   

                    //data.Add("status", mc.GetMessage("process_success"));
                    //data.Add("numofdata",data.GetValue("numofdata"));

                //});

            }
            catch (Exception ex)
            {
                //data.Add("status", "invalid");
                data["status"] = "invalid";
                data.Add("Message", ex.Message);
            }

            //return data;
        }

        public async void Processpdfrpt(JObject json, string count_pefid)
        {


            try
            {
                //await Task.Delay(1000);
            }
            finally
            {
                //#region proses pdf rpt
                var numofdata = json.GetValue("numofdata").ToString();
                string strXmlData = "";
                var pefindoid = json.GetValue("pefindoid").ToString();
                var requestid = json.GetValue("requestid").ToString();
                var jopdf_rpt = new JObject();
                var strRes_smr_pdfrpt = "";
                var targetDir = "";
                var pathnew = "";
                var soapaction = "soap_action_smr_company";

                var status = json.GetValue("joother_data").ToString();
                if (status == mc.GetMessage("api_output_ok"))
                {
                    strRes_smr_pdfrpt = json.GetValue("response").ToString();


                    XmlDocument resdocx_smr_pdfrpf = new XmlDocument();
                    resdocx_smr_pdfrpf.Load(new StringReader(strRes_smr_pdfrpt));

                    var josmr_pdfrpt_xml = JsonConvert.SerializeXmlNode(resdocx_smr_pdfrpf);
                    var joReqPdfRpt = JObject.Parse(josmr_pdfrpt_xml);
                    joReqPdfRpt.Add("subject_type", json.GetValue("type_data").ToString());
                    joReqPdfRpt.Add("ktp", json.GetValue("ktp").ToString());
                    var joGenReqPdfRpt = GenerateReqPersonalPdfRpt(joReqPdfRpt, pefindoid, requestid);

                    var filename = joGenReqPdfRpt.GetValue("filename").ToString();
                    var folder = Path.GetFullPath("file/request/pdfrpt/");
                    var path = folder + prefix_req + "pdfrpt_" + filename + json.GetValue("ext").ToString();

                    strXmlData = "";
                    using (StreamReader sr = new StreamReader(path))
                    {
                        strXmlData = sr.ReadToEnd();
                    }

                    folder = Path.GetFullPath("file/request/pdfrpt/");
                    XmlDocument reqdocx_pdfrpt = new XmlDocument();
                    reqdocx_pdfrpt.Load(new StringReader(strXmlData));

                   
                    //// start remark untuk proses 2 dummy data dan unremark jika direct langnsung ke pefindo webservice
                    //jopdf_rpt = PostDataXml(reqdocx_pdfrpt, soapaction);
                    //jopdf_rpt = PostDataXmlnotsoap(strXmlData);
                    ////end

                   

                    //  start proses 2 ini jika pakai dumm dan remark jika pakai dummy data

                    var reqname = json.GetValue("dummy_pdf").ToString();
                    //reqname = json.GetValue("ktp").ToString() + reqname;
                    filename = reqname;
                    folder = Path.GetFullPath("file/response/pdfrpt/");
                    path = folder + prefix_res + "pdfrpt_" + filename + json.GetValue("ext").ToString();

                    strXmlData = "";
                    using (StreamReader sr = new StreamReader(path))
                    {
                        strXmlData = sr.ReadToEnd();
                    }
                    var mkFolder = Path.GetFullPath("file/response/pdfrpt/");

                    var dataDummy4 = new JObject();
                    dataDummy4.Add("status", mc.GetMessage("api_output_ok"));
                    dataDummy4.Add("response", strXmlData);
                    jopdf_rpt = dataDummy4;
                    // end 

                    var filename_pdfrpt = "";
                    if (jopdf_rpt.GetValue("status").ToString() == mc.GetMessage("api_output_ok"))
                    {

                        try
                        {
                            var strRes_pdfrpt_data = jopdf_rpt.GetValue("response").ToString();

                            var rtndata = new JObject();
                            var rtndata1 = new JObject();
                            XmlDocument doc = new XmlDocument();
                            doc.Load(new StringReader(strRes_pdfrpt_data));
                            string jsonText = JsonConvert.SerializeXmlNode(doc);
                            rtndata = JObject.Parse(jsonText);
                            var today = DateTime.Now.ToString("yyyyMMddHHmmss");
                            filename_pdfrpt = json.GetValue("ktp").ToString() + "_" + json.GetValue("pefindoid").ToString() + today.ToString();
                            //folder = Path.GetFullPath("file/response/");
                            folder = Path.GetFullPath("file/response/pdfrpt/");
                            var pathpdf = folder + prefix_res + "pdfrpt_" + filename_pdfrpt + ".zip";

                            var value = JObject.Parse((JObject.Parse((JObject.Parse(rtndata["s:Envelope"].ToString()))["s:Body"].ToString()))["GetPdfReportResponse"].ToString());
                            var code = value.GetValue("GetPdfReportResult").ToString();



                            string base64BinaryStr = code;
                            byte[] sPDFDecoded = Convert.FromBase64String(base64BinaryStr);

                            System.IO.File.WriteAllBytes(pathpdf, sPDFDecoded);


                            FastZip fastZip = new FastZip();
                            string fileFilter = null;
                            targetDir = folder + prefix_res + "pdfrpt_" + filename_pdfrpt;
                            // Will always overwrite if target filenames already exist
                            fastZip.ExtractZip(pathpdf, targetDir, fileFilter);

                            // Create a FileInfo  
                            System.IO.FileInfo fi = new System.IO.FileInfo(targetDir + "//" + "report.pdf");
                            // Check if file is there  
                            if (fi.Exists)
                            {
                                var renamepdf = targetDir + "//" + prefix_res + "pdfrpt_" + filename_pdfrpt + ".pdf";
                                pathnew = renamepdf;
                                // Move file with a new name. Hence renamed.  
                                fi.MoveTo(renamepdf);

                            }

                            //if (System.IO.File.Exists(filenamecatch))
                            //{
                            //    using (StreamWriter sw = System.IO.File.AppendText(filenamecatch))
                            //    {
                            //        sw.WriteLine(DateTime.Now + " - idcbk_Processpdfrpt end" + " - " + json.GetValue("pefindoid").ToString());
                            //        //sw.WriteLine(outApi);
                            //        sw.Close();
                            //    }
                            //}

                        }
                        catch (Exception e)
                        {
                            string filename8 = Path.GetFullPath("file/catch/catch.txt");
                            if (System.IO.File.Exists(filename8))
                            {
                                using (StreamWriter sw = System.IO.File.AppendText(filename8))
                                {
                                    sw.WriteLine(DateTime.Now);
                                    sw.WriteLine(e.Message);
                                    sw.Close();
                                }
                            }


                            Console.WriteLine(e.ToString());

                        }



                        // save to credential server 

                        var fileNamepdf1 = prefix_res + "pdfrpt_" + filename_pdfrpt + ".pdf";
                        var ip = dbconn.GetCredential("urlNetworkCredentialy");
                        var pathserverother = dbconn.GetCredential("pathserverother_bk");
                        var username = dbconn.GetCredential("username");
                        var pass = dbconn.GetCredential("pass");
                        string a = abc.connectToRemote(ip, username, pass);

                        string local = @"" + pathnew + "";
                        string remote = @"" + pathserverother + "pdfrpt\\" + fileNamepdf1 + "";

                        if (System.IO.File.Exists(remote))
                        {

                            System.IO.File.Delete(remote);

                            System.IO.File.Copy(local, remote);



                        }
                        else
                        {

                            System.IO.File.Copy(local, remote);

                        }
                        abc.disconnectRemote(ip);
                        var pathaippdf = dbconn.GetCredential("pathserverother_bk").ToString() + "pdfrpt\\" + fileNamepdf1;
                        this.Insertfilemapbk("pdf", json.GetValue("ttable").ToString(), json.GetValue("ktp").ToString(), pefindoid, pathaippdf, count_pefid, numofdata);

                        // end

                    }
                }

            }

        }

        /*public async void Processpdfrpt(JObject json, string count_pefid)
        {
     

            try
            {
                //await Task.Delay(1000);
            }
            finally
            {
                //#region proses pdf rpt
                var numofdata = json.GetValue("numofdata").ToString();
                string strXmlData = "";
                var pefindoid = json.GetValue("pefindoid").ToString();
                var requestid = json.GetValue("requestid").ToString();
                var jopdf_rpt = new JObject();
                var strRes_smr_pdfrpt = "";
                var targetDir = "";
                var pathnew = "";
                var soapaction = "soap_action_smr_company";

                var status = json.GetValue("joother_data").ToString();
                if (status == mc.GetMessage("api_output_ok"))
                {
                    strRes_smr_pdfrpt = json.GetValue("response").ToString();


                    XmlDocument resdocx_smr_pdfrpf = new XmlDocument();
                    resdocx_smr_pdfrpf.Load(new StringReader(strRes_smr_pdfrpt));

                    var josmr_pdfrpt_xml = JsonConvert.SerializeXmlNode(resdocx_smr_pdfrpf);
                    var joReqPdfRpt = JObject.Parse(josmr_pdfrpt_xml);
                    joReqPdfRpt.Add("subject_type", json.GetValue("type_data").ToString());
                    joReqPdfRpt.Add("ktp", json.GetValue("ktp").ToString());
                    var joGenReqPdfRpt = GenerateReqPersonalPdfRpt(joReqPdfRpt, pefindoid, requestid);

                    var filename = joGenReqPdfRpt.GetValue("filename").ToString();
                    var folder = Path.GetFullPath("file/request/pdfrpt/");
                    var path = folder + prefix_req + "pdfrpt_" + filename + json.GetValue("ext").ToString();

                    strXmlData = "";
                    using (StreamReader sr = new StreamReader(path))
                    {
                        strXmlData = sr.ReadToEnd();
                    }

                    folder = Path.GetFullPath("file/request/pdfrpt/");
                    XmlDocument reqdocx_pdfrpt = new XmlDocument();
                    reqdocx_pdfrpt.Load(new StringReader(strXmlData));

                    //if (System.IO.File.Exists(filenamecatch))
                    //{
                    //    using (StreamWriter sw = System.IO.File.AppendText(filenamecatch))
                    //    {
                    //        sw.WriteLine(DateTime.Now + " - idcbk_sendPdfReq start" + " - " + json.GetValue("pefindoid").ToString());
                    //        //sw.WriteLine(outApi);
                    //        sw.Close();
                    //    }
                    //}

                    //// start remark untuk proses 2 dummy data dan unremark jika direct langnsung ke pefindo webservice
                    jopdf_rpt = PostDataXml(reqdocx_pdfrpt, soapaction);
                    //jopdf_rpt = PostDataXmlnotsoap(strXmlData);
                    ////end

                    //if (System.IO.File.Exists(filenamecatch))
                    //{
                    //    using (StreamWriter sw = System.IO.File.AppendText(filenamecatch))
                    //    {
                    //        sw.WriteLine(DateTime.Now + " - idcbk_sendPdfReq end" + " - " + json.GetValue("pefindoid").ToString());
                    //        //sw.WriteLine(outApi);
                    //        sw.Close();
                    //    }
                    //}

                    //  start proses 2 ini jika pakai dumm dan remark jika pakai dummy data

                    //var reqname = json.GetValue("dummy_pdf").ToString();
                    ////reqname = json.GetValue("ktp").ToString() + reqname;
                    //filename = reqname;
                    //folder = Path.GetFullPath("file/response/pdfrpt/");
                    //path = folder + prefix_res + "pdfrpt_" + filename + json.GetValue("ext").ToString();

                    //strXmlData = "";
                    //using (StreamReader sr = new StreamReader(path))
                    //{
                    //    strXmlData = sr.ReadToEnd();
                    //}
                    //var mkFolder = Path.GetFullPath("file/response/pdfrpt/");

                    //var dataDummy4 = new JObject();
                    //dataDummy4.Add("status", mc.GetMessage("api_output_ok"));
                    //dataDummy4.Add("response", strXmlData);
                    //jopdf_rpt = dataDummy4;
                    // end 

                    var filename_pdfrpt = "";
                    if (jopdf_rpt.GetValue("status").ToString() == mc.GetMessage("api_output_ok"))
                    {

                        try
                        {
                            var strRes_pdfrpt_data = jopdf_rpt.GetValue("response").ToString();
                          
                            var rtndata = new JObject();
                            var rtndata1 = new JObject();
                            XmlDocument doc = new XmlDocument();
                            doc.Load(new StringReader(strRes_pdfrpt_data));
                            string jsonText = JsonConvert.SerializeXmlNode(doc);
                            rtndata = JObject.Parse(jsonText);
                            var today = DateTime.Now.ToString("yyyyMMddHHmmss");
                            filename_pdfrpt = json.GetValue("ktp").ToString() + "_" + json.GetValue("pefindoid").ToString() + today.ToString();
                            //folder = Path.GetFullPath("file/response/");
                            folder = Path.GetFullPath("file/response/pdfrpt/");
                            var pathpdf = folder + prefix_res + "pdfrpt_" + filename_pdfrpt + ".zip";

                            var value = JObject.Parse((JObject.Parse((JObject.Parse(rtndata["ns0:ServiceEnvelope"].ToString()))["ns1:ServiceBody"].ToString()))["ns2:pfGetPdfReportRs"].ToString());
                            var code = value.GetValue("ns2:GetPdfReportResult").ToString();

                        

                            string base64BinaryStr = code;
                            byte[] sPDFDecoded = Convert.FromBase64String(base64BinaryStr);

                            System.IO.File.WriteAllBytes(pathpdf, sPDFDecoded);


                            FastZip fastZip = new FastZip();
                            string fileFilter = null;
                            targetDir = folder + prefix_res + "pdfrpt_" + filename_pdfrpt;
                            // Will always overwrite if target filenames already exist
                            fastZip.ExtractZip(pathpdf, targetDir, fileFilter);

                            // Create a FileInfo  
                            System.IO.FileInfo fi = new System.IO.FileInfo(targetDir + "//" + "report.pdf");
                            // Check if file is there  
                            if (fi.Exists)
                            {
                                var renamepdf = targetDir +"//" +prefix_res + "pdfrpt_" + filename_pdfrpt + ".pdf";
                                pathnew = renamepdf;
                                // Move file with a new name. Hence renamed.  
                                fi.MoveTo(renamepdf);
                               
                            }

                            //if (System.IO.File.Exists(filenamecatch))
                            //{
                            //    using (StreamWriter sw = System.IO.File.AppendText(filenamecatch))
                            //    {
                            //        sw.WriteLine(DateTime.Now + " - idcbk_Processpdfrpt end" + " - " + json.GetValue("pefindoid").ToString());
                            //        //sw.WriteLine(outApi);
                            //        sw.Close();
                            //    }
                            //}

                        }
                        catch (Exception e)
                        {
                            string filename8 = Path.GetFullPath("file/catch/catch.txt");
                            if (System.IO.File.Exists(filename8))
                            {
                                using (StreamWriter sw = System.IO.File.AppendText(filename8))
                                {
                                    sw.WriteLine(DateTime.Now);
                                    sw.WriteLine(e.Message);
                                    sw.Close();
                                }
                            }


                            Console.WriteLine(e.ToString());

                        }



                        // save to credential server 

                        var fileNamepdf1 = prefix_res + "pdfrpt_" + filename_pdfrpt + ".pdf";
                        var ip = dbconn.GetCredential("urlNetworkCredentialy");
                        var pathserverother = dbconn.GetCredential("pathserverother_bk");
                        var username = dbconn.GetCredential("username");
                        var pass = dbconn.GetCredential("pass");
                        string a = abc.connectToRemote(ip, username, pass);

                        string local = @"" + pathnew + "";
                        string remote = @"" + pathserverother + "pdfrpt\\" + fileNamepdf1 + "";

                        if (System.IO.File.Exists(remote))
                        {

                            System.IO.File.Delete(remote);

                            System.IO.File.Copy(local, remote);



                        }
                        else
                        {

                            System.IO.File.Copy(local, remote);

                        }
                        abc.disconnectRemote(ip);
                        var pathaippdf = dbconn.GetCredential("pathserverother_bk").ToString() + "pdfrpt\\" + fileNamepdf1;
                        this.Insertfilemapbk("pdf", json.GetValue("ttable").ToString(), json.GetValue("ktp").ToString(), pefindoid, pathaippdf, count_pefid, numofdata);

                        // end

                    }
                }

            }

        }*/


        public JObject GenerateReqPersonalData(JObject json, string requestid)
        {
            var data = new JObject();
            var joRtnInfo = new JObject();
            var today = DateTime.Now.ToString("yyyyMMddHHmmss");
            var reqdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string allText = "";

            using (StreamReader sr = new StreamReader("file/headerconfig.json"))
            {
                allText = sr.ReadToEnd();
            }
            var joHeader = JObject.Parse(allText);
            var joData = json;
            var doc = new XmlDocument();

            #region generate req xml
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace cb5 = "http://creditinfo.com/CB5";
            XNamespace smar = "http://creditinfo.com/CB5/v5.53/SmartSearch";

            XElement root = new XElement(soapenv + "Envelope", new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "cb5", cb5.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "smar", smar.NamespaceName),
                new XElement(soapenv + "Header"),
                new XElement(soapenv + "Body",
                             new XElement(cb5 + "SmartSearchIndividual",
                                          new XElement(cb5 + "query",
                                                       new XElement(smar + "InquiryReason", joHeader.GetValue("InquiryReason").ToString()),
                                                       new XElement(smar + "InquiryReasonText"),
                                                       new XElement(smar + "Parameters",
                                                                    new XElement(smar + "DateOfBirth", json.GetValue("dob").ToString()),
                                                                    new XElement(smar + "FullName", json.GetValue("name").ToString()),
                                                                    new XElement(smar + "IdNumbers",
                                                                                 new XElement(smar + "IdNumberPairIndividual",
                                                                                              new XElement(smar + "IdNumber", json.GetValue("ktp").ToString()),
                                                                                              new XElement(smar + "IdNumberType", "KTP")
                                                                                              )
                                                                                 )

                                                                     )
                                                       )
                                           )
                             )
                );
            #endregion 

            var filename = "";
            //var folder = Path.GetFullPath("file/request/");
            var folder = Path.GetFullPath("file/request/smrserch/");
            filename = joData.GetValue("ktp").ToString() + "_" + today;
            var path = folder + prefix_req + prefix_smr + filename + joHeader.GetValue("ext").ToString();

            root.Save(path);

            data.Add("filename", filename);
            data.Add("reqdate", reqdate);

            return data;
        }

        public JObject GenerateReqPersonalCustomReport(JObject json, string pefindoid, string requestid)
        {

            var jo1 = JObject.Parse(json.GetValue("s:Envelope").ToString());
            var jo2 = JObject.Parse(jo1.GetValue("s:Body").ToString());
            var jo3 = JObject.Parse(jo2.GetValue("SmartSearchIndividualResponse").ToString());
            var jo4 = JObject.Parse(jo3.GetValue("SmartSearchIndividualResult").ToString());

            var data = new JObject();
            var joRtnInfo = new JObject();
            var today = DateTime.Now.ToString("yyyyMMddHHmmss");
            var reqdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var reqdate_cusrpt = DateTime.Now.ToString("yyyy-MM-dd");
            string allText = "";

            using (StreamReader sr = new StreamReader("file/headerconfig.json"))
            {
                allText = sr.ReadToEnd();
            }
            var joHeader = JObject.Parse(allText);
            var joData = json;
            var doc = new XmlDocument();

            #region generate req xml
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace cb5 = "http://creditinfo.com/CB5";
            XNamespace cus = "http://creditinfo.com/CB5/v5.53/CustomReport";
            XNamespace arr = "http://schemas.microsoft.com/2003/10/Serialization/Arrays";

            XElement root = new XElement(soapenv + "Envelope", new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "cb5", cb5.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "cus", cus.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "arr", arr.NamespaceName),
                new XElement(soapenv + "Header"),
                new XElement(soapenv + "Body",
                             new XElement(cb5 + "GetCustomReport",
                                          new XElement(cb5 + "parameters",
                                                       new XElement(cus + "Consent", Convert.ToBoolean(joHeader.GetValue("Consent").ToString())),
                                                       new XElement(cus + "IDNumber", jo4.GetValue("a:PefindoId").ToString()),
                                                       new XElement(cus + "IDNumberType", joHeader.GetValue("IDNumberType").ToString()),
                                                       new XElement(cus + "InquiryReason", joHeader.GetValue("InquiryReason").ToString()),
                                                       new XElement(cus + "InquiryReasonText"),
                                                       new XElement(cus + "ReportDate", reqdate_cusrpt),
                                                       new XElement(cus + "Sections",
                                                                    new XElement(arr + "string", joHeader.GetValue("Sections").ToString())
                                                                    ),
                                                       new XElement(cus + "SubjectType", joData.GetValue("subject_type").ToString())
                                                     )
                                        )
                              )

                        );
            #endregion 

            var filename_custrpt = "";
            var folder = Path.GetFullPath("file/request/custrpt/");

            //filename_custrpt = joData.GetValue("ktp").ToString() + "_" + today;
            filename_custrpt = joData.GetValue("ktp").ToString() + "_" + pefindoid.ToString() + today.ToString();
            var path = folder + prefix_req + prefix_cus + filename_custrpt + joHeader.GetValue("ext").ToString();
            
            root.Save(path);

            data.Add("filename", filename_custrpt);
            data.Add("reqdate", reqdate);

            return data;
        }

        public JObject GenerateReqPersonalOtherData(JObject json, string pefindoid, string requestid)
        {
           
            var data = new JObject();
            var joRtnInfo = new JObject();
            var today = DateTime.Now.ToString("yyyyMMddHHmmss");
            var reqdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var reqdate_cusrpt = DateTime.Now.ToString("yyyy-MM-dd");
            var RqTimestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff");
            string allText = "";

            using (StreamReader sr = new StreamReader("file/headerconfig.json"))
            {
                allText = sr.ReadToEnd();
            }
            var joHeader = JObject.Parse(allText);
            var joData = json;
            var doc = new XmlDocument();

            #region generate req xml

            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace tem = "http://tempuri.org/";
            XNamespace wsse = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
            XNamespace wsu = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
            XNamespace mustUnderstand = "1";
            XNamespace token = "UsernameToken-3B48524E5385DAB52C15363175673104";
            XNamespace type = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText";


            XElement root = new XElement(soapenv + "Envelope", new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "tem", tem.NamespaceName),
              
        
                    new XElement(soapenv + "Header", 
                            new XElement(wsse + "Security", new XAttribute(soapenv + "mustUnderstand", mustUnderstand.NamespaceName), new XAttribute(XNamespace.Xmlns + "wsse", wsse.NamespaceName), new XAttribute(XNamespace.Xmlns + "wsu", wsu.NamespaceName),
                            new XElement(wsse + "UsernameToken", new XAttribute(wsu + "Id", token.NamespaceName),
                                     new XElement(wsse + "Username", "{username}"),
                                     new XElement(wsse + "Password", new XAttribute("Type", type.NamespaceName),
                                          "{username}"
                                        )
                                    )
                                  )
                                ),
                             new XElement(soapenv + "Body",
                             new XElement(tem + "OTGetReport",
                                new XElement(tem + "param",
                                            new XElement(tem + "PefindoId", pefindoid),
                                            new XElement(tem + "InquiryReason", joHeader.GetValue("InquiryReason_otd").ToString()),
                                            new XElement(tem + "InquiryReasonText"),
                                            new XElement(tem + "SubjectType", joHeader.GetValue("SubjectType_otd").ToString())
                                            )

                                     )
                             )
                );

            #endregion 

            var filename_otherdata = "";
            var folder = Path.GetFullPath("file/request/other/");

            filename_otherdata = joData.GetValue("ktp").ToString() + "_" + pefindoid.ToString() + today.ToString();
            var path = folder + prefix_req + "other_" + filename_otherdata + joHeader.GetValue("ext").ToString();

            root.Save(path);

            data.Add("filename", filename_otherdata);
            data.Add("reqdate", reqdate);

            return data;
        }


        public JObject GenerateReqPersonalPdfRpt(JObject json, string pefindoid, string requestid)
        {

            var data = new JObject();
            var joRtnInfo = new JObject();
            var today = DateTime.Now.ToString("yyyyMMddHHmmss");
            var reqdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var reqdate_cusrpt = DateTime.Now.ToString("yyyy-MM-dd");
            var RqTimestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff");
            string allText = "";

            using (StreamReader sr = new StreamReader("file/headerconfig.json"))
            {
                allText = sr.ReadToEnd();
            }
            var joHeader = JObject.Parse(allText);
            var joData = json;
            var doc = new XmlDocument();

            #region generate req xml


            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace cb5 = "http://creditinfo.com/CB5";
            XNamespace cus = "http://creditinfo.com/CB5/v5.31/CustomReport";
        


            XElement root = new XElement(soapenv + "Envelope", new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName), new XAttribute(XNamespace.Xmlns + "cb5", cb5.NamespaceName), new XAttribute(XNamespace.Xmlns + "cus", cus.NamespaceName),
                 new XElement(soapenv + "Header"),

                new XElement(soapenv + "Body", 
                             new XElement(cb5 + "GetPdfReport",
                               
                                    new XElement(cb5 + "parameters",
                                                new XElement(cus + "Consent", Convert.ToBoolean(joHeader.GetValue("Consent_pdf").ToString())),
                                                new XElement(cus + "IDNumber", pefindoid),
                                                new XElement(cus + "IDNumberType", joHeader.GetValue("IDNumberType_pdf").ToString()),
                                                new XElement(cus + "InquiryReason", joHeader.GetValue("InquiryReason_pdf").ToString()),
                                                new XElement(cus + "InquiryReasonText"),
                                                new XElement(cus + "LanguageCode", joHeader.GetValue("LanguageCode_pdf").ToString()),
                                                new XElement(cus + "ReportName", joHeader.GetValue("ReportName_pdf").ToString()),
                                                new XElement(cus + "SubjectType", joHeader.GetValue("SubjectType_pdf").ToString())
                                                )

                                           )
                             )
                );


            #endregion 

            var filename_pdfrpt = "";
            var folder = Path.GetFullPath("file/request/pdfrpt/");

            filename_pdfrpt = joData.GetValue("ktp").ToString() + "_" + pefindoid.ToString() + today.ToString();
            var path = folder + prefix_req + "pdfrpt_" + filename_pdfrpt + joHeader.GetValue("ext").ToString();

            root.Save(path);

            data.Add("filename", filename_pdfrpt);
            data.Add("reqdate", reqdate);

            return data;
        }


  

        #region global function
        public JObject PostDataXml([FromBody]XmlDocument xml, string soapaction)
        {
            var data = new JObject();
            var url = dbconn.domainGetApi("urlWS_pefindo");
            var handlesleep = Convert.ToInt32(dbconn.GetCredential("handle_sleep"));
            var credential = dbconn.GetCredential("Creadential_pefindo");
            var content_type = dbconn.GetCredential("Content_Type"); ;
            string outApi = "";
            byte[] restByte = { };
            XmlDocument docx = new XmlDocument();
            try
            {
                var client = new HttpClient();

                var _url = url;
                var _action = dbconn.domainGetApi(soapaction);
                XmlDocument soapEnvelopeXml = xml;
                HttpWebRequest webRequest = CreateWebRequest(_url, _action, credential);
                InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

                // begin async call to web request.
                IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

                Task<int> task = HandleSleepAsync(handlesleep);
                task.Wait();
                var x = task.Result;

                // suspend this thread until call is complete. You might want to
                // do something usefull here like update your UI.
                asyncResult.AsyncWaitHandle.WaitOne();

                // get the response from the completed web request.
                string soapResult;
                using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
                {
                    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                    {
                        soapResult = rd.ReadToEnd();
                    }
                }

                data = new JObject();
                data.Add("status", mc.GetMessage("api_output_ok"));
                data.Add("response", soapResult);

            }
            catch (Exception ex)
            {
                data = new JObject();
                data.Add("status", mc.GetMessage("api_output_not_ok"));
                outApi = ex.Message.ToString() + ". Url ==> " + url + " . Content type ==> " + content_type + ", username & pwd : " + credential;

                string filename = Path.GetFullPath("file/catch/catch.txt");  
                if (System.IO.File.Exists(filename))
                {
                    using (StreamWriter sw = System.IO.File.AppendText(filename))
                    {
                        sw.WriteLine(DateTime.Now);
                        sw.WriteLine(outApi);
                        sw.Close();
                    }
                }

                //XmlDocument resdocx_cus_company = new XmlDocument();
                //resdocx_cus_company.Load(new StringReader(outApi));
                ////save response custom report
                //var folder = Path.GetFullPath("file/catch/");
                //var path = folder + "catch.txt";
                //resdocx_cus_company.Save(path);


                data.Add("response", outApi);
            }


            return data;
        }



        //public string execExtAPIPostXML(string api, string xmlstr)
        //{
        //    var WebAPIURL = dbconn.domainGetApi(api);
        //    string requestStr = WebAPIURL;

        //    var client = new HttpClient();
        //    var contentData = new StringContent(xmlstr, System.Text.Encoding.UTF8, "text/xml");

        //    HttpResponseMessage response = client.PostAsync(requestStr, contentData).Result;
        //    string result = response.Content.ReadAsStringAsync().Result;
        //    return result;
        //}

        public JObject PostDataXmlnotsoap(string xmlreq)
        {
            var data = new JObject();
            var url = dbconn.domainGetApi("urlWS_pefindo");
            string outApi = "";


            try
            {
                var WebAPIURL = dbconn.domainGetApi("urlWS_pefindo");
                string requestStr = WebAPIURL;

                var client = new HttpClient();
                var contentData = new StringContent(xmlreq, System.Text.Encoding.UTF8, "text/xml");

                HttpResponseMessage response = client.PostAsync(requestStr, contentData).Result;

                string result = response.Content.ReadAsStringAsync().Result;

                data = new JObject();
                data.Add("status", mc.GetMessage("api_output_ok"));
                data.Add("response", result);
            }
            catch (Exception ex)
            {
                data = new JObject();
                data.Add("status", mc.GetMessage("api_output_not_ok"));
                outApi = ex.Message.ToString() + ". Url ==> " + url;

                string filename = Path.GetFullPath("file/catch/catch.txt");
                if (System.IO.File.Exists(filename))
                {
                    using (StreamWriter sw = System.IO.File.AppendText(filename))
                    {
                        sw.WriteLine(DateTime.Now);
                        sw.WriteLine(outApi);
                        sw.Close();
                    }
                }

                //XmlDocument resdocx_cus_company = new XmlDocument();
                //resdocx_cus_company.Load(new StringReader(outApi));
                ////save response custom report
                //var folder = Path.GetFullPath("file/catch/");
                //var path = folder + "catch.txt";
                //resdocx_cus_company.Save(path);


                data.Add("response", outApi);
            }


            return data;
        }



        private static HttpWebRequest CreateWebRequest(string url, string action, string credential)
        {
            byte[] credentialBuffer = new UTF8Encoding().GetBytes(credential);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(credentialBuffer));
            webRequest.Headers.Add("SOapAction", action);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        private static HttpWebRequest CreateWebRequestnotsoap(string url)
        {
            //byte[] credentialBuffer = new UTF8Encoding().GetBytes(credential);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            //webRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(credentialBuffer));
            //webRequest.Headers.Add("SOapAction", action);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        private static void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
        }

        static async Task<int> HandleSleepAsync(int hdlsleep)
        {
            System.Threading.Thread.Sleep(hdlsleep);
            return 1;
        }

        public void Insertfilemapbk(string rptname, string ttable, string ktp, string pefindoid, string paths, string count_pefid, string numofdata)
        {
            var jaReturn = new JArray();
            var provider = dbconn.sqlprovider();
            var cstrname = dbconn.constringName("skybk");
            var split = "|";
            var schema = "pefindo";

            string spname = "pfd_insert_filemap";
            string p1 = "p_rptname" + split + rptname.ToLower() + split + "s";
            string p2 = "p_ttable" + split + ttable + split + "s";
            string p3 = "p_ktp" + split + ktp + split + "s";
            string p4 = "p_pefindoid" + split + pefindoid + split + "s";
            string p5 = "p_path" + split + paths + split + "s";
            string p6 = "p_countpefid" + split + count_pefid + split + "s";
            string p7 = "p_numofdata" + split + numofdata + split + "s";

            bc.ExecSqlWithReturnCustomSplit(provider, cstrname, split, schema, spname, p1, p2, p3, p4, p5, p6, p7);
        }
        public JObject chkjarowinkler(string ktp, string fullnama, string dob, string address, string ttable)
        {
            var jaReturn = new JArray();
            var joReturn = new JObject();
            var provider = dbconn.sqlprovider();
            var cstrname = dbconn.constringName("skyen");
            var split = "||";
            var schema = "public";

            string spname = "chkSimilarity";
            string p1 = "@ktp" + split + ktp + split + "s";
            string p2 = "@fullname" + split + fullnama + split + "s";
            string p3 = "@dateofbirth" + split + dob + split + "s";
            string p4 = "@address" + split + address + split + "s";
            string p5 = "@ttable" + split + ttable + split + "s";

            var retObject = new List<dynamic>();
            retObject =  bc.ExecSqlWithReturnCustomSplit(provider, cstrname, split, schema, spname, p1, p2, p3, p4, p5);
            jaReturn = lc.convertDynamicToJArray(retObject);
            if (jaReturn.Count > 0)
            {
                joReturn = JObject.Parse(jaReturn[0].ToString());
            }

            return joReturn;

        }


        public void updatecounterpefid(string ttable)
        {
            var jaReturn = new JArray();
            var provider = dbconn.sqlprovider();
            var cstrname = dbconn.constringName("skyen");
            var split = "|";

            string spname = "update_counter_pefid";
            string p1 = "@ttable" + split + ttable + split + "s";
            bc.ExecSqlWithoutReturnCustomSplit(provider, cstrname, split, spname, p1);
        }

        private JObject CheckDataObject(string rawdata)
        {
            var data = new JObject();
            try
            {
                var jToken = JToken.Parse(rawdata);
                if (jToken is JObject)
                {
                    data.Add("object_type", "JObject");
                }
                else if (jToken is JArray)
                {
                    data.Add("object_type", "JArray");
                }
                else
                {
                    data.Add("object_type", "String");
                }
            }
            catch (Exception ex)
            {
                data.Add("object_type", "String");
            }

            return data;
        }



        #endregion

        //======================== new =========================
        public JArray insertlogreqandres(string refid, string ktp, string object_type, string pefindoid, string flag, string id)
        {
            var jaReturn = new JArray();
            var provider = dbconn.sqlprovider();
            var cstrname = dbconn.constringName("skybk");
            var split = "|";
            var schema = "pefindo";

            string spname = "insert_log_requestAndRespon";
            string p1 = "@refid" + split + refid + split + "s";
            string p2 = "@ktp" + split + ktp + split + "s";
            string p3 = "@object" + split + object_type + split + "s";
            string p4 = "@pefindoid" + split + pefindoid + split + "s";
            string p5 = "@flag" + split + flag + split + "s";
            string p6 = "@id" + split + id + split + "i";

            var retObject = new List<dynamic>();
            retObject = bc.ExecSqlWithReturnCustomSplit(provider, cstrname, split, schema, spname, p1, p2, p3, p4, p5, p6);
            jaReturn = lc.convertDynamicToJArray(retObject);

            return jaReturn;
        }
        //======================== end =========================
    }
}


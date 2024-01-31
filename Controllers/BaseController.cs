using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sky.bk.integration.Libs;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net;
using System.Dynamic;
using System.Globalization;
using Npgsql;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace sky.bk.integration.Controllers
{
    public class BaseController : Controller
    {
        private lDbConn dbconn = new lDbConn();
        private MessageController mc = new MessageController();
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public void ExecSqlWithoutReturnCustomSplit(string strname, string cstsplit, string schema, string spname, params string[] list)
        {
            var retObject = new List<dynamic>();
            string message = "";
            StringBuilder sb = new StringBuilder();
            var conn = dbconn.constringList(strname);

            spname = schema + "." + spname;
            NpgsqlConnection nconn = new NpgsqlConnection(conn);
            nconn.Open();
            NpgsqlCommand cmd = new NpgsqlCommand(spname, nconn);
            cmd.CommandType = CommandType.StoredProcedure;
            if (list != null && list.Count() > 0)
            {
                foreach (var item in list)
                {
                    var pars = item.Split(cstsplit);

                    if (pars.Count() > 2)
                    {
                        if (pars[2] == "i")
                        {
                            cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), Convert.ToInt32(pars[1]));
                        }
                        else if (pars[2] == "s")
                        {
                            cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), (Convert.ToString(pars[1])));
                        }
                        else if (pars[2] == "d")
                        {
                            cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), Convert.ToDecimal(pars[1]));
                        }
                        else if (pars[2] == "dt")
                        {
                            cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), DateTime.ParseExact(pars[1], "yyyy-MM-dd", CultureInfo.InvariantCulture));
                        }
                        else if (pars[2] == "b")
                        {
                            cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), Convert.ToBoolean(pars[1]));
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), pars[1]);
                        }
                    }
                    else if (pars.Count() > 1)
                    {
                        cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), pars[1]);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), pars[0]);
                    }
                }
            }
            try
            {
                cmd.ExecuteNonQuery();
                message = mc.GetMessage("execdb_success");
                if (nconn.State.Equals(ConnectionState.Open))
                {
                    nconn.Close();
                }
                NpgsqlConnection.ClearPool(nconn);
            }
            catch (NpgsqlException e)
            {
                message = e.Message;
                if (nconn.State.Equals(ConnectionState.Open))
                {
                    nconn.Close();
                }
                NpgsqlConnection.ClearPool(nconn);
            }
        }

        public List<dynamic> ExecSqlWithReturnCustomSplit(string dbprv, string strname, string cstsplit, string schema, string spname, params string[] list)
        {
            var retObject = new List<dynamic>();
            StringBuilder sb = new StringBuilder();
            var conn = dbconn.constringList_v2(dbprv, strname);

            if (dbprv == "postgresql")
            {
                spname = schema + "." + spname;
                NpgsqlConnection nconn = new NpgsqlConnection(conn);
                nconn.Open();
                //NpgsqlTransaction tran = nconn.BeginTransaction();
                NpgsqlCommand cmd = new NpgsqlCommand(spname, nconn);
                cmd.CommandType = CommandType.StoredProcedure;

                if (list != null && list.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        var pars = item.Split(cstsplit);

                        if (pars.Count() > 2)
                        {
                            if (pars[2] == "i")
                            {
                                cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), Convert.ToInt32(pars[1]));
                            }
                            else if (pars[2] == "s")
                            {
                                cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), (Convert.ToString(pars[1])));
                            }
                            else if (pars[2] == "d")
                            {
                                cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), Convert.ToDecimal(pars[1]));
                            }
                            else if (pars[2] == "dt")
                            {
                                cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), DateTime.ParseExact(pars[1], "yyyy-MM-dd", CultureInfo.InvariantCulture));
                            }
                            else if (pars[2] == "b")
                            {
                                cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), Convert.ToBoolean(pars[1]));
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), pars[1]);
                            }
                        }
                        else if (pars.Count() > 1)
                        {
                            cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), pars[1]);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), pars[0]);
                        }
                    }
                }

                NpgsqlDataReader dr = cmd.ExecuteReader();

                if (dr == null || dr.FieldCount == 0)
                {
                    return retObject;
                }

                retObject = GetDataObjPgsql(dr);
                nconn.Close();
            }
            else if (dbprv == "sqlserver")
            {
                SqlConnection nconn = new SqlConnection(conn);
                nconn.Open();
                SqlCommand cmd = new SqlCommand(spname, nconn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = nconn.ConnectionTimeout;

                if (list != null && list.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        var pars = item.Split(cstsplit);

                        if (pars.Count() > 2)
                        {
                            if (pars[2] == "i")
                            {
                                cmd.Parameters.AddWithValue(pars[0], Convert.ToInt32(pars[1]));
                            }
                            else if (pars[2] == "s")
                            {
                                cmd.Parameters.AddWithValue(pars[0], Convert.ToString(pars[1]));
                            }
                            else if (pars[2] == "d")
                            {
                                cmd.Parameters.AddWithValue(pars[0], Convert.ToDecimal(pars[1]));
                            }
                            else if (pars[2] == "dt")
                            {
                                cmd.Parameters.AddWithValue(pars[0], DateTime.ParseExact(pars[1], "yyyy-MM-dd", CultureInfo.InvariantCulture));
                            }
                            else if (pars[2] == "b")
                            {
                                cmd.Parameters.AddWithValue(pars[0], Convert.ToBoolean(pars[1]));
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue(pars[0], pars[1]);
                            }
                        }
                        else if (pars.Count() > 1)
                        {
                            cmd.Parameters.AddWithValue(pars[0], pars[1]);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue(pars[0], pars[0]);
                        }
                    }
                }

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr == null || dr.FieldCount == 0)
                {
                    return retObject;
                }

                retObject = GetDataObjSqlsvr(dr);
                nconn.Close();
            }

            return retObject;
        }

        public void ExecSqlWithoutReturnCustomSplit(string dbprv, string strname, string cstsplit, string schema, string spname, params string[] list)
        {
            var retObject = new List<dynamic>();
            string message = "";
            StringBuilder sb = new StringBuilder();
            //var conn = dbconn.constringList(dbprv, strname);
            var conn = dbconn.constringList_v2(dbprv, strname);

            if (dbprv == "postgresql")
            {
                spname = schema + "." + spname;
                NpgsqlConnection nconn = new NpgsqlConnection(conn);
                nconn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand(spname, nconn);
                cmd.CommandType = CommandType.StoredProcedure;
                if (list != null && list.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        var pars = item.Split(cstsplit);

                        if (pars.Count() > 2)
                        {
                            if (pars[2] == "i")
                            {
                                cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), Convert.ToInt32(pars[1]));
                            }
                            else if (pars[2] == "s")
                            {
                                cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), Convert.ToString(pars[1]));
                            }
                            else if (pars[2] == "d")
                            {
                                cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), Convert.ToDecimal(pars[1]));
                            }
                            else if (pars[2] == "b")
                            {
                                cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), Convert.ToBoolean(pars[1]));
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), pars[1]);
                            }
                        }
                        else if (pars.Count() > 1)
                        {
                            cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), pars[1]);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue((pars[0].ToString()).Replace("@", "p_"), pars[0]);
                        }
                    }
                }
                try
                {
                    cmd.ExecuteNonQuery();
                    message = mc.GetMessage("execdb_success");
                }
                catch (NpgsqlException e)
                {
                    message = e.Message;
                }
                finally
                {
                    nconn.Close();
                }
            }
            else if (dbprv == "sqlserver")
            {
                SqlConnection nconn = new SqlConnection(conn);
                nconn.Open();
                SqlCommand cmd = new SqlCommand(spname, nconn);
                cmd.CommandType = CommandType.StoredProcedure;
                if (list != null && list.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        var pars = item.Split(cstsplit);

                        if (pars.Count() > 2)
                        {
                            if (pars[2] == "i")
                            {
                                cmd.Parameters.AddWithValue(pars[0], Convert.ToInt32(pars[1]));
                            }
                            else if (pars[2] == "s")
                            {
                                cmd.Parameters.AddWithValue(pars[0], Convert.ToString(pars[1]));
                            }
                            else if (pars[2] == "d")
                            {
                                cmd.Parameters.AddWithValue(pars[0], Convert.ToDecimal(pars[1]));
                            }
                            else if (pars[2] == "b")
                            {
                                cmd.Parameters.AddWithValue(pars[0], Convert.ToBoolean(pars[1]));
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue(pars[0], pars[1]);
                            }
                        }
                        else if (pars.Count() > 1)
                        {
                            cmd.Parameters.AddWithValue(pars[0], pars[1]);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue(pars[0], pars[0]);
                        }
                    }
                }
                try
                {
                    cmd.ExecuteNonQuery();
                    message = mc.GetMessage("execdb_success");
                }
                catch (NpgsqlException e)
                {
                    message = e.Message;
                }
                finally
                {
                    nconn.Close();
                }
            }
        }

        public List<dynamic> GetDataObjPgsql(NpgsqlDataReader dr)
        {
            var retObject = new List<dynamic>();
            while (dr.Read())
            {
                var dataRow = new ExpandoObject() as IDictionary<string, object>;
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    dataRow.Add(
                           dr.GetName(i),
                           dr.IsDBNull(i) ? null : dr[i] // use null instead of {}
                   );
                }
                retObject.Add((ExpandoObject)dataRow);
            }

            return retObject;
        }
        public List<dynamic> GetDataObjSqlsvr(SqlDataReader dr)
        {
            var retObject = new List<dynamic>();
            while (dr.Read())
            {
                var dataRow = new ExpandoObject() as IDictionary<string, object>;
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    dataRow.Add(
                           dr.GetName(i),
                           dr.IsDBNull(i) ? null : dr[i] // use null instead of {}
                   );
                }
                retObject.Add((ExpandoObject)dataRow);
            }

            return retObject;
        }

        public string execExtAPIPostWithToken(string api, string path, string json, string credential)
        {
            var WebAPIURL = dbconn.domainGetApi(api);
            string requestStr = WebAPIURL + path;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", credential);
            var contentData = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            //contentData.Headers.Add("Authorization", credential);   

            HttpResponseMessage response = client.PostAsync(requestStr, contentData).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            return result;
        }

        public string execExtAPIPostWithToken(string api, string path, string json)
        {
            var WebAPIURL = dbconn.domainGetApi(api);
            string requestStr = WebAPIURL + path;

            var client = new HttpClient();
            var contentData = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(requestStr, contentData).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            return result;
        }

    }
}

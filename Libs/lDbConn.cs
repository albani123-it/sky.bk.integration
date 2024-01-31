using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace sky.bk.integration.Libs
{
    public class lDbConn
    {
        private lConvert lc = new lConvert();
        public string sqlprovider()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            return "" + config.GetSection("SqlProvider:provider").Value.ToString();
        }

        public string constringName(string cstr)
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            return "" + config.GetSection("constringName:" + cstr).Value.ToString();
        }

        public string conString()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            return "" + config.GetSection("DbContextSettings:ConnectionString").Value.ToString();
        }

        public string conStringLog()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            return "" + config.GetSection("DbContextSettings:ConnectionString_log").Value.ToString();
        }

        #region -- connnection string by database --
        public string constringList(string strname)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var config = builder.Build();
            var configPass = lc.decrypt(config.GetSection("configPass:passwordDB").Value.ToString());
            var configDB = config.GetSection("DbContextSettings:" + strname).Value.ToString();

            var repPass = configDB.Replace("{pass}", configPass);
            return "" + repPass;
        }

        public string constringList_v2(string dbprv, string strname)
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();

            //var configPass = lc.decrypt(config.GetSection("configPass:passwordDB").Value.ToString());

            var configDB = config.GetSection("DbContextSettings:" + dbprv + ":" + strname).Value.ToString();

            //var repPass = configDB.Replace("{pass}", configPass);
            return "" + configDB;


        }

        public string conStringLogProcess()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var config = builder.Build();
            var configPass = lc.decrypt(config.GetSection("configPass:passwordDB").Value.ToString());
            var configDB = config.GetSection("DbContextSettings:ConnectionString_log").Value.ToString();

            var repPass = configDB.Replace("{pass}", configPass);
            return "" + repPass;
        }

        #endregion

        public string getAppSettingParam(string group, string api)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var config = builder.Build();
            return "" + config.GetSection(group + ":" + api).Value.ToString();
        }

        public string domainGetApi(string api)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var config = builder.Build();
            return "" + config.GetSection("APISettings:" + api).Value.ToString();
        }

        public string domainGetTokenCredential(string param)
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            return config.GetSection("TokenAuthentication:" + param).Value.ToString();
        }

        public string domainPostApi()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            return "" + config.GetSection("DomainSettings:urlPostDomainAPI").Value.ToString();
        }

        public string getFromAddress()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            return "" + config.GetSection("NotificationSetting:From").Value.ToString();
        }

        public string getTitleFrom()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            return "" + config.GetSection("NotificationSetting:TitleFrom").Value.ToString();
        }

        public string getTitleTo()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            return "" + config.GetSection("NotificationSetting:TitleTo").Value.ToString();
        }

        public string getSubjectNotification()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            return "" + config.GetSection("NotificationSetting:SubjectNotification").Value.ToString();
        }

        public string getLogo()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            return "" + config.GetSection("NotificationSetting:PathLogo").Value.ToString();
        }

        public string getSmtpServer()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            return "" + config.GetSection("NotificationSetting:SmtpServer").Value.ToString();
        }

        public string getSmtpPortNumber()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            return "" + config.GetSection("NotificationSetting:SmtpPortNumber").Value.ToString();
        }

        public string getAuthenticateUsr()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            return "" + config.GetSection("NotificationSetting:AuthenticateUsr").Value.ToString();
        }

        public string getAuthenticatePwd()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            return "" + config.GetSection("NotificationSetting:AuthenticatePwd").Value.ToString();
        }

        public string getByPassEmail()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            return "" + config.GetSection("NotificationSetting:ByPassEmail").Value.ToString();
        }

        public string getAnalystEmail()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            return "" + config.GetSection("NotificationSetting:AnalystEmail").Value.ToString();
        }

        public string getApprovalEmail()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            return "" + config.GetSection("NotificationSetting:ApprovalEmail").Value.ToString();
        }

        public string GetCredential(string data)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var config = builder.Build();
            return "" + config.GetSection("Credential:" + data).Value.ToString();
        }
    }
}

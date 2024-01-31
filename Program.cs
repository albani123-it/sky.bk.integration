using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace sky.bk.integration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true)
                .Build();

            var webHost = WebHost.CreateDefaultBuilder(args)
             .UseConfiguration(config)
             .ConfigureKestrel(options => { options.AddServerHeader = false; })
             .UseContentRoot(Directory.GetCurrentDirectory())
             .UseMetricsWebTracking()
             .UseMetrics(options => {
                 options.EndpointOptions = endpointsOptions =>
                 {
                     endpointsOptions.MetricsTextEndpointOutputFormatter = new MetricsPrometheusTextOutputFormatter();
                     endpointsOptions.MetricsEndpointOutputFormatter = new MetricsPrometheusProtobufOutputFormatter();
                     endpointsOptions.EnvironmentInfoEndpointEnabled = false;
                 };
             })
             .UseStartup<Startup>();

            return webHost;
        }

    }
}

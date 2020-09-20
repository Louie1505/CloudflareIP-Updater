using CloudflareIP_Updater;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FunFacts
{
    class Program
    {
        public static IConfigurationRoot configuration;
        public static async Task Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            // Build configuration
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            return Host.CreateDefaultBuilder(args)
            .ConfigureLogging(
              options => options.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Warning))
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>()
                  .Configure<EventLogSettings>(config =>
                  {
                      config.LogName = "Cloudflare IP Monitoring Service";
                      config.SourceName = "CFIP-Monitor";
                  });
                // Add access to generic IConfigurationRoot
                services.AddSingleton<IConfigurationRoot>(configuration);
            }).UseWindowsService();
        }
    }
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private Timer timer;
        private int interval;
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var freq = Program.configuration["frequency"];
            this.interval = (string.IsNullOrEmpty(freq) ? 2 : int.Parse(freq)) * 1000 * 60;
            this.timer = new Timer(Callback, null, interval, Timeout.Infinite);
        }
        private async void Callback(object state)
        {
            var currentIP = Helpers.CurrentPublicIP();
            if (string.IsNullOrEmpty(currentIP))
                throw new Exception("Unable to fetch IP");
            CloudflareAPIWorker worker = new CloudflareAPIWorker(Program.configuration["urlBase"], Program.configuration["xAuthEmail"], Program.configuration["xAuthKey"]);
            var zones = await worker.GetZones();
            foreach (var zone in zones)
            {
                var dnsRecords = await worker.GetDNSRecords(zone.id);
                foreach (var dnsRecord in dnsRecords)
                {
                    if (!Regex.IsMatch(dnsRecord.content, currentIP))
                    {
                        var newDns = dnsRecord;
                        newDns.content = currentIP;
                        await worker.UpdateDNSRecords(dnsRecord, newDns);
                        Helpers.LogOutWarning($"UPDATED DNS RECORD FOR {dnsRecord.name} . CHANGED IP FROM {dnsRecord.content} TO {currentIP}.");
                    }
                }
            }
            //Set next timer
            timer.Change(interval, Timeout.Infinite);
        }
    }
}
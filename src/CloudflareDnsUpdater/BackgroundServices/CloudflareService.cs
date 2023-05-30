using CloudFlare.Client;
using CloudFlare.Client.Api.Zones.DnsRecord;
using CloudFlare.Client.Enumerators;
using CloudflareDnsUpdater.Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace CloudflareDnsUpdater.BackgroundServices
{
    public class CloudflareService : BackgroundService
    {
        private readonly ILogger<CloudflareService> _logger;
        private readonly AppSettings _appSettings;
        public CloudflareService(ILogger<CloudflareService> logger, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("CloudflareService running at: {time}", DateTimeOffset.Now);
                    this.Run(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception on CloudflareService - ExecuteAsync");
                }
                GC.Collect();
                await Task.Delay(TimeSpan.FromMinutes(_appSettings.Delay), stoppingToken);
            }
        }
        private void Run(CancellationToken stoppingToken)
        {
            var selectedDomains = _appSettings.Domain.Split(' ').ToList();
            var client = new CloudFlareClient(_appSettings.Token);
            var myIp = IpHelper.GetIp();
            var zoneList = client.Zones.GetAsync(cancellationToken: stoppingToken).Result;
            foreach (var zoneItem in zoneList.Result)
            {
                var aDnsRecords = client.Zones.DnsRecords.GetAsync(zoneItem.Id, new DnsRecordFilter { Type = DnsRecordType.A }, null, stoppingToken).Result;
                var filteredRecords = aDnsRecords.Result.Where(x => selectedDomains.Contains(x.Name)).ToList();
                foreach (var filteredRecord in filteredRecords)
                {
                    if (!string.IsNullOrWhiteSpace(myIp) && !string.IsNullOrWhiteSpace(filteredRecord.Content) && myIp != filteredRecord.Content)
                    {
                        var modified = new ModifiedDnsRecord
                        {
                            Type = DnsRecordType.A,
                            Name = filteredRecord.Name,
                            Content = myIp,
                        };
                        var updateResult = client.Zones.DnsRecords.UpdateAsync(zoneItem.Id, filteredRecord.Id, modified, stoppingToken).Result;
                        if (updateResult.Success)
                        {
                            _logger.LogInformation("{Name} domain is updated. Old ip - {content} New ip - {myIp}", filteredRecord.Name, filteredRecord.Content, myIp);
                        }
                    }
                }
            }
        }
    }
}

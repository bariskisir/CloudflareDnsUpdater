using CloudflareDnsUpdater.Helpers;
using CloudflareDnsUpdater.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace CloudflareDnsUpdater.BackgroundServices
{
    public class CloudflareService : BackgroundService
    {
        private readonly ILogger<CloudflareService> _logger;
        private readonly IConfiguration _configuration;
        public CloudflareService(ILogger<CloudflareService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("CloudflareService running at: {time}", DateTimeOffset.Now);
                try
                {
                    this.Run();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception on CloudflareService - ExecuteAsync");
                }
                await Task.Delay(TimeSpan.FromMinutes(_configuration.GetValue<int>("Delay")), stoppingToken);
            }
        }
        private void Run()
        {
            var selectedDomainsConfValue = _configuration.GetValue<string>("Domain");
            var selectedDomains = selectedDomainsConfValue.Split(' ').ToList();
            var token = _configuration.GetValue<string>("Token");
            var client = this.GetClient(token);
            var myIp = IpHelper.GetIp();
            var zoneIdList = this.GetZoneIdList(client);
            foreach (var zoneIdItem in zoneIdList)
            {
                var dnsRecordList = this.GetDnsRecords(client, zoneIdItem);
                var filteredDnsRecordList = dnsRecordList.Where(x => x.type == "A")
                                                         .Where(x => selectedDomains.Contains(x.name)).ToList();
                foreach (var filteredDnsRecordItem in filteredDnsRecordList)
                {
                    if (myIp != filteredDnsRecordItem.content)
                    {
                        this.UpdateDnsRecord(client, myIp, zoneIdItem, filteredDnsRecordItem.id);
                        _logger.LogInformation("{name} domain is updated. Old ip - {content} New ip - {myIp}", filteredDnsRecordItem.name, filteredDnsRecordItem.content, myIp);
                    }
                }
            }
        }
        private void UpdateDnsRecord(RestClient client, string myIp, string zoneIdItem, string dnsRecordId)
        {
            var dnsRecordRequest = new DnsRecordRequest() { content = myIp };
            var updateDnsRecordsRequest = new RestRequest($"zones/{zoneIdItem}/dns_records/{dnsRecordId}", Method.PATCH);
            updateDnsRecordsRequest.RequestFormat = DataFormat.Json;
            updateDnsRecordsRequest.AddJsonBody(dnsRecordRequest);
            client.Execute(updateDnsRecordsRequest);
        }
        private List<DnsRecord> GetDnsRecords(RestClient client, string zoneIdItem)
        {
            var dnsRecordsRequest = new RestRequest($"zones/{zoneIdItem}/dns_records", Method.GET);
            var dnsRecordsResponse = client.Execute(dnsRecordsRequest);
            var dnsRecordsContent = dnsRecordsResponse.Content;
            var dnsRecordResponse = JsonConvert.DeserializeObject<DnsRecordsResponse>(dnsRecordsContent);
            var dnsRecordList = dnsRecordResponse.result;
            return dnsRecordList;
        }
        private List<string> GetZoneIdList(RestClient client)
        {
            var zonesRequest = new RestRequest("zones", Method.GET);
            var zonesResponse = client.Execute(zonesRequest);
            var zonesContent = zonesResponse.Content;
            var zones = JsonConvert.DeserializeObject<ZonesResponse>(zonesContent);
            var zoneIdList = zones.result.Select(x => x.id).ToList();
            return zoneIdList;
        }
        private RestClient GetClient(string token)
        {
            var baseApiUrl = "https://api.cloudflare.com/client/v4/";
            var client = new RestClient($"{baseApiUrl}");
            client.AddDefaultHeader("Authorization", $"Bearer {token}");
            return client;
        }
    }
}

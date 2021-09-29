using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;

namespace CloudflareDnsUpdater.Helpers
{
    public static class IpHelper
    {
        public static List<string> IpAddressProviders = new List<string>() 
        { 
            "https://api.ipify.org",
            "http://ipv4bot.whatismyipaddress.com",
            "http://icanhazip.com",
            "http://bot.whatismyipaddress.com",
            "http://ipinfo.io/ip",
            "https://ipecho.net/plain",
            "https://checkip.amazonaws.com"
        };

        public static string GetIp()
        {
            var ip = string.Empty;
            foreach (var ipAddressProviderItem in IpAddressProviders)
            {
                try
                {
                    var client = new RestClient(ipAddressProviderItem);
                    var request = new RestRequest(Method.GET);
                    var response = client.Execute(request);
                    var content = response.Content;
                    if (IsValidIp(content))
                    {
                        ip = content;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Exception on IpHelper - GetIp - {ipAddressProviderItem}", ipAddressProviderItem);
                }
            }
            if (string.IsNullOrWhiteSpace(ip))
            {
                throw new Exception("Couldnt get the ip address");
            }
            return ip;
        }
        private static bool IsValidIp(string ip)
        {
            var result = false;
            if (IPAddress.TryParse(ip, out _))
            {
                result = true;
            }
            return result;
        }
    }
}

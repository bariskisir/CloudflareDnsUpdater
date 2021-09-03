using RestSharp;
namespace CloudflareDnsUpdater.Helpers
{
    public static class IpHelper
    {
        public static string GetIp()
        {
            var client = new RestClient("https://api.ipify.org/");
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);
            var content = response.Content;
            return content;
        }
    }
}

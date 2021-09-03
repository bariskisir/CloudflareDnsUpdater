namespace CloudflareDnsUpdater.Models
{
    public class DnsRecord
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string content { get; set; }
        public int ttl { get; set; }
        public bool proxied { get; set; }
    }
}

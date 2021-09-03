using System.Collections.Generic;

namespace CloudflareDnsUpdater.Models
{
    public class DnsRecordsResponse
    {
        public List<DnsRecord> result { get; set; }
    }
}

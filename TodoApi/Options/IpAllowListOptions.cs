using System.Collections.Generic;

namespace TodoApi.Options
{
    public class IpAllowListOptions
    {
        public const string SectionName = "IpAllowList";
        public List<string> AllowedIps { get; set; }
    }
}

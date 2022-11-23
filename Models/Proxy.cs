using System.Collections.Generic;

namespace angular_server.Models
{
    public class Proxy
    {
        
        public string DisplayName { get; set; }
        
        public string TargetHost { get; set; }
        
        public List<string> UriStarts { get; set; }
        
    }
}
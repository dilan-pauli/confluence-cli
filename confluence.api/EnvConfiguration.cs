using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace confluence.api
{
    public class EnvConfiguration : IConfluenceConfiguration
    {
        public string BaseUrl { get; private set; }

        public string Username { get; private set; }

        public string APIKey { get; private set; }

        public EnvConfiguration()
        {
            this.BaseUrl = System.Environment.GetEnvironmentVariable("CONFLUENCE_URL") 
                ?? throw new InvalidOperationException("CONFLUENCE_URL environment variable not found, please provide in '{instance_name}.atlassian.net'");
            this.Username = System.Environment.GetEnvironmentVariable("CONFLUENCE_USER") 
                ?? throw new InvalidOperationException("CONFLUENCE_USER environment variable not found");
            this.APIKey = System.Environment.GetEnvironmentVariable("CONFLUENCE_APIKEY")
                ?? throw new InvalidOperationException("CONFLUENCE_APIKEY environment variable not found");
        }
    }
}

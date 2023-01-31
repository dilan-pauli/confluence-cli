using RestSharp;
using RestSharp.Authenticators;

namespace confluence.api
{
    public interface IConfluenceClient
    {

    }

    public class ConfluenceRestSharpClient : IConfluenceClient
    {
        private readonly IConfluenceConfiguration config;
        private RestClient client;

        public ConfluenceRestSharpClient(IConfluenceConfiguration config)
        {
            this.config = config;
            var options = new RestClientOptions($"https://{config.BaseUrl}");

            client = new RestClient(options)
            {
                Authenticator = new HttpBasicAuthenticator(this.config.Username, this.config.APIKey)
            };
        }
    }
}
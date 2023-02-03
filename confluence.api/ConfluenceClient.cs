using Confluence.Api.Models;
using RestSharp;
using RestSharp.Authenticators;

namespace confluence.api
{
    public interface IConfluenceClient
    {
        Task<List<Space>> GetAllGlobalActiveSpaces();
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

        /// <summary>
        /// Gets all global, current spaces in the instance
        /// </summary>
        /// <remarks>TODO: Expand to allow parameter input. Deal with paging better.</remarks>
        /// <returns></returns>
        public async Task<List<Space>> GetAllGlobalActiveSpaces()
        {
            var request = new RestRequest("wiki/rest/api/space", Method.Get);
            request.AddParameter("type", "global");
            request.AddParameter("limit", "100");
            request.AddParameter("status", "current");
            var result = await client.GetAsync<SpaceArray>(request);

            return result?.results ?? throw new InvalidProgramException("Unable to get spaces from service.");
        }

    }
}
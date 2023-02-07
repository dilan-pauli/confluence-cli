using Confluence.Api.Models;
using System.Net.Http.Json;

namespace confluence.api
{
    public interface IConfluenceClient
    {
        Task<List<Space>> GetAllGlobalActiveSpaces();
    }

    public class ConfluenceRestSharpClient : IConfluenceClient
    {
        private HttpClient client;

        public ConfluenceRestSharpClient(HttpClient clientFactory)
        {
            if (clientFactory is null)
            {
                throw new ArgumentNullException(nameof(clientFactory));
            }
            this.client = client;
        }

        /// <summary>
        /// Gets all global, current spaces in the instance
        /// </summary>
        /// <remarks>TODO: Expand to allow parameter input. Deal with paging better.</remarks>
        /// <returns></returns>
        public async Task<List<Space>> GetAllGlobalActiveSpaces()
        {
            var uriBuilder = new UriBuilder("/wiki/rest/api/space");
            uriBuilder.Query = "type=global&limit=100&status=current";
            var result = await client.GetFromJsonAsync<SpaceArray>(uriBuilder.Uri);


            return result?.results ?? throw new InvalidProgramException("Unable to get spaces from service.");
        }

    }
}
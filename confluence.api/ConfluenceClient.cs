using System.Net.Http.Json;
using Confluence.Api.Models;

namespace confluence.api
{
    public interface IConfluenceClient
    {
        Task<List<Space>> GetAllGlobalActiveSpaces();
    }

    public class ConfluenceRestSharpClient : IConfluenceClient
    {
        private HttpClient client;

        public ConfluenceRestSharpClient(HttpClient client)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
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
            var result = await client.GetFromJsonAsync<SpaceArray>("/wiki/rest/api/space" +
                "?type=global&limit=100&status=current");

            return result?.results ?? throw new InvalidProgramException("Unable to get spaces from service.");
        }
    }
}
using System.Net.Http.Json;
using Confluence.Api.Models;

namespace confluence.api
{
    public interface IConfluenceClient
    {
        Task<List<Space>> GetAllGlobalActiveSpaces();

        Task<List<Content>> GetAllPagesForSpace(string spaceKey);
    }

    public class ConfluenceHttpClient : IConfluenceClient
    {
        private HttpClient client;

        public ConfluenceHttpClient(HttpClient client)
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

        /// <summary>
        /// Gets all global, current spaces in the instance
        /// </summary>
        /// <remarks>TODO: Expand to allow parameter input. Deal with paging better.</remarks>
        /// <returns></returns>
        public async Task<List<Content>> GetAllPagesForSpace(string spaceKey)
        {
            var returnResults = new List<Content>();

            var result = await client.GetFromJsonAsync<ContentArray>("/wiki/rest/api/content" +
                $"?spaceKey={spaceKey}&limit=100&expand=body.storage,version,history");
            returnResults.AddRange(result.results);

            while (!string.IsNullOrEmpty(result._links.next))
            {
                result = await client.GetFromJsonAsync<ContentArray>("/wiki" + result._links.next);
                returnResults.AddRange(result.results);
            }

            return returnResults;
        }
    }
}
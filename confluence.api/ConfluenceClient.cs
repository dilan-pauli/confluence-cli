using System.Net.Http.Json;
using Confluence.Api.Models;

namespace confluence.api
{
    public interface IConfluenceClient
    {
        Task<List<Space>> GetAllGlobalActiveSpaces();

        Task<List<Content>> GetAllPagesForSpace(string spaceKey);

        Task<List<Content>> GetPagesByCQL(string query);

        Task<List<Content>> GetPagesByCQL(string query, Action<int> pageProgress);
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
            var result = await client.GetFromJsonAsync<ConfluenceArray<Space>>("/wiki/rest/api/space" +
                "?type=global&limit=100&status=current");

            return result?.results ?? throw new InvalidProgramException("Unable to get spaces from service.");
        }

        private async Task<List<T>> FetchWithPagination<T>(string url, Action<int>? pageProgress = null)
        {
            var returnResults = new List<T>();

            var result = await client.GetFromJsonAsync<ConfluenceArray<T>>(url);

            if(result is null)
            {
                throw new InvalidOperationException("No results from GET");
            }

            returnResults.AddRange(result.results);

            while (!string.IsNullOrEmpty(result._links.next))
            {
                pageProgress?.Invoke(returnResults.Count);
                result = await client.GetFromJsonAsync<ConfluenceArray<T>>("/wiki" + result._links.next);

                if (result is null)
                {
                    throw new InvalidOperationException("No results from paginated GET");
                }
                returnResults.AddRange(result.results);
            }

            return returnResults;
        }

        /// <summary>
        /// Gets all global, current spaces in the instance
        /// </summary>
        /// <returns></returns>
        public async Task<List<Content>> GetAllPagesForSpace(string spaceKey)
        {
            var url = "/wiki/rest/api/content" +
                $"?spaceKey={spaceKey}&limit=100&expand=body.storage,version,history";

            return await FetchWithPagination<Content>(url);
        }

        /// <summary>
        /// Gets all the pages as content from the server, using the given CQL
        /// query to flter the results that are returned
        /// </summary>
        /// <param name="query">https://developer.atlassian.com/cloud/confluence/advanced-searching-using-cql/</param>
        /// <returns></returns>
        public async Task<List<Content>> GetPagesByCQL(string query)
        {
            return await GetPagesByCQL(query, null);
        }

        /// <summary>
        /// Gets all the pages as content from the server, using the given CQL
        /// query to flter the results that are returned
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageProgress">Called on every page returned by the api with the curren result count</param>
        /// <returns></returns>
        public async Task<List<Content>> GetPagesByCQL(string query, Action<int>? pageProgress)
        {
            var url = "/wiki/rest/api/content/search" +
                $"?limit=100&expand=body.storage,version,history&cql={query}";

            return await FetchWithPagination<Content>(url, pageProgress);
        }
    }
}
using System.Net.Http.Json;
using Confluence.Api.Models;

namespace confluence.api
{
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

            if (result is null)
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

        public async Task<List<Content>> GetAllContentForSpace(string spaceKey)
        {
            var url = "/wiki/rest/api/content" +
                $"?spaceKey={spaceKey}&limit=100&expand=body.storage,version,history";

            return await FetchWithPagination<Content>(url);
        }

        public async Task<List<Content>> GetContentByCQL(string query)
        {
            return await GetContentByCQL(query, null);
        }

        public async Task<List<Content>> GetContentByCQL(string query, Action<int>? pageProgress = null)
        {
            var url = "/wiki/rest/api/content/search" +
                $"?limit=100&expand=body.storage,version,history&cql={query}";

            return await FetchWithPagination<Content>(url, pageProgress);
        }

        public Task<IDictionary<int, Page>> GetCurrentPagesInSpace(int spaceId, Action<int>? pageProgress = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<InlineComment>> GetInlineCommentsOnPage(int pageId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<FooterComment>> GetFooterCommentsOnPage(int pageId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetViewsOfPage(int pageId, DateTime? fromDate = null)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetViewersOfPage(int pageId, DateTime? fromDate = null)
        {
            throw new NotImplementedException();
        }
    }
}
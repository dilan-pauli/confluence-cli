using System.Globalization;
using System.Net.Http.Json;
using Confluence.Api.Models;

namespace confluence.api
{
    public class ConfluenceHttpClient : IConfluenceClient
    {
        public static string RETURN_LIMIT = "250";

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
                $"?type=global&limit={RETURN_LIMIT}&status=current");

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

            while (!string.IsNullOrEmpty(result._links?.next))
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

        private async Task<IDictionary<string, T>> FetchWithPaginationV2<T>(string url, Action<int>? pageProgress = null) where T : ConfluenceResponse
        {
            var returnResults = new Dictionary<string, T>();

            var result = await client.GetFromJsonAsync<ConfluenceArray<T>>(url);

            if (result is null)
            {
                throw new InvalidOperationException("No results from GET");
            }

            result.results.ForEach(x => returnResults.Add(x.id, x));

            while (!string.IsNullOrEmpty(result._links?.next))
            {
                pageProgress?.Invoke(returnResults.Count);
                result = await client.GetFromJsonAsync<ConfluenceArray<T>>(result._links.next);

                if (result is null)
                {
                    throw new InvalidOperationException("No results from paginated GET");
                }
                result.results.ForEach(x => returnResults.Add(x.id, x));
            }

            return returnResults;
        }

        public async Task<List<Content>> GetAllContentForSpace(string spaceKey)
        {
            var url = "/wiki/rest/api/content" +
                $"?spaceKey={spaceKey}&limit={RETURN_LIMIT}&expand=body.storage,version,history";

            return await FetchWithPagination<Content>(url);
        }

        public async Task<List<Content>> GetContentByCQL(string query)
        {
            return await GetContentByCQL(query, null);
        }

        public async Task<List<Content>> GetContentByCQL(string query, Action<int>? pageProgress = null)
        {
            var url = "/wiki/rest/api/content/search" +
                $"?limit={RETURN_LIMIT}&expand=body.storage,version,history&cql={query}";

            return await FetchWithPagination<Content>(url, pageProgress);
        }

        public async Task<IDictionary<string, Page>> GetCurrentPagesInSpace(long spaceId, Action<int>? pageProgress = null)
        {
            var url = $"/wiki/api/v2/spaces/{spaceId}/pages" +
                $"?status=current&limit={RETURN_LIMIT}&serialize-ids-as-strings=true";

            return await FetchWithPaginationV2<Page>(url, pageProgress);
        }

        public async Task<IEnumerable<InlineComment>> GetInlineCommentsOnPage(string pageId)
        {
            var url = $"/wiki/api/v2/pages/{pageId}/inline-comments" +
                $"?limit={RETURN_LIMIT}&serialize-ids-as-strings=true";

            var results = await FetchWithPaginationV2<InlineComment>(url);

            return results.Values.AsEnumerable();
        }

        public async Task<IEnumerable<FooterComment>> GetFooterCommentsOnPage(string pageId)
        {
            var url = $"/wiki/api/v2/pages/{pageId}/footer-comments" +
                $"?limit={RETURN_LIMIT}&serialize-ids-as-strings=true";

            var results = await FetchWithPaginationV2<FooterComment>(url);

            return results.Values.AsEnumerable();
        }

        public async Task<int> GetViewsOfPage(string pageId, DateTime? fromDate = null)
        {
            var url = $"/wiki/rest/api/analytics/content/{pageId}/views";

            if (fromDate is not null)
                url += $"?fromDate={fromDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

            var result = await client.GetFromJsonAsync<Views>(url);

            return result?.count ?? throw new InvalidProgramException($"Unable to get views from page {pageId}.");
        }

        public async Task<int> GetViewersOfPage(string pageId, DateTime? fromDate = null)
        {
            var url = $"/wiki/rest/api/analytics/content/{pageId}/viewers";

            if (fromDate is not null)
                url += $"?fromDate={fromDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

            var result = await client.GetFromJsonAsync<Views>(url);

            return result?.count ?? throw new InvalidProgramException($"Unable to get viewers from page {pageId}.");
        }
    }
}
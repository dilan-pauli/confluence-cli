using Confluence.Api.Models;

namespace confluence.api
{
    public interface IConfluenceClient
    {
        Task<List<Space>> GetAllGlobalActiveSpaces();

        /// <summary>
        /// Gets all the current pages in the given space.
        /// </summary>
        /// <param name="spaceId"></param>
        /// <returns>Returns a hash table of the pages to their content for easy parent lookup.</returns>
        Task<IDictionary<string, Page>> GetCurrentPagesInSpace(long spaceId, Action<int>? pageProgress = null);

        /// <summary>
        /// Gets all the inline comments on a given page.
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        Task<IEnumerable<InlineComment>> GetInlineCommentsOnPage(string pageId);

        /// <summary>
        /// Gets all the footer comments for the given page
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        Task<IEnumerable<FooterComment>> GetFooterCommentsOnPage(string pageId);

        /// <summary>
        /// Gets the total number of views for the given page.
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="fromDate">Views from this time to now.</param>
        /// <returns></returns>
        Task<int> GetViewsOfPage(string pageId, DateTime? fromDate = null);

        /// <summary>
        /// Gets the total number of unique views for the given page.
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="fromDate">Views from this time to now.</param>
        /// <returns></returns>
        Task<int> GetViewersOfPage(string pageId, DateTime? fromDate = null);

        /// <summary>
        /// Gets all global, current spaces in the instance
        /// </summary>
        /// <returns></returns>
        Task<List<Content>> GetAllContentForSpace(string spaceKey);

        /// <summary>
        /// Gets all the pages as content from the server, using the given CQL
        /// query to flter the results that are returned
        /// </summary>
        /// <param name="query">https://developer.atlassian.com/cloud/confluence/advanced-searching-using-cql/</param>
        /// <returns></returns>
        Task<List<Content>> GetContentByCQL(string query);

        /// <summary>
        /// Gets all the pages as content from the server, using the given CQL
        /// query to flter the results that are returned
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageProgress">Called on every page returned by the api with the curren result count</param>
        /// <returns></returns>
        Task<List<Content>> GetContentByCQL(string query, Action<int>? pageProgress = null);
    }
}
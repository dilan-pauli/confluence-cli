namespace confluence.api
{
    public interface IConfluenceConfiguration
    {
        string Username { get; }
        string APIKey { get; }
        string BaseUrl { get; }
    }
}
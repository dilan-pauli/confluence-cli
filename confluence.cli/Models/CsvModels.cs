namespace Confluence.Cli.Models
{
    public record Space(string key, string name, string status, string type, string link);
    public record Page(string id, string title, string status, DateTime created, DateTime lastUpdated, bool hasContent, string type, string link);
}
namespace Confluence.Cli.Models
{
    public record Space(string key, string name, string status, string type, string link);
    public record Content(string id, string title, string status, DateTime created, DateTime lastUpdated, bool hasContent, string type, string link);
    public record PageAnalytic(int id, string title, string parentTitle, DateTime created, DateTime lastUpdated, int numberOfComments, int viewers, int views);
}
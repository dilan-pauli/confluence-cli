using confluence.api;
using Confluence.Cli;
using Confluence.Cli.Commands;
using Confluence.Cli.Infrastructure;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

// Create a type registrar and register any dependencies.
// A type registrar is an adapter for a DI framework.
var registrations = new ServiceCollection();
registrations.AddHttpClient<IConfluenceClient, ConfluenceHttpClient>(
    (services, client) =>
    {
        var config = services.GetService<IConfluenceConfiguration>();
        if (config is not null)
        {
            client.BaseAddress = new Uri($"https://{config.BaseUrl}");
            client.SetBasicAuthentication(config.Username, config.APIKey);
        }
    }).AddPolicyHandler(Polices.RetryHonouringRetryAfter).AddPolicyHandler(Polices.RetryAfterError);
registrations.AddScoped<IConfluenceConfiguration, EnvConfiguration>();
var registrar = new TypeRegistrar(registrations);

var app = new CommandApp(registrar);
app.Configure(config =>
{
    config.AddCommand<SpacesCommand>("spaces").WithExample(new[] { "spaces", "--list" });
    config.AddCommand<ContentCommand>("content").WithExample(new[] { "content", @"""lastModified < '2020/01/01' and space = LWProduct and type = page""" });
    config.AddCommand<PageAnalyticsCommand>("analytics").WithExample(new[] { "analytics", "{spaceID}" }).WithExample(new[] { "analytics", "{spaceID}", "-d", "2022-05-18" });
    config.SetApplicationName("conflutil");
});
if (args.Length == 0 || args.Contains("-h") || args.Contains("--help"))
{
    AnsiConsole.Write(new FigletText("Confluence CLI").Centered().Color(Color.Purple));
    AnsiConsole.Markup("This CLI uses environment variables to get it's configuration. Before use please set the following environment variables.\r\n\r\n- CONFLUENCE_URL = 'your_domain.atlassian.net'\r\n- CONFLUENCE_USER = 'your_username'\r\n- CONFLUENCE_APIKEY = 'your_api_key'\r\n\r\nThe API can be generated going to the [link=https://id.atlassian.com/manage-profile/security] Manage your Account[/] area when you are logged into Confluence. Then click on the Security tab and then the [link=https://id.atlassian.com/manage-profile/security/api-tokens] Create and manage API token[/] link.\r\n\r\n");
}
await app.RunAsync(args);
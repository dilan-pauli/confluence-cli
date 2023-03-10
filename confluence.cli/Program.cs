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
    }).AddPolicyHandler(Polices.RetryHonouringRetryAfter);
registrations.AddScoped<IConfluenceConfiguration, EnvConfiguration>();
var registrar = new TypeRegistrar(registrations);

var app = new CommandApp(registrar);
app.Configure(config =>
{
    config.AddCommand<SpacesCommand>("spaces").WithExample(new[] { "spaces", "--list" });
    config.AddCommand<ContentCommand>("content").WithExample(new[] { "content", "{cql}" });
    config.SetApplicationName("Confluence CLI");
});
if (args.Length == 0)
{
    AnsiConsole.Write(new FigletText("Confluence CLI").Centered().Color(Color.Purple));
}
await app.RunAsync(args);
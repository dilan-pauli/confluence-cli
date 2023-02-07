using confluence.api;
using Confluence.Cli.Commands;
using Confluence.Cli.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Http;
using IdentityModel.Client;

// Create a type registrar and register any dependencies.
// A type registrar is an adapter for a DI framework.
var registrations = new ServiceCollection();
registrations.AddHttpClient<IConfluenceClient, ConfluenceRestSharpClient>(
    (services, client) =>
    {
        var config = services.GetService<IConfluenceConfiguration>();
        if (config is not null)
        {
            client.BaseAddress = new Uri(config.BaseUrl);
            client.SetBasicAuthentication(config.Username, config.APIKey);
        }
    });
registrations.AddScoped<IConfluenceConfiguration, EnvConfiguration>();
var registrar = new TypeRegistrar(registrations);

var app = new CommandApp(registrar);
app.Configure(config =>
{
    config.AddCommand<SpacesCommand>("spaces").WithExample(new[] { "spaces", "--list" });
    config.SetApplicationName("Confluence CLI");
});
if (args.Length == 0)
{
    AnsiConsole.Write(new FigletText("Confluence CLI").Centered().Color(Color.Purple));
}
await app.RunAsync(args);
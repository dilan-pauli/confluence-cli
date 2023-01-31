using confluence.api;
using confluence.cli.Commands;
using confluence.cli.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

// Create a type registrar and register any dependencies.
// A type registrar is an adapter for a DI framework.
var registrations = new ServiceCollection();
registrations.AddScoped<IConfluenceClient, ConfluenceRestSharpClient>();
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
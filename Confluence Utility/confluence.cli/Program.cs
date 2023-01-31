using confluence.api;
using confluence.cli.Commands;
using confluence.cli.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

// Create a type registrar and register any dependencies.
// A type registrar is an adapter for a DI framework.
var registrations = new ServiceCollection();
registrations.AddScoped<IConfluenceClient, ConfluenceRestSharpClient>();
registrations.AddScoped<IConfluenceConfiguration, EnvConfiguration>();
var registrar = new TypeRegistrar(registrations);

var app = new CommandApp<SpacesCommand>(registrar);
app.Configure(config =>
{
    config.SetApplicationName("Confluence CLI");
});

await app.RunAsync(args);
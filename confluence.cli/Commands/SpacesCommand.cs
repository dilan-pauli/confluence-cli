using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using confluence.api;
using System.Data;
using CsvHelper;
using System.Globalization;
using Confluence.Cli.Models;

namespace Confluence.Cli.Commands
{
    public sealed class SpacesCommand : AsyncCommand<SpacesCommand.Settings>
    {
        private readonly IAnsiConsole console;
        private readonly IConfluenceClient confluenceClient;
        private readonly IConfluenceConfiguration config;

        public sealed class Settings : CommandSettings
        {
            [CommandOption("-c|--csv")]
            [Description("Print output as CSV to the given file location.")]
            public string? CSV { get; set; }
        }

        public SpacesCommand(IAnsiConsole console, IConfluenceClient confluenceClient, IConfluenceConfiguration config)
        {
            this.console = console;
            this.confluenceClient = confluenceClient;
            this.config = config;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            try
            {
                var spaces = await this.confluenceClient.GetAllGlobalActiveSpaces();

                if (settings.CSV is not null)
                {
                    using (var writer = new StreamWriter(settings.CSV))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteHeader<Space>();
                        csv.NextRecord();
                        foreach (var space in spaces.OrderBy(x => x.name))
                        {
                            var output = new Space(space.key,
                                                   space.name,
                                                   space.status,
                                                   space.type,
                                                   space.GenerateFullWebURL(this.config.BaseUrl));
                            csv.WriteRecord(output);
                            csv.NextRecord();
                        }
                        console.WriteLine($"Wrote {spaces.Count} spaces to {settings.CSV}");
                    }
                }
                else
                {
                    var consoleTable = new Table();
                    consoleTable.AddColumn(new TableColumn(nameof(Space.key)));
                    consoleTable.AddColumn(new TableColumn(nameof(Space.name)));
                    consoleTable.AddColumn(new TableColumn(nameof(Space.status)));
                    consoleTable.AddColumn(new TableColumn(nameof(Space.type)));
                    consoleTable.AddColumn(new TableColumn("Link"));

                    foreach (var space in spaces.OrderBy(x => x.name))
                    {
                        consoleTable.AddRow(space.key, space.name, space.status, space.type, $"[link]{space.GenerateFullWebURL(this.config.BaseUrl)}[/]");
                    }
                    console.Write(consoleTable);
                    console.WriteLine($"Fetched {spaces.Count} spaces...");
                }
            }
            catch(Exception ex)
            {
                console.WriteException(ex);
            }

            return 0;
        }
    }
}

using System.ComponentModel;
using System.Globalization;
using confluence.api;
using Confluence.Api.Models;
using Confluence.Cli.Models;
using CsvHelper;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Confluence.Cli.Commands
{
    public sealed class ContentCommand : AsyncCommand<ContentCommand.Settings>
    {
        private readonly IAnsiConsole console;
        private readonly IConfluenceClient confluenceClient;
        private readonly IConfluenceConfiguration config;

        public sealed class Settings : CommandSettings
        {
            [CommandArgument(0, "<CQL>")]
            [Description("Confluence Query")]
            public string Query { get; set; }

            [CommandOption("-c|--csv")]
            [Description("Print output as CSV to the given file location.")]
            public string? CSV { get; set; }
        }

        public ContentCommand(IAnsiConsole console, IConfluenceClient confluenceClient, IConfluenceConfiguration config)
        {
            this.console = console;
            this.confluenceClient = confluenceClient;
            this.config = config;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            List<Api.Models.Content> contents = new List<Api.Models.Content>();
            try
            {
                await console.Status()
                    .AutoRefresh(true)
                    .Spinner(Spinner.Known.Star)
                    .SpinnerStyle(Style.Parse("green bold"))
                    .StartAsync("Fetching...", async ctx =>
                    {
                        contents = await this.confluenceClient.GetContentByCQL(settings.Query, (pages) =>
                        {
                            ctx.Status($"Fetching {pages} pages...");
                        });
                    });

                if (settings.CSV is not null)
                {
                    using (var writer = new StreamWriter(settings.CSV))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteHeader<Models.Content>();
                        csv.NextRecord();
                        foreach (var content in contents.OrderBy(x => x.history.createdDate))
                        {
                            var output = new Models.Content(content.id,
                                                  content.title,
                                                  content.status,
                                                  content.history.createdDate,
                                                  content.version.when,
                                                  content.HasContent(),
                                                  content.type,
                                                  content.GenerateFullWebURL(this.config.BaseUrl));
                            csv.WriteRecord(output);
                            csv.NextRecord();
                        }
                        console.WriteLine($"Wrote {contents.Count} results to {settings.CSV}");
                    }
                }
                else
                {
                    var consoleTable = new Table();
                    consoleTable.AddColumn(new TableColumn("ID"));
                    consoleTable.AddColumn(new TableColumn("Title"));
                    consoleTable.AddColumn(new TableColumn("Status"));
                    consoleTable.AddColumn(new TableColumn("Created Date"));
                    consoleTable.AddColumn(new TableColumn("Last Updated"));
                    consoleTable.AddColumn(new TableColumn("HasContent"));
                    consoleTable.AddColumn(new TableColumn("Type"));
                    consoleTable.AddColumn(new TableColumn("Link"));

                    foreach (var content in contents.OrderBy(x => x.history.createdDate))
                    {
                        consoleTable.AddRow(content.id,
                                            content.title,
                                            content.status,
                                            content.history.createdDate.ToShortDateString(),
                                            content.version.when.ToShortDateString(),
                                            content.HasContent().ToString(),
                                            content.type,
                                            $"[link]{content.GenerateFullWebURL(this.config.BaseUrl)}[/]");
                    }
                    console.Write(consoleTable);
                    console.WriteLine($"Fetched {contents.Count} results...");
                }
            }
            catch (Exception ex)
            {
                console.WriteException(ex);
                console.WriteLine($"double check provided input query: {settings.Query}");
            }

            return 0;
        }
    }
}
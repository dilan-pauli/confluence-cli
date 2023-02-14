using System.ComponentModel;
using confluence.api;
using Confluence.Api.Models;
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
            [Description("Print output as CSV")]
            public bool CSV { get; set; }
        }

        public ContentCommand(IAnsiConsole console, IConfluenceClient confluenceClient, IConfluenceConfiguration config)
        {
            this.console = console;
            this.confluenceClient = confluenceClient;
            this.config = config;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            List<Content> pages = new List<Content>();
            try
            {
                await console.Status()
                    .AutoRefresh(true)
                    .Spinner(Spinner.Known.Star)
                    .SpinnerStyle(Style.Parse("green bold"))
                    .StartAsync("Fetching...", async ctx =>
                    {
                        pages = await this.confluenceClient.GetPagesByCQL(settings.Query, (pages) =>
                        {
                            ctx.Status($"Fetching {pages} pages...");
                        });
                    });

                if (settings.CSV)
                {
                    // TODO write a file for CSV to prevent the STD output capture of the status text.
                    console.WriteLine($"ID,Title,Status,CreatedDate,LastUpdated,HasContent,Type,Link");
                    foreach (var page in pages.OrderBy(x => x.history.createdDate))
                    {
                        string hasContent = "FALSE";
                        if (page.type != "attachment")
                        {
                            hasContent = page.body.storage.value.Length > 100 ? "TRUE" : "FALSE";
                        }
                        console.WriteLine($"{page.id},{page.title},{page.status},{page.history.createdDate},{page.version.when},{hasContent},{page.type},{page.GenerateFullWebURL(this.config.BaseUrl)}");
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

                    foreach (var page in pages.OrderBy(x => x.history.createdDate))
                    {
                        string hasContent = "FALSE";
                        if (page.type != "attachment")
                        {
                            hasContent = page.body.storage.value.Length > 100 ? "TRUE" : "FALSE";
                        }
                        consoleTable.AddRow(page.id, page.title, page.status, page.history.createdDate.ToString(), page.version.when.ToString(), hasContent, page.type, $"[link]{page.GenerateFullWebURL(this.config.BaseUrl)}[/]");
                    }
                    console.Write(consoleTable);
                }
                console.WriteLine($"{pages.Count} results");
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
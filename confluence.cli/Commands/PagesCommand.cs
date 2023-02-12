using System.ComponentModel;
using confluence.api;
using Confluence.Api.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Confluence.Cli.Commands
{
    public sealed class PagesCommand : AsyncCommand<PagesCommand.Settings>
    {
        private readonly IAnsiConsole console;
        private readonly IConfluenceClient confluenceClient;

        public sealed class Settings : CommandSettings
        {
            [CommandOption("-q|--cql")]
            [Description("Conflueence Query")]
            public string Query { get; set; }

            [CommandOption("-c|--csv")]
            [Description("Print output as CSV")]
            public bool CSV { get; set; }
        }

        public PagesCommand(IAnsiConsole console, IConfluenceClient confluenceClient)
        {
            this.console = console;
            this.confluenceClient = confluenceClient;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            List<Content> pages = new List<Content>();
            try
            {
                await console.Status()
                    .AutoRefresh(false)
                    .Spinner(Spinner.Known.Star)
                    .SpinnerStyle(Style.Parse("green bold"))
                    .StartAsync("Fetching..", async ctx =>
                    {
                        pages = await this.confluenceClient.GetPagesByCQL(settings.Query, (pages) =>
                        {
                            ctx.Refresh();
                            console.WriteLine($"received {pages} pages...");
                        });
                    });

                if (settings.CSV)
                {
                    console.WriteLine($"ID,Title,Status,CreatedDate,LastUpdated,Views,HasContent,Link");
                    foreach (var page in pages.OrderBy(x => x.version.when))
                    {
                        var hasContent = page.body.storage.value.Length > 0 ? "TRUE" : "FALSE";
                        console.WriteLine($"{page.id},{page.title},{page.status},{page.history.createdDate},{page.version.when},X,{hasContent}, {page._links.self}");
                    }
                }
                else
                {
                    // Currenly too much data for a specter table
                    var consoleTable = new Table();
                    consoleTable.AddColumn(new TableColumn("ID"));
                    consoleTable.AddColumn(new TableColumn("Title"));
                    consoleTable.AddColumn(new TableColumn("Status"));
                    consoleTable.AddColumn(new TableColumn("Created Date"));
                    consoleTable.AddColumn(new TableColumn("Last Updated"));
                    consoleTable.AddColumn(new TableColumn("Views"));
                    consoleTable.AddColumn(new TableColumn("HasContent"));
                    consoleTable.AddColumn(new TableColumn("Link"));

                    foreach (var page in pages.OrderBy(x => x.version.when))
                    {
                        var hasContent = page.body.storage.value.Length > 0 ? "TRUE" : "FALSE";
                        consoleTable.AddRow(page.id, page.title, page.status, page.history.createdDate.ToString(), page.version.when.ToString(), "X", hasContent, $"[link]{page._links.self}[/]");
                    }
                    console.Write(consoleTable);
                }
                console.WriteLine($"{pages.Count} results");
            }
            catch (Exception ex)
            {
                console.WriteException(ex);
            }

            return 0;
        }
    }
}
using System.ComponentModel;
using confluence.api;
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
            [CommandOption("-s|--space")]
            [Description("Key of the space")]
            public string SpaceKey { get; set; }

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
            try
            {
                var pages = await this.confluenceClient.GetAllPagesForSpace(settings.SpaceKey);

                if (settings.CSV)
                {
                    console.WriteLine($"ID,Title,Type,CreatedDate,LastUpdated,Views,HasContent,Link");
                    foreach (var page in pages.OrderBy(x => x.version.when))
                    {
                        var hasContent = page.body.storage.value.Length > 0 ? "TRUE" : "FALSE";
                        console.WriteLine($"{page.id},{page.title},{page.history.createdDate},{page.version.when},X,{hasContent}, {page._links.self}");
                    }
                }
                else
                {
                    var consoleTable = new Table();
                    consoleTable.AddColumn(new TableColumn("ID"));
                    consoleTable.AddColumn(new TableColumn("Title"));
                    consoleTable.AddColumn(new TableColumn("Type"));
                    consoleTable.AddColumn(new TableColumn("Created Date"));
                    consoleTable.AddColumn(new TableColumn("Last Updated"));
                    consoleTable.AddColumn(new TableColumn("Views"));
                    consoleTable.AddColumn(new TableColumn("HasContent"));
                    consoleTable.AddColumn(new TableColumn("Link"));

                    foreach (var page in pages.OrderBy(x => x.version.when))
                    {
                        var hasContent = page.body.storage.value.Length > 0 ? "TRUE" : "FALSE";
                        consoleTable.AddRow(page.id,page.title, page.type, page.history.createdDate.ToString(), page.version.when.ToString(), "X", hasContent, $"[link]{page._links.self}[/]");
                    }
                    console.Write(consoleTable);
                }
            }
            catch (Exception ex)
            {
                console.WriteException(ex);
            }

            return 0;
        }
    }
}
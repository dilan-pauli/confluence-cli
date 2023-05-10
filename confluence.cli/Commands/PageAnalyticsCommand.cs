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
    public sealed class PageAnalyticsCommand : AsyncCommand<PageAnalyticsCommand.Settings>
    {
        private readonly IAnsiConsole console;
        private readonly IConfluenceClient confluenceClient;
        private readonly IConfluenceConfiguration config;

        public sealed class Settings : CommandSettings
        {
            [CommandArgument(0, "<SPACEID>")]
            [Description("Space ID")]
            public int SpaceId { get; set; }

            [CommandOption("-c|--csv")]
            [Description("Print output as CSV to the given file location.")]
            public string? CSV { get; set; }
        }

        public PageAnalyticsCommand(IAnsiConsole console, IConfluenceClient confluenceClient, IConfluenceConfiguration config)
        {
            this.console = console;
            this.confluenceClient = confluenceClient;
            this.config = config;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            IDictionary<int, Page> contents = new Dictionary<int, Page>();
            try
            {
                await console.Status()
                    .AutoRefresh(true)
                    .Spinner(Spinner.Known.Star)
                    .SpinnerStyle(Style.Parse("green bold"))
                    .StartAsync("Fetching...", async ctx =>
                    {
                        contents = await this.confluenceClient.GetCurrentPagesInSpace(settings.SpaceId, (pages) =>
                        {
                            ctx.Status($"Fetching {pages} pages...");
                        });
                    });

                if (settings.CSV is not null)
                {
                    using (var writer = new StreamWriter(settings.CSV))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteHeader<PageAnalytic>();
                        csv.NextRecord();
                        foreach (var content in contents.OrderBy(x => x.Value.createdAt).ToList())
                        {
                            PageAnalytic output = await ConvertToAnalytic(content.Value, contents[content.Value.parentId].title);
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
                    consoleTable.AddColumn(new TableColumn("Parent Title"));
                    consoleTable.AddColumn(new TableColumn("Created Date"));
                    consoleTable.AddColumn(new TableColumn("Last Updated"));
                    consoleTable.AddColumn(new TableColumn("Comment Count"));
                    consoleTable.AddColumn(new TableColumn("Users Viewed"));
                    consoleTable.AddColumn(new TableColumn("Views"));

                    foreach (var content in contents.OrderBy(x => x.Value.createdAt).ToList())
                    {
                        PageAnalytic output = await ConvertToAnalytic(content.Value, contents[content.Value.parentId].title);
                        consoleTable.AddRow(output.id.ToString(),
                                                  output.title,
                                                  output.parentTitle,
                                                  output.created.ToString(),
                                                  output.lastUpdated.ToString(),
                                                  output.numberOfComments.ToString(),
                                                  output.viewers.ToString(),
                                                  output.views.ToString());
                    }
                    console.Write(consoleTable);
                    console.WriteLine($"Fetched {contents.Count} results...");
                }
            }
            catch (Exception ex)
            {
                console.WriteException(ex);
            }

            return 0;
        }

        private async Task<PageAnalytic> ConvertToAnalytic(Page content, string parentTitle)
        {
            var inlineComments = await this.confluenceClient.GetInlineCommentsOnPage(content.id);
            var footerComments = await this.confluenceClient.GetFooterCommentsOnPage(content.id);
            var views = await this.confluenceClient.GetViewsOfPage(content.id);
            var viewers = await this.confluenceClient.GetViewersOfPage(content.id);

            var output = new PageAnalytic(content.id,
                                  content.title,
                                  parentTitle,
                                  content.createdAt,
                                  content.version.createdAt,
                                  inlineComments.Count() + footerComments.Count(),
                                  viewers,
                                  views);
            return output;
        }
    }
}
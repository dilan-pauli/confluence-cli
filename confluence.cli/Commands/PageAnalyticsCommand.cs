using System.Collections.Concurrent;
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

            [CommandOption("-d|--fromDate")]
            [Description("Date from which the views are retrieved.")]
            public DateTime? fromDate { get; set; }

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
            IDictionary<string, Page> contents = new Dictionary<string, Page>();
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
                    using (var writer = new StreamWriter(Path.GetFullPath(settings.CSV)))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        var results = new ConcurrentQueue<PageAnalytic>();
                        csv.WriteHeader<PageAnalytic>();
                        csv.NextRecord();
                        await ReteriveExtraData(contents, results.Enqueue, settings.fromDate);
                        while(results.TryDequeue(out var output))
                        {
                            csv.WriteRecord(output);
                            csv.NextRecord();
                        }
                        console.WriteLine($"Wrote {contents.Count} results to ${Path.GetFullPath(settings.CSV)}...");
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
                    await ReteriveExtraData(contents, (PageAnalytic output) =>
                    {
                        consoleTable.AddRow(Markup.Escape(output.id.ToString()),
                                            Markup.Escape(output.title),
                                            Markup.Escape(output.parentTitle),
                                            Markup.Escape(output.created.ToString()),
                                            Markup.Escape(output.lastUpdated.ToString()),
                                            Markup.Escape(output.numberOfComments.ToString()),
                                            Markup.Escape(output.viewers.ToString()),
                                            Markup.Escape(output.views.ToString()));
                    }, settings.fromDate);
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

        /// <summary>
        /// Retrieve the extra data associated with the pages for the analytics
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="onPageFetched"> Called in parallel</param>
        /// <param name="fromDate"></param>
        /// <returns></returns>
        private async Task ReteriveExtraData(IDictionary<string, Page> contents, Action<PageAnalytic> onPageFetched, DateTime? fromDate = null)
        {
            await console.Status()
                .AutoRefresh(true)
                .Spinner(Spinner.Known.Aesthetic)
                .SpinnerStyle(Style.Parse("green bold"))
                .StartAsync("Fetching comments and views..", async ctx =>
                {
                    var count = 0;
                    var pages = new ConcurrentQueue<PageAnalytic>();
                    var orderedContents = contents.Values.ToList();
                    ParallelOptions parallelOptions = new()
                    {
                        MaxDegreeOfParallelism = 32
                    };

                    await Parallel.ForEachAsync(orderedContents, parallelOptions, async (Page content, CancellationToken token) =>
                    {
                        var parentTitle = "none";
                        if (!string.IsNullOrEmpty(content.parentId))
                            parentTitle = contents[content.parentId].title;
                        var output = await ConvertToAnalytic(content, parentTitle, fromDate);
                        onPageFetched(output);
                        var currentCount = Interlocked.Increment(ref count);
                        if (currentCount % 8 == 0)
                            ctx.Status($"Fetching comments and views {count}/{contents.Count} pages...");
                    });
                });
        }

        private async Task<PageAnalytic> ConvertToAnalytic(Page content, string parentTitle, DateTime? fromDate = null)
        {
            var inlineComments = await this.confluenceClient.GetInlineCommentsOnPage(content.id);
            var footerComments = await this.confluenceClient.GetFooterCommentsOnPage(content.id);
            var views = await this.confluenceClient.GetViewsOfPage(content.id, fromDate);
            var viewers = await this.confluenceClient.GetViewersOfPage(content.id, fromDate);

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
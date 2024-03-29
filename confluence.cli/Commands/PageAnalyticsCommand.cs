﻿using System.Collections.Concurrent;
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
    [Description("Returns all the pages in the given space with the following headers: id, title, parentTitle, createdDate, updatedDate, # of comments, viewers, views. This command could take some time to execute if the result list from the query is large. It will display progress while fetching.")]
    public sealed class PageAnalyticsCommand : AsyncCommand<PageAnalyticsCommand.Settings>
    {
        private readonly IAnsiConsole console;
        private readonly IConfluenceClient confluenceClient;
        private readonly IConfluenceConfiguration config;

        public sealed class Settings : CommandSettings
        {
            [CommandArgument(0, "[SpaceId]")]
            [Description("Space ID")]
            public uint? SpaceId { get; set; }

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
            try
            {
                if (settings.SpaceId is not null)
                {
                    var contents = await GetPagesForSpace(settings.SpaceId.Value);

                    if (settings.CSV is not null)
                    {
                        // If a file name is provided use that, else generate a filename with the space id (or name) and date.
                        await OutputPageDataToCSV(contents, Path.GetDirectoryName(settings.CSV) ?? settings.CSV, Path.GetFileName(settings.CSV), settings.fromDate);
                    }
                    else
                    {
                        await OutputPageDataToConsole(contents, settings.fromDate);
                    }
                }
                else
                {
                    var spaces = await confluenceClient.GetAllGlobalActiveSpaces();
                    console.WriteLine($"Iterating over {spaces.Count} spaces this could take awhile...");

                    foreach (var space in spaces)
                    {
                        console.WriteLine($"Starting output for {space.name}...");

                        var contents = await GetPagesForSpace(space.id, space.name);

                        if (settings.CSV is not null)
                        {
                            if (Directory.Exists(settings.CSV))
                                throw new Exception("Invalid Path, directory does not exist please provide another.");
                            var fileName = space.name + DateTime.Now.ToString("s").Replace(":", "") + ".csv";
                            // If a file name is provided use that, else generate a filename with the space id (or name) and date.
                            await OutputPageDataToCSV(contents, settings.CSV, fileName, settings.fromDate);
                        }
                        else
                        {
                            await OutputPageDataToConsole(contents, settings.fromDate);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                console.WriteException(ex);
            }

            return 0;
        }

        private async Task OutputPageDataToCSV(IDictionary<string, Page> contents, string csvFolderPath, string csvFileName, DateTime? viewsFromDate)
        {
            var csvPath = Path.Combine(csvFolderPath.Trim('"'), csvFileName).Replace(' ', '_');
            using (var writer = new StreamWriter(csvPath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                var results = new ConcurrentQueue<PageAnalytic>();
                csv.WriteHeader<PageAnalytic>();
                csv.NextRecord();
                await ReteriveExtraData(contents, results.Enqueue, viewsFromDate);
                while (results.TryDequeue(out var output))
                {
                    csv.WriteRecord(output);
                    csv.NextRecord();
                }
                console.WriteLine($"Wrote {contents.Count} results to ${Path.GetFullPath(csvPath)}...");
            }
        }

        public async Task OutputPageDataToConsole(IDictionary<string, Page> contents, DateTime? viewsFromDate)
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
            }, viewsFromDate);
            console.Write(consoleTable);
            console.WriteLine($"Fetched {contents.Count} results...");
        }

        private async Task<IDictionary<string, Page>> GetPagesForSpace(long spaceId, string spaceName = "")
        {
            IDictionary<string, Page> contents = new Dictionary<string, Page>();
            await console.Status()
                .AutoRefresh(true)
                .Spinner(Spinner.Known.Star)
                .SpinnerStyle(Style.Parse("green bold"))
                .StartAsync("Fetching...", async ctx =>
                {
                    contents = await this.confluenceClient.GetCurrentPagesInSpace(spaceId, (pages) =>
                    {
                        ctx.Status($"Fetching {pages} pages in space {spaceName}...");
                    });
                });
            return contents;
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
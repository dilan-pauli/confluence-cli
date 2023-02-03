using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluence.Api.Models;
using confluence.api;
using Spectre.Console.Cli;
using Spectre.Console;

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
                var spaces = await this.confluenceClient.GetAllGlobalActiveSpaces();

                if (settings.CSV)
                {
                    //console.WriteLine($"{nameof(Space.key)},{nameof(Space.key)},{nameof(Space.key)},{nameof(Space.key)},Link");
                    foreach (var space in spaces.OrderBy(x => x.name))
                    {
                        //console.WriteLine($"{space.key},{space.name},{space.status},{space.type},{space._links.self}");
                    }
                }
                else
                {
                    var consoleTable = new Table();
                    //consoleTable.AddColumn(new TableColumn(nameof(Space.key)));
                    //consoleTable.AddColumn(new TableColumn(nameof(Space.name)));
                    //consoleTable.AddColumn(new TableColumn(nameof(Space.status)));
                    //consoleTable.AddColumn(new TableColumn(nameof(Space.type)));
                    //consoleTable.AddColumn(new TableColumn("Link"));

                    foreach (var space in spaces.OrderBy(x => x.name))
                    {
                        //consoleTable.AddRow(space.key, space.name, space.status, space.type, $"[link]{space._links.self}[/]");
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

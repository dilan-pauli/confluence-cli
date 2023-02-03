using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;
using Spectre.IO;
using confluence.api;
using System.Data;
using Spectre.Console.Extensions.Table;
using Confluence.Api.Models;

namespace Confluence.Cli.Commands
{
    public sealed class SpacesCommand : AsyncCommand<SpacesCommand.Settings>
    {
        private readonly IAnsiConsole console;
        private readonly IConfluenceClient confluenceClient;

        public sealed class Settings : CommandSettings
        {
            [CommandOption("-c|--csv")]
            [Description("Print output as CSV")]
            public bool CSV { get; set; }
        }

        public SpacesCommand(IAnsiConsole console, IConfluenceClient confluenceClient)
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
                    console.WriteLine($"Key,Name,Status,Type,Link");
                    foreach (var space in spaces.OrderBy(x => x.name))
                    {
                        console.WriteLine($"{space.key},{space.name},{space.status},{space.type},{space._links.self}");
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
                        consoleTable.AddRow(space.key, space.name, space.status, space.type, $"[link]{space._links.self}[/]");
                    }
                    console.Write(consoleTable);
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

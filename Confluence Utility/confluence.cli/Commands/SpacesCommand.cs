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

namespace confluence.cli.Commands
{
    public sealed class SpacesCommand : AsyncCommand<SpacesCommand.Settings>
    {
        private readonly IAnsiConsole console;
        private readonly IConfluenceClient confluenceClient;

        public sealed class Settings : CommandSettings
        {
            [CommandOption("-l|--list")]
            [Description("Lists all global spaces")]
            public bool List { get; set; }

            // TODO Add CSV output options
        }

        public SpacesCommand(IAnsiConsole console, IConfluenceClient confluenceClient)
        {
            this.console = console;
            this.confluenceClient = confluenceClient;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            // Ask client for spaces

            // format 

            // output to console
            return 0;
        }
    }
}

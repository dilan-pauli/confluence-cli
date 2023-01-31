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
using confluence.api.Models;

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
            var spaces = await this.confluenceClient.GetAllGlobalActiveSpaces();

            // format 

            var dataset = new DataSet("Spaces");
            var datatable = DataExtensions.CreateDataTable<Space>();
            datatable.TableName = "Spaces";
            DataExtensions.FillDataTable(datatable, spaces);
            dataset.Tables.Add(datatable);
            var dataSetToDisplay = dataset.FromDataSet(opt => opt.BorderColor(Color.Aqua));
            console.Write(dataSetToDisplay);


            // output to console
            return 0;
        }
    }
}

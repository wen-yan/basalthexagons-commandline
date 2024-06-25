using System;
using System.Threading.Tasks;
using Basalt.CommandLine;
using Basalt.CommandLine.Annotations;

namespace SimpleUsage.Commands;

#nullable disable
partial class FsLsCommandOptions
{
    public ConsoleColor Color { get; init; }

    [CliCommandSymbol(CliCommandSymbolType.Argument)]
    public string Directory { get; init; }
}
#nullable restore

[CliCommandBuilder("ls", typeof(FsCliCommandBuilder))]
partial class FsLsCliCommandBuilder : CliCommandBuilder<FsLsCommand, FsLsCommandOptions>
{
    public FsLsCliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "ls command";
        this.ColorOption = new(["--color", "-c"], () => ConsoleColor.Yellow, "command output color");
        this.DirectoryArgument = new("directory", "target directory of ls command");
    }
}

class FsLsCommand : Command<FsLsCommandOptions>
{
    public FsLsCommand(CommandContext commandContext, IDemoService demoService) : base(commandContext)
    {
        this.DemoService = demoService;
    }

    private IDemoService DemoService { get; }

    public override async ValueTask Execute()
    {
        ConsoleColor currentColor = Console.ForegroundColor;
        Console.ForegroundColor = this.Options.Color;
        try
        {
            await this.DemoService.WriteLine($"FsLsCommand output, endpoint: {this.Options.Endpoint}, target: {this.Options.Directory}");
        }
        finally
        {
            Console.ForegroundColor = currentColor;
        }
    }
}
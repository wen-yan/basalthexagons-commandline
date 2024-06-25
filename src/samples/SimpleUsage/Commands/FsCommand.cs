using System;
using System.Threading.Tasks;
using Basalt.CommandLine;
using Basalt.CommandLine.Annotations;

namespace SimpleUsage.Commands;

#nullable disable
partial class FsCommandOptions
{
    [CliCommandSymbol(CliCommandSymbolType.GlobalOption)]
    public string Endpoint { get; init; }
}
#nullable restore

[CliCommandBuilder("fs", typeof(AppCliCommandBuilder))]
partial class FsCliCommandBuilder : CliCommandBuilder<FsCommand, FsCommandOptions>
{
    public FsCliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "fs command";
        this.EndpointOption = new(["--endpoint"], "endpoint of the file system") { IsRequired = true };
    }
}

class FsCommand : Command<FsCommandOptions>
{
    public FsCommand(CommandContext commandContext) : base(commandContext)
    {
    }

    public override async ValueTask Execute()
    {
        await Console.Out.WriteLineAsync($"FsCommand output, endpoint: {this.Options.Endpoint}");
    }
}
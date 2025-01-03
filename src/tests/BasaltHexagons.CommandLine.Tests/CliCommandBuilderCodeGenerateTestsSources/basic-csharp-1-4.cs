using System;
using System.Threading.Tasks;
using BasaltHexagons.CommandLine;
using BasaltHexagons.CommandLine.Annotations;

namespace MyNamespace
{
    class FsCommandOptions
    {
        [CliCommandSymbol]
        public string Option1 { get; set; }
    }
    
    [CliCommandBuilder("fs", null)]
    class FsCommandBuilder : CliCommandBuilder//<FsCommand, FsCommandOptions>
    {
        public FsCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider) {}
    }
    class FsCommand : Command//<FsCommandOptions>
    {
        public FsCommand(CommandContext commandContext) : base(commandContext) {}
        public override ValueTask ExecuteAsync() { return ValueTask.CompletedTask; }
    }
}
using System;
using System.IO;
using System.Threading.Tasks;
using BasaltHexagons.CommandLine;
using BasaltHexagons.CommandLine.Annotations;

namespace MyNamespace
{
    partial class FsCommandOptions
    {
        [CliCommandSymbol]
        public int? Option1 { get; }
        
        [CliCommandSymbol(CliCommandSymbolType.Option)]
        public string? Option2 { get; }
        
        [CliCommandSymbol]
        public FileInfo? Option3 { get; }
    }
    
    [CliCommandBuilder("fs", null)]
    partial class FsCommandBuilder : CliCommandBuilder<FsCommand, FsCommandOptions>
    {
        public FsCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider) {}
    }
    class FsCommand : Command<FsCommandOptions>
    {
        public FsCommand(CommandContext commandContext) : base(commandContext) {}
        public override ValueTask ExecuteAsync() { return ValueTask.CompletedTask; }
    }
}
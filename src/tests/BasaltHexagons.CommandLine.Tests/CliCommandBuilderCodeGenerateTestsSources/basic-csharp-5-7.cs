using System;
using System.Threading.Tasks;
using BasaltHexagons.CommandLine;
using BasaltHexagons.CommandLine.Annotations;

namespace MyNamespace
{
    partial class FsCommandOptions
    {
        [CliCommandSymbol]
        public string Option1 { get; private set; }
        
        [CliCommandSymbol(CliCommandSymbolType.Option)]
        public string Option2 { get; private set; }
    
        [CliCommandSymbol(CliCommandSymbolType.GlobalOption)]
        public string GlobalOption1 { get; private set; }
        
        [CliCommandSymbol(CliCommandSymbolType.Argument)]
        public string Argument1 { get; private set; }
        
        public string ExcludedProperty { get { return this.GlobalOption1 + this.Argument1; } }
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
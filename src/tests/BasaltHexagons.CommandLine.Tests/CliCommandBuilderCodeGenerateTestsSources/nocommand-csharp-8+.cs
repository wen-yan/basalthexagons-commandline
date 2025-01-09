using System;
using System.Threading.Tasks;
using BasaltHexagons.CommandLine;
using BasaltHexagons.CommandLine.Annotations;

namespace MyNamespace
{
    partial class FsCommandOptions
    {
        [CliCommandSymbol]
        public string Option1 { get; }
        
        [CliCommandSymbol(CliCommandSymbolType.Option)]
        public string Option2 { get; }
    
        [CliCommandSymbol(CliCommandSymbolType.GlobalOption)]
        public string GlobalOption1 { get; }
        
        [CliCommandSymbol(CliCommandSymbolType.Argument)]
        public string Argument1 { get; }
        
        public string ExcludedProperty { get { return this.GlobalOption1 + this.Argument1; } }
    }
    
    [CliCommandBuilder("fs", null)]
    partial class FsCommandBuilder : CliCommandBuilder<FsCommandOptions>
    {
        public FsCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider) {}
    }
}
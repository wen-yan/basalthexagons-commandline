using System;
using System.IO;
using System.Threading.Tasks;
using BasaltHexagons.CommandLine;
using BasaltHexagons.CommandLine.Annotations;

namespace MyNamespace
{
    // level 1
    partial class Level1CommandOptions
    {
        [CliCommandSymbol(CliCommandSymbolType.GlobalOption)]
        public int? OptionLevel1A { get; }
        
        [CliCommandSymbol(CliCommandSymbolType.GlobalOption)]
        public string? OptionLevel1B { get; }
        
        [CliCommandSymbol(CliCommandSymbolType.GlobalOption)]
        public FileInfo? OptionLevel1C { get; }
        
        [CliCommandSymbol]
        public int OptionLevel1D { get; }
    }
    
    [CliCommandBuilder("fs", null)]
    partial class Level1CommandBuilder : CliCommandBuilder<Level1Command, Level1CommandOptions>
    {
        public Level1CommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider) {}
    }
    class Level1Command : Command<Level1CommandOptions>
    {
        public Level1Command(CommandContext commandContext) : base(commandContext) {}
        public override ValueTask ExecuteAsync() { return ValueTask.CompletedTask; }
    }
    
    // level 2
    partial class Level2CommandOptions
    {
        [CliCommandSymbol(CliCommandSymbolType.GlobalOption)]
        public int? OptionLevel2A { get; }
        
        [CliCommandSymbol(CliCommandSymbolType.GlobalOption)]
        public string? OptionLevel2B { get; }
        
        [CliCommandSymbol(CliCommandSymbolType.GlobalOption)]
        public FileInfo? OptionLevel2C { get; }
        
        [CliCommandSymbol]
        public int OptionLevel2D { get; }
    }
    
    [CliCommandBuilder("fs", typeof(Level1CommandBuilder))]
    partial class Level2CommandBuilder : CliCommandBuilder<Level2Command, Level2CommandOptions>
    {
        public Level2CommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider) {}
    }
    class Level2Command : Command<Level2CommandOptions>
    {
        public Level2Command(CommandContext commandContext) : base(commandContext) {}
        public override ValueTask ExecuteAsync() { return ValueTask.CompletedTask; }
    }
    
    // level 3
    partial class Level3CommandOptions
    {
        [CliCommandSymbol(CliCommandSymbolType.GlobalOption)]
        public int? OptionLevel3A { get; }
        
        [CliCommandSymbol(CliCommandSymbolType.GlobalOption)]
        public string? OptionLevel3B { get; }
        
        [CliCommandSymbol(CliCommandSymbolType.GlobalOption)]
        public FileInfo? OptionLevel3C { get; }
        
        [CliCommandSymbol]
        public int OptionLevel3D { get; }
    }
    
    [CliCommandBuilder("fs", typeof(Level2CommandBuilder))]
    partial class Level3CommandBuilder : CliCommandBuilder<Level3Command, Level3CommandOptions>
    {
        public Level3CommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider) {}
    }
    class Level3Command : Command<Level3CommandOptions>
    {
        public Level3Command(CommandContext commandContext) : base(commandContext) {}
        public override ValueTask ExecuteAsync() { return ValueTask.CompletedTask; }
    }
}
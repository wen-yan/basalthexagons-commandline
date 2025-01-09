namespace MyNamespace
{
    partial class Level3CommandOptions
    {
        public Level3CommandOptions(int? OptionLevel3A, string? OptionLevel3B, System.IO.FileInfo? OptionLevel3C, int OptionLevel3D, int? OptionLevel2A, string? OptionLevel2B, System.IO.FileInfo? OptionLevel2C, int? OptionLevel1A, string? OptionLevel1B, System.IO.FileInfo? OptionLevel1C)
        {
            this.OptionLevel3A = OptionLevel3A;
            this.OptionLevel3B = OptionLevel3B;
            this.OptionLevel3C = OptionLevel3C;
            this.OptionLevel3D = OptionLevel3D;
            this.OptionLevel2A = OptionLevel2A;
            this.OptionLevel2B = OptionLevel2B;
            this.OptionLevel2C = OptionLevel2C;
            this.OptionLevel1A = OptionLevel1A;
            this.OptionLevel1B = OptionLevel1B;
            this.OptionLevel1C = OptionLevel1C;
        }

        // from MyNamespace.Level2CommandBuilder
        [global::BasaltHexagons.CommandLine.Annotations.CliCommandSymbol(global::BasaltHexagons.CommandLine.Annotations.CliCommandSymbolType.FromGlobalOption)]
        public int? OptionLevel2A { get; }

        // from MyNamespace.Level2CommandBuilder
        [global::BasaltHexagons.CommandLine.Annotations.CliCommandSymbol(global::BasaltHexagons.CommandLine.Annotations.CliCommandSymbolType.FromGlobalOption)]
        public string? OptionLevel2B { get; }

        // from MyNamespace.Level2CommandBuilder
        [global::BasaltHexagons.CommandLine.Annotations.CliCommandSymbol(global::BasaltHexagons.CommandLine.Annotations.CliCommandSymbolType.FromGlobalOption)]
        public System.IO.FileInfo? OptionLevel2C { get; }

        // from MyNamespace.Level1CommandBuilder
        [global::BasaltHexagons.CommandLine.Annotations.CliCommandSymbol(global::BasaltHexagons.CommandLine.Annotations.CliCommandSymbolType.FromGlobalOption)]
        public int? OptionLevel1A { get; }

        // from MyNamespace.Level1CommandBuilder
        [global::BasaltHexagons.CommandLine.Annotations.CliCommandSymbol(global::BasaltHexagons.CommandLine.Annotations.CliCommandSymbolType.FromGlobalOption)]
        public string? OptionLevel1B { get; }

        // from MyNamespace.Level1CommandBuilder
        [global::BasaltHexagons.CommandLine.Annotations.CliCommandSymbol(global::BasaltHexagons.CommandLine.Annotations.CliCommandSymbolType.FromGlobalOption)]
        public System.IO.FileInfo? OptionLevel1C { get; }
    }
}

namespace MyNamespace
{
    using System.CommandLine;
    using Microsoft.Extensions.DependencyInjection;

    partial class Level3CommandBuilder
    {
        protected string Description { get; }

        internal global::System.CommandLine.Option<int?> OptionLevel3AOption { get; }

        internal global::System.CommandLine.Option<string?> OptionLevel3BOption { get; }

        internal global::System.CommandLine.Option<System.IO.FileInfo?> OptionLevel3COption { get; }

        private global::System.CommandLine.Option<int> OptionLevel3DOption { get; }

        protected override global::System.CommandLine.Command BuildCliCommandCore()
        {
            string name = this.GetCliCommandBuilderAttribute().Name;
            global::System.CommandLine.Command cliCommand = this.CreateCliCommand(name, this.Description);
            cliCommand.AddGlobalOption(OptionLevel3AOption);
            cliCommand.AddGlobalOption(OptionLevel3BOption);
            cliCommand.AddGlobalOption(OptionLevel3COption);
            cliCommand.AddOption(OptionLevel3DOption);
            cliCommand.SetHandler(async (global::System.CommandLine.Invocation.InvocationContext invocationContext) =>
            {
                global::System.CommandLine.Parsing.ParseResult parseResult = invocationContext.BindingContext.ParseResult;
                global::MyNamespace.Level3CommandOptions options = new global::MyNamespace.Level3CommandOptions(OptionLevel2A: parseResult.GetValueForOption(this.GetRequiredParentBuilder<global::MyNamespace.Level2CommandBuilder>().OptionLevel2AOption), OptionLevel2B: parseResult.GetValueForOption(this.GetRequiredParentBuilder<global::MyNamespace.Level2CommandBuilder>().OptionLevel2BOption), OptionLevel2C: parseResult.GetValueForOption(this.GetRequiredParentBuilder<global::MyNamespace.Level2CommandBuilder>().OptionLevel2COption), OptionLevel1A: parseResult.GetValueForOption(this.GetRequiredParentBuilder<global::MyNamespace.Level1CommandBuilder>().OptionLevel1AOption), OptionLevel1B: parseResult.GetValueForOption(this.GetRequiredParentBuilder<global::MyNamespace.Level1CommandBuilder>().OptionLevel1BOption), OptionLevel1C: parseResult.GetValueForOption(this.GetRequiredParentBuilder<global::MyNamespace.Level1CommandBuilder>().OptionLevel1COption), OptionLevel3A: parseResult.GetValueForOption(this.OptionLevel3AOption), OptionLevel3B: parseResult.GetValueForOption(this.OptionLevel3BOption), OptionLevel3C: parseResult.GetValueForOption(this.OptionLevel3COption), OptionLevel3D: parseResult.GetValueForOption(this.OptionLevel3DOption));
                await using (global::Microsoft.Extensions.DependencyInjection.AsyncServiceScope scope = this.ServiceProvider.CreateAsyncScope())
                {
                    global::BasaltHexagons.CommandLine.CommandContext context = scope.ServiceProvider.GetRequiredService<global::BasaltHexagons.CommandLine.CommandContext>();
                    context.InvocationContext = invocationContext;
                    context.Options = options;
                    global::MyNamespace.Level3Command command = scope.ServiceProvider.GetRequiredService<global::MyNamespace.Level3Command>();
                    await command.ExecuteAsync();
                }
            });
            this.OnCommandLineBuilt(cliCommand);
            return cliCommand;
        }

        partial void OnCommandLineBuilt(Command command);
    }
}
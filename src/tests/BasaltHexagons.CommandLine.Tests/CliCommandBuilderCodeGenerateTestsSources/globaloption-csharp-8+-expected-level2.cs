namespace MyNamespace
{
    partial class Level2CommandOptions
    {
        public Level2CommandOptions(int? OptionLevel2A, string? OptionLevel2B, System.IO.FileInfo? OptionLevel2C, int OptionLevel2D, int? OptionLevel1A, string? OptionLevel1B, System.IO.FileInfo? OptionLevel1C)
        {
            this.OptionLevel2A = OptionLevel2A;
            this.OptionLevel2B = OptionLevel2B;
            this.OptionLevel2C = OptionLevel2C;
            this.OptionLevel2D = OptionLevel2D;
            this.OptionLevel1A = OptionLevel1A;
            this.OptionLevel1B = OptionLevel1B;
            this.OptionLevel1C = OptionLevel1C;
        }

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

    partial class Level2CommandBuilder
    {
        protected string Description { get; }

        internal global::System.CommandLine.Option<int?> OptionLevel2AOption { get; }

        internal global::System.CommandLine.Option<string?> OptionLevel2BOption { get; }

        internal global::System.CommandLine.Option<System.IO.FileInfo?> OptionLevel2COption { get; }

        private global::System.CommandLine.Option<int> OptionLevel2DOption { get; }

        protected override global::System.CommandLine.Command BuildCliCommandCore()
        {
            string name = this.GetCliCommandBuilderAttribute().Name;
            global::System.CommandLine.Command cliCommand = this.CreateCliCommand(name, this.Description);
            cliCommand.AddGlobalOption(OptionLevel2AOption);
            cliCommand.AddGlobalOption(OptionLevel2BOption);
            cliCommand.AddGlobalOption(OptionLevel2COption);
            cliCommand.AddOption(OptionLevel2DOption);
            cliCommand.SetHandler(async (global::System.CommandLine.Invocation.InvocationContext invocationContext) =>
            {
                global::System.CommandLine.Parsing.ParseResult parseResult = invocationContext.BindingContext.ParseResult;
                global::MyNamespace.Level2CommandOptions options = new global::MyNamespace.Level2CommandOptions(OptionLevel1A: parseResult.GetValueForOption(this.GetRequiredParentBuilder<global::MyNamespace.Level1CommandBuilder>().OptionLevel1AOption), OptionLevel1B: parseResult.GetValueForOption(this.GetRequiredParentBuilder<global::MyNamespace.Level1CommandBuilder>().OptionLevel1BOption), OptionLevel1C: parseResult.GetValueForOption(this.GetRequiredParentBuilder<global::MyNamespace.Level1CommandBuilder>().OptionLevel1COption), OptionLevel2A: parseResult.GetValueForOption(this.OptionLevel2AOption), OptionLevel2B: parseResult.GetValueForOption(this.OptionLevel2BOption), OptionLevel2C: parseResult.GetValueForOption(this.OptionLevel2COption), OptionLevel2D: parseResult.GetValueForOption(this.OptionLevel2DOption));
                await using (global::Microsoft.Extensions.DependencyInjection.AsyncServiceScope scope = this.ServiceProvider.CreateAsyncScope())
                {
                    global::BasaltHexagons.CommandLine.CommandContext context = scope.ServiceProvider.GetRequiredService<global::BasaltHexagons.CommandLine.CommandContext>();
                    context.InvocationContext = invocationContext;
                    context.Options = options;
                    global::MyNamespace.Level2Command command = scope.ServiceProvider.GetRequiredService<global::MyNamespace.Level2Command>();
                    await command.ExecuteAsync();
                }
            });
            this.OnCommandLineBuilt(cliCommand);
            return cliCommand;
        }

        partial void OnCommandLineBuilt(Command command);
    }
}
namespace MyNamespace
{
    partial class Level1CommandOptions
    {
        public Level1CommandOptions(int? OptionLevel1A, string? OptionLevel1B, System.IO.FileInfo? OptionLevel1C, int OptionLevel1D)
        {
            this.OptionLevel1A = OptionLevel1A;
            this.OptionLevel1B = OptionLevel1B;
            this.OptionLevel1C = OptionLevel1C;
            this.OptionLevel1D = OptionLevel1D;
        }
    }
}

namespace MyNamespace
{
    using System.CommandLine;
    using Microsoft.Extensions.DependencyInjection;

    partial class Level1CommandBuilder
    {
        protected string Description { get; }

        internal global::System.CommandLine.Option<int?> OptionLevel1AOption { get; }

        internal global::System.CommandLine.Option<string?> OptionLevel1BOption { get; }

        internal global::System.CommandLine.Option<System.IO.FileInfo?> OptionLevel1COption { get; }

        private global::System.CommandLine.Option<int> OptionLevel1DOption { get; }

        protected override global::System.CommandLine.Command BuildCliCommandCore()
        {
            string name = this.GetCliCommandBuilderAttribute().Name;
            global::System.CommandLine.Command cliCommand = this.CreateCliCommand(name, this.Description);
            cliCommand.AddGlobalOption(OptionLevel1AOption);
            cliCommand.AddGlobalOption(OptionLevel1BOption);
            cliCommand.AddGlobalOption(OptionLevel1COption);
            cliCommand.AddOption(OptionLevel1DOption);
            cliCommand.SetHandler(async (global::System.CommandLine.Invocation.InvocationContext invocationContext) =>
            {
                global::System.CommandLine.Parsing.ParseResult parseResult = invocationContext.BindingContext.ParseResult;
                global::MyNamespace.Level1CommandOptions options = new global::MyNamespace.Level1CommandOptions(OptionLevel1A: parseResult.GetValueForOption(this.OptionLevel1AOption), OptionLevel1B: parseResult.GetValueForOption(this.OptionLevel1BOption), OptionLevel1C: parseResult.GetValueForOption(this.OptionLevel1COption), OptionLevel1D: parseResult.GetValueForOption(this.OptionLevel1DOption));
                await using (global::Microsoft.Extensions.DependencyInjection.AsyncServiceScope scope = this.ServiceProvider.CreateAsyncScope())
                {
                    global::BasaltHexagons.CommandLine.CommandContext context = scope.ServiceProvider.GetRequiredService<global::BasaltHexagons.CommandLine.CommandContext>();
                    context.InvocationContext = invocationContext;
                    context.Options = options;
                    global::MyNamespace.Level1Command command = scope.ServiceProvider.GetRequiredService<global::MyNamespace.Level1Command>();
                    await command.ExecuteAsync();
                }
            });
            this.OnCommandLineBuilt(cliCommand);
            return cliCommand;
        }

        partial void OnCommandLineBuilt(Command command);
    }
}
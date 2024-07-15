using System;
using System.Threading.Tasks;
using Krotus.CommandLine;
using Krotus.CommandLine.Annotations;

namespace MiddlewareDemo.Commands;

[CliCommandBuilder(CliCommandBuilderAttribute.DefaultRootCommandName, null)]
partial class AppCliCommandBuilder : RootCliCommandBuilder<AppCommand, AppCommandOptions>
{
    public AppCliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "Krotus.CommandLine middleware demo";
    }
}

#nullable disable
partial class AppCommandOptions
{
}
#nullable restore

class AppCommand : Command<AppCommandOptions>
{
    public AppCommand(CommandContext commandContext) : base(commandContext)
    {
    }

    public override async ValueTask ExecuteAsync()
    {
        await Console.Out.WriteLineAsync("this is the root command");
    }
}
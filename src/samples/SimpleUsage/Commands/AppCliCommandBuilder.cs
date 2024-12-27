using System;
using BasaltHexagons.CommandLine;
using BasaltHexagons.CommandLine.Annotations;

namespace SimpleUsage.Commands;

[CliCommandBuilder(CliCommandBuilderAttribute.DefaultRootCommandName, null)]
partial class AppCliCommandBuilder : RootCliCommandBuilder
{
    public AppCliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "BasaltHexagons.CommandLine simple usage sample";
    }
}
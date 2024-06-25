using System;
using Basalt.CommandLine;
using Basalt.CommandLine.Annotations;

namespace SimpleUsage.Commands;

[CliCommandBuilder(CliCommandBuilderAttribute.DefaultRootCommandName, null)]
partial class AppCliCommandBuilder : RootCliCommandBuilder
{
    public AppCliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "Basalt.CommandLine simple usage sample";
    }
}
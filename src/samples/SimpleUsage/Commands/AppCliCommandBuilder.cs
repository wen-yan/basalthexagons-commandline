using System;
using Krotus.CommandLine;
using Krotus.CommandLine.Annotations;

namespace SimpleUsage.Commands;

[CliCommandBuilder(CliCommandBuilderAttribute.DefaultRootCommandName, null)]
partial class AppCliCommandBuilder : RootCliCommandBuilder
{
    public AppCliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "Krotus.CommandLine simple usage sample";
    }
}
using System;
using System.CommandLine;

namespace Basalt.CommandLine;

public abstract class RootCliCommandBuilder : CliCommandBuilder
{
    protected RootCliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override Command CreateCliCommand(string name, string? description)
        => new RootCommand(description ?? string.Empty);
}

public abstract class RootCliCommandBuilder<TOptions> : CliCommandBuilder<TOptions>
{
    protected RootCliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override Command CreateCliCommand(string name, string? description)
        => new RootCommand(description ?? string.Empty);
}

public abstract class RootCliCommandBuilder<TCommand, TOptions> : CliCommandBuilder<TCommand, TOptions>
    where TCommand : ICommand<TOptions>
{
    protected RootCliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override Command CreateCliCommand(string name, string? description)
        => new RootCommand(description ?? string.Empty);
}
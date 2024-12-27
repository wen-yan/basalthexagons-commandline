using System;
using System.CommandLine;

namespace BasaltHexagons.CommandLine;

/// <summary>
/// Cli command builder for root command.
/// </summary>
public abstract class RootCliCommandBuilder : CliCommandBuilder
{
    /// <inheritdoc />
    protected RootCliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Template method for creating cli command. Returns new `RootCommand` by default.
    /// </summary>
    /// <param name="name">Command name. It's not used since root command doesn't need a name.</param>
    /// <param name="description">Command description</param>
    /// <returns>Cli command instance</returns>
    protected override Command CreateCliCommand(string name, string? description)
        => new RootCommand(description ?? string.Empty);
}

/// <summary>
/// Cli command builder for root command.
/// </summary>
/// <typeparam name="TOptions">Command options type</typeparam>
public abstract class RootCliCommandBuilder<TOptions> : CliCommandBuilder<TOptions>
{
    /// <inheritdoc />
    protected RootCliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Template method for creating cli command. Returns new `RootCommand` by default.
    /// </summary>
    /// <param name="name">Command name. It's not used since root command doesn't need a name.</param>
    /// <param name="description">Command description</param>
    /// <returns>Cli command instance</returns>
    protected override Command CreateCliCommand(string name, string? description)
        => new RootCommand(description ?? string.Empty);
}

/// <summary>
/// Cli command builder for root command.
/// </summary>
/// <typeparam name="TCommand">Command type</typeparam>
/// <typeparam name="TOptions">Command options type</typeparam>
public abstract class RootCliCommandBuilder<TCommand, TOptions> : CliCommandBuilder<TCommand, TOptions>
    where TCommand : ICommand<TOptions>
{
    /// <inheritdoc />
    protected RootCliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Template method for creating cli command. Returns new `RootCommand` by default.
    /// </summary>
    /// <param name="name">Command name. It's not used since root command doesn't need a name.</param>
    /// <param name="description">Command description</param>
    /// <returns>Cli command instance</returns>
    protected override Command CreateCliCommand(string name, string? description)
        => new RootCommand(description ?? string.Empty);
}
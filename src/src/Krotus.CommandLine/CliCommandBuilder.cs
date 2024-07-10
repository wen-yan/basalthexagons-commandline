using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading;
using Krotus.CommandLine.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Krotus.CommandLine;

/// <summary>
/// Base class of cli command builder.
/// </summary>
public abstract class CliCommandBuilder : ICliCommandBuilder
{
    private readonly Lazy<Command> _command;
    private readonly Lazy<(string CommandKey, string? ParentCommandKey)> _commandKeys;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="serviceProvider"></param>
    protected CliCommandBuilder(IServiceProvider serviceProvider)
    {
        this.ServiceProvider = serviceProvider;
        _command = new Lazy<Command>(this.BuildCliCommand, LazyThreadSafetyMode.None);
        _commandKeys = new Lazy<(string CommandKey, string? ParentCommandKey)>(() =>
        {
            CliCommandBuilderAttribute attribute = this.GetCliCommandBuilderAttribute();
            return (attribute.CommandKey, attribute.ParentCommandKey);
        }, LazyThreadSafetyMode.None);
    }

    /// <summary>
    /// IServiceProvider instance.
    /// </summary>
    protected IServiceProvider ServiceProvider { get; }

    /// <inheritdoc />
    public Command CliCommand => _command.Value;

    /// <inheritdoc />
    public string CommandKey => _commandKeys.Value.CommandKey;

    /// <inheritdoc />
    public string? ParentCommandKey => _commandKeys.Value.ParentCommandKey;

    private Command BuildCliCommand()
    {
        Command cliCommand = this.BuildCliCommandCore();

        // Find all children commands
        IEnumerable<ICliCommandBuilder> children = this.ServiceProvider.GetServices<ICliCommandBuilder>()
            .Where(x => x.ParentCommandKey == this.CommandKey);

        foreach (ICliCommandBuilder child in children)
        {
            cliCommand.AddCommand(child.CliCommand);
        }

        return cliCommand;
    }

    /// <summary>
    /// Create cli command instance.
    /// </summary>
    /// <param name="name">Command name.</param>
    /// <param name="description">Command description.</param>
    /// <returns>Cli command instance</returns>
    protected virtual Command CreateCliCommand(string name, string? description)
        => new(name, description);

    /// <summary>
    /// Abstract method for creating and configuring cli command instance.
    /// This method usually is implemented by `CliCommandBuilderCodeGenerator`.
    /// </summary>
    /// <returns>Cli command instance.</returns>
    protected abstract Command BuildCliCommandCore();

    /// <summary>
    /// Get `CliCommandBuilderAttribute` instance of `this` cli command builder.
    /// </summary>
    /// <returns>CliCommandBuilderAttribute instance.</returns>
    /// <exception cref="ApplicationException">Throw when no CliCommandBuilderAttribute is found.</exception>
    protected CliCommandBuilderAttribute GetCliCommandBuilderAttribute()
    {
        CliCommandBuilderAttribute? attribute = CliCommandBuilderAttribute.GetFromType(this.GetType());
        if (attribute == null)
            throw new ApplicationException($"CliCommandBuild doesn't have {nameof(CliCommandBuilderAttribute)}, type: {this.GetType()}");
        return attribute;
    }

    /// <summary>
    /// Get parent cli command builder.
    /// </summary>
    /// <typeparam name="T">Parent cli command builder type.</typeparam>
    /// <returns>Parent cli command builder instance. Null when this is a root cli command builder.</returns>
    protected T? GetParentBuilder<T>() where T : class, ICliCommandBuilder
    {
        CliCommandBuilderAttribute attribute = this.GetCliCommandBuilderAttribute();

        string? parentKey = attribute.ParentCommandKey;
        return parentKey == null ? default : this.ServiceProvider.GetRequiredKeyedService<ICliCommandBuilder>(parentKey) as T ?? default;
    }

    /// <summary>
    /// Get parent cli command builder. Throw exception when parent cli command build is not found.
    /// </summary>
    /// <typeparam name="T">Parent cli command builder type.</typeparam>
    /// <returns>Parent cli command builder instance.</returns>
    /// <exception cref="ApplicationException">Throw when not found.</exception>
    protected T GetRequiredParentBuilder<T>() where T : class, ICliCommandBuilder
    {
        return this.GetParentBuilder<T>() ?? throw new ApplicationException($"CliCommandBuilder [{this.GetType()}] doesn't have parent CliCommandBuilder");
    }
}

/// <summary>
/// Base class of cli command builder.
/// </summary>
/// <typeparam name="TOptions">Command options type.</typeparam>
public abstract class CliCommandBuilder<TOptions> : CliCommandBuilder
{
    /// <inheritdoc />
    protected CliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}

/// <summary>
/// Base class of cli command builder.
/// </summary>
/// <typeparam name="TCommand">Command type.</typeparam>
/// <typeparam name="TOptions">Command options type.</typeparam>
public abstract class CliCommandBuilder<TCommand, TOptions> : CliCommandBuilder<TOptions>
    where TCommand : ICommand<TOptions>
{
    /// <inheritdoc />
    protected CliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}
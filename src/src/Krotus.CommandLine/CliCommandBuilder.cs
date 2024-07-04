using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading;
using Krotus.CommandLine.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Krotus.CommandLine;

// CliCommandBuilder
public abstract class CliCommandBuilder : ICliCommandBuilder
{
    private readonly Lazy<Command> _command;
    private readonly Lazy<(string CommandKey, string? ParentCommandKey)> _commandKeys;

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

    protected IServiceProvider ServiceProvider { get; }

    public Command CliCommand => _command.Value;
    public string CommandKey => _commandKeys.Value.CommandKey;
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

    protected virtual Command CreateCliCommand(string name, string? description)
        => new(name, description);

    protected abstract Command BuildCliCommandCore();

    protected CliCommandBuilderAttribute GetCliCommandBuilderAttribute()
    {
        CliCommandBuilderAttribute? attribute = CliCommandBuilderAttribute.GetFromType(this.GetType());
        if (attribute == null)
            throw new ApplicationException($"CliCommandBuild doesn't have {typeof(CliCommandBuilderAttribute).Name}, type: {this.GetType()}");
        return attribute;
    }

    protected T? GetParentBuilder<T>() where T : class, ICliCommandBuilder
    {
        CliCommandBuilderAttribute attribute = this.GetCliCommandBuilderAttribute();

        string? parentKey = attribute.ParentCommandKey;
        return parentKey == null ? default : this.ServiceProvider.GetRequiredKeyedService<ICliCommandBuilder>(parentKey) as T ?? default;
    }

    protected T GetRequiredParentBuilder<T>() where T : class, ICliCommandBuilder
    {
        return this.GetParentBuilder<T>() ?? throw new ApplicationException($"CliCommandBuilder [{this.GetType()}] doesn't have parent CliCommandBuilder");
    }
}

public abstract class CliCommandBuilder<TOptions> : CliCommandBuilder
{
    protected CliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}

public abstract class CliCommandBuilder<TCommand, TOptions> : CliCommandBuilder<TOptions>
    where TCommand : ICommand<TOptions>
{
    protected CliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}
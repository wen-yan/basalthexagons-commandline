using System;
using System.Linq;

namespace Krotus.CommandLine.Annotations;

/// <summary>
/// Attribute class for marking cli command builder class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class CliCommandBuilderAttribute : Attribute
{
    /// <summary>
    /// Default root command name.
    /// </summary>
    public const string DefaultRootCommandName = "__root";

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="name">Cli command name.</param>
    /// <param name="parent">Parent cli command builder type.</param>
    public CliCommandBuilderAttribute(string name, Type? parent)
    {
        this.Name = name;
        this.Parent = parent;
    }

    /// <summary>
    /// Cli command name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Parent cli command builder type.
    /// </summary>
    public Type? Parent { get; }

    /// <summary>
    /// Cli command key. It's unique key in cli command tree.
    /// </summary>
    public string CommandKey => this.Parent == null ? this.Name : $"{this.ParentCommandKey} {this.Name}";

    /// <summary>
    /// Cli command parent's key.
    /// </summary>
    public string? ParentCommandKey => GetFromType(this.Parent)?.CommandKey;

    /// <summary>
    /// Get CliCommandBuilderAttribute instance from type.
    /// </summary>
    /// <param name="type">Type to get CliCommandBuilderAttribute instance.</param>
    /// <returns>CliCommandBuilderAttribute instance.</returns>
    public static CliCommandBuilderAttribute? GetFromType(Type? type)
    {
        return type?
            .GetCustomAttributes(typeof(CliCommandBuilderAttribute), false)
            .Cast<CliCommandBuilderAttribute>()
            .FirstOrDefault();
    }
}
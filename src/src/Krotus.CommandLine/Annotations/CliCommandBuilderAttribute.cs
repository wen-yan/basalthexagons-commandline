using System;
using System.Linq;

namespace Krotus.CommandLine.Annotations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class CliCommandBuilderAttribute : Attribute
{
    public const string DefaultRootCommandName = "__root";

    public CliCommandBuilderAttribute(string name, Type? parent)
    {
        this.Name = name;
        this.Parent = parent;
    }

    public string Name { get; }
    public Type? Parent { get; }

    public string CommandKey => this.Parent == null ? this.Name : $"{this.ParentCommandKey} {this.Name}";
    public string? ParentCommandKey => GetFromType(this.Parent)?.CommandKey;

    public static CliCommandBuilderAttribute? GetFromType(Type? type)
    {
        return type?
            .GetCustomAttributes(typeof(CliCommandBuilderAttribute), false)
            .Cast<CliCommandBuilderAttribute>()
            .FirstOrDefault();
    }
}
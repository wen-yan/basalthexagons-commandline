using System;

namespace Krotus.CommandLine.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class CliCommandSymbolAttribute : Attribute
{
    public CliCommandSymbolAttribute(CliCommandSymbolType symbolType = CliCommandSymbolType.Option)
    {
        this.SymbolType = symbolType;
    }

    public CliCommandSymbolType SymbolType { get; }
}
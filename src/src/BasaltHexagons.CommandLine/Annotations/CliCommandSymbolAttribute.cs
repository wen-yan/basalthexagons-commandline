using System;

namespace BasaltHexagons.CommandLine.Annotations;

/// <summary>
/// Attribution class for configuring command options properties' command line token type.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CliCommandSymbolAttribute : Attribute
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="symbolType">Command line token type.</param>
    public CliCommandSymbolAttribute(CliCommandSymbolType symbolType = CliCommandSymbolType.Option)
    {
        this.SymbolType = symbolType;
    }

    /// <summary>
    /// Command line token type.
    /// </summary>
    public CliCommandSymbolType SymbolType { get; }
}
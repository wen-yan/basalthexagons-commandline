namespace Krotus.CommandLine.Annotations;

/// <summary>
/// Enum of command line token type.
/// </summary>
public enum CliCommandSymbolType
{
    /// <summary>
    /// Command line option. CliCommandBuilderCodeGenerator creates an Option property in cli command builder class and adds it to cli command using `AddOption()` method.
    /// </summary>
    Option,
    
    /// <summary>
    /// Command line global option. CliCommandBuilderCodeGenerator creates an Option property in cli command builder class and adds it to cli command using `AddGlobalOption()` method.
    /// </summary>
    GlobalOption,
    
    /// <summary>
    /// Command line argument. CliCommandBuilderCodeGenerator creates an Argument property in cli command builder class and adds it to cli command using `AddArgument()` method.
    /// </summary>
    Argument,
    
    /// <summary>
    /// This should NOT be used directly in code. CliCommandBuilderCodeGenerator uses this to assign value from parent builder's `GlobalOption`.
    /// </summary>
    FromGlobalOption
}
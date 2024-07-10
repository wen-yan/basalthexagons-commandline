using System.CommandLine;

namespace Krotus.CommandLine;

/// <summary>
/// Cli command builder interface. All cli command builders should implement it.
/// A cli command builder is responsible for
///     - Setup cli command line, Options, GlobalOptions and Arguments
///     - Setup cli command line tree
///     - Create and configure Command instance
/// </summary>
public interface ICliCommandBuilder
{
    /// <summary>
    /// Cli command instance.
    /// </summary>
    Command CliCommand { get; }
    
    /// <summary>
    /// Cli command key. It's unique key in cli command tree.
    /// </summary>
    string CommandKey { get; }
    
    /// <summary>
    /// Cli command parent's key.
    /// </summary>
    string? ParentCommandKey { get; }
}
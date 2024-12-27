using System;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace BasaltHexagons.CommandLine;

/// <summary>
/// Command context. Available during command lifecycle.
/// </summary>
public sealed class CommandContext
{
    /// <summary>
    /// InvocationContext instance.
    /// </summary>
    public InvocationContext? InvocationContext { get; set; }

    /// <summary>
    /// Command options instance.
    /// </summary>
    public object? Options { get; set; }
}

/// <summary>
/// Command interface. All command classes should implement it.
/// </summary>
/// <typeparam name="TOptions">Command options type.</typeparam>
public interface ICommand<TOptions> : IAsyncDisposable
{
    /// <summary>
    /// Execute command.
    /// </summary>
    /// <returns>ValueTask instance</returns>
    ValueTask ExecuteAsync();
}
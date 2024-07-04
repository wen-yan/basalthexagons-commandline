using System;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Krotus.CommandLine;

public sealed class CommandContext
{
    public InvocationContext? InvocationContext { get; set; }
    public object? Options { get; set; }
}

public interface ICommand<TOptions> : IAsyncDisposable
{
    ValueTask Execute();
}
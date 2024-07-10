using System.Threading.Tasks;

namespace Krotus.CommandLine;

/// <summary>
/// Base class of command.
/// </summary>
/// <typeparam name="TOptions">Command options type.</typeparam>
public abstract class Command<TOptions> : ICommand<TOptions>
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="commandContext">CommandContext instance.</param>
    protected Command(CommandContext commandContext)
    {
        this.CommandContext = commandContext;
    }

    /// <summary>
    /// CommandContext instance.
    /// </summary>
    protected CommandContext CommandContext { get; }

    /// <summary>
    /// Command options instance. Its properties are assigned from command line tokens. 
    /// </summary>
    protected TOptions Options => (TOptions)this.CommandContext.Options!;

    /// <inheritdoc />
    public abstract ValueTask Execute();

    /// <inheritdoc />
    public virtual ValueTask DisposeAsync() => default;
}
using System.Threading.Tasks;

namespace Krotus.CommandLine;

public abstract class Command<TOptions> : ICommand<TOptions>
{
    protected Command(CommandContext commandContext)
    {
        this.CommandContext = commandContext;
    }

    protected CommandContext CommandContext { get; }
    protected TOptions Options => (TOptions)this.CommandContext.Options!;

    public abstract ValueTask Execute();

    public virtual ValueTask DisposeAsync() => default;
}
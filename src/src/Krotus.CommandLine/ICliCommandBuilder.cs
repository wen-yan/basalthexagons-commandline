using System.CommandLine;

namespace Krotus.CommandLine;

public interface ICliCommandBuilder
{
    Command CliCommand { get; }
    string CommandKey { get; }
    string? ParentCommandKey { get; }
}
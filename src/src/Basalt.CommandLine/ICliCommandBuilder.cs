using System.CommandLine;

namespace Basalt.CommandLine;

public interface ICliCommandBuilder
{
    Command CliCommand { get; }
    string CommandKey { get; }
    string? ParentCommandKey { get; }
}
# Basalt.CommandLine

## What is Basalt.CommandLine?

Basalt.CommandLine is a project for building command line application using System.CommandLine package. It includes
- CLI command builder base classes
- Code generators for generating CLI command builder classes
- Base classes and interfaces for command
- Extension methods for adding dependency injection


Its code generator can generate code for building cli command from [command](https://github.com/wen-yan/basalt-commandline/blob/master/src/src/Basalt.CommandLine/ICommand.cs) and option classes. It supports command line Option, GlobalOption, Argument and command hierarchy.

## Code Sample
Here is an example of using it to generate command line. Full sample code is [here](https://github.com/wen-yan/basalt-commandline/tree/master/src/samples/SimpleUsage).
```csharp
#nullable disable
partial class FsLsCommandOptions
{
    public ConsoleColor Color { get; init; }

    [CliCommandSymbol(CliCommandSymbolType.Argument)]
    public string Directory { get; init; }
}
#nullable restore

[CliCommandBuilder("ls", typeof(FsCliCommandBuilder))]
partial class FsLsCliCommandBuilder : CliCommandBuilder<FsLsCommand, FsLsCommandOptions>
{
    public FsLsCliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "ls command";
        this.ColorOption = new(["--color", "-c"], () => ConsoleColor.Yellow, "command output color");
        this.DirectoryArgument = new("directory", "target directory of ls command");
    }
}

class FsLsCommand : Command<FsLsCommandOptions>
{
    public FsLsCommand(CommandContext commandContext, IDemoService demoService) : base(commandContext)
    {
        this.DemoService = demoService;
    }

    private IDemoService DemoService { get; }

    public override async ValueTask Execute()
    {
        ConsoleColor currentColor = Console.ForegroundColor;
        Console.ForegroundColor = this.Options.Color;
        try
        {
            await this.DemoService.WriteLine($"FsLsCommand output, endpoint: {this.Options.Endpoint}, target: {this.Options.Directory}");
        }
        finally
        {
            Console.ForegroundColor = currentColor;
        }
    }
}
```

More samples can be found in [samples](https://github.com/wen-yan/basalt-commandline/tree/master/src/samples).


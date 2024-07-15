using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Krotus.CommandLine.CodeGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Krotus.CommandLine.Tests;

[TestClass]
public class IntegrationTests
{
    [TestMethod]
    public void GlobalOption_Test()
    {
        string code =
            """
            [CliCommandBuilder(CliCommandBuilderAttribute.DefaultRootCommandName, null)]
            partial class AppCliCommandBuilder : RootCliCommandBuilder
            {
                public AppCliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                    this.Description = "Krotus.CommandLine simple usage sample";
                }
            }

            #nullable disable
            partial class FsCommandOptions
            {
                [CliCommandSymbol(CliCommandSymbolType.GlobalOption)]
                public string Endpoint { get; init; }
            }
            #nullable restore

            [CliCommandBuilder("fs", typeof(AppCliCommandBuilder))]
            partial class FsCliCommandBuilder : CliCommandBuilder<FsCommand, FsCommandOptions>
            {
                public FsCliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                    this.Description = "fs command";
                    this.EndpointOption = new(new[] {"--endpoint"}, "endpoint of the file system") { IsRequired = true };
                }
            }
            class FsCommand : Command<FsCommandOptions>
            {
                public FsCommand(CommandContext commandContext) : base(commandContext) {}
                public override async ValueTask ExecuteAsync() => await Console.Out.WriteLineAsync($"FsCommand output, endpoint: {this.Options.Endpoint}");
            }

            #nullable disable
            partial class FsLsCommandOptions
            {
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
                    this.DirectoryArgument = new("directory", "target directory of ls command");
                }
            }
            class FsLsCommand : Command<FsLsCommandOptions>
            {
                public FsLsCommand(CommandContext commandContext) : base(commandContext) {}
                public override async ValueTask ExecuteAsync() => Console.WriteLine($"FsLsCommand output, endpoint: {this.Options.Endpoint}, target: {this.Options.Directory}");
            }
            """;

        this.Test(code, $"FsCommand output, endpoint: test-endpoint{Environment.NewLine}", "fs", "--endpoint", "test-endpoint");
        this.Test(code, $"FsLsCommand output, endpoint: test-endpoint, target: test-argument{Environment.NewLine}", "fs", "ls", "--endpoint", "test-endpoint", "test-argument");
    }

    [TestMethod]
    public void RootCommand_Test()
    {
        string code =
            """
            [CliCommandBuilder(CliCommandBuilderAttribute.DefaultRootCommandName, null)]
            partial class AppCliCommandBuilder : RootCliCommandBuilder<AppCommand, AppCommandOptions>
            {
                public AppCliCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
                {
                    this.Description = "Krotus.CommandLine simple usage sample";
                    this.EndpointOption = new(new[] {"--endpoint"}, "endpoint of the file system") { IsRequired = true };
                }
            }

            #nullable disable
            partial class AppCommandOptions
            {
                public string Endpoint { get; init; }
            }
            #nullable restore

            class AppCommand : Command<AppCommandOptions>
            {
                public AppCommand(CommandContext commandContext) : base(commandContext) {}
                public override async ValueTask ExecuteAsync() => await Console.Out.WriteLineAsync($"AppCommand output, endpoint: {this.Options.Endpoint}");
            }
            """;

        this.Test(code, $"AppCommand output, endpoint: test-endpoint{Environment.NewLine}", "--endpoint", "test-endpoint");
    }

    private void Test(string code, string expectedResult, params string[] args)
    {
        string finalCode =
            $$"""
              using System;
              using System.CommandLine;
              using System.IO;
              using System.Threading.Tasks;
              using Krotus.CommandLine;
              using Krotus.CommandLine.Annotations;
              using Microsoft.Extensions.DependencyInjection;
              using Microsoft.Extensions.Hosting;

              namespace TestNamespace;

              public static class Program
              {
                  public static async Task<string> TestEntry(string[] args)
                  {
                      IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
              
                      hostBuilder
                          .ConfigureServices((context, services) =>
                          {
                              services
                                  .AddCommandLineSupport(assembly: System.Reflection.Assembly.GetExecutingAssembly())
                                  ;
                          });
                      
                      IHost host = hostBuilder.Build();
                      
                      TextWriter oldWriter = Console.Out;
                      StringWriter writer = new();
                      Console.SetOut(writer);
                      
                      try
                      {
                          RootCommand rootCommand = host.Services.GetRequiredService<RootCommand>();
                          await rootCommand.InvokeAsync(args);
                      }
                      finally
                      {
                          Console.SetOut(oldWriter);
                      }
                      return writer.ToString();
                  }
              }
              {{code}}
              """;

        this.TestImpl(finalCode, expectedResult, args);
    }

    private void TestImpl(string code, string expectedResult, params string[] args)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(LanguageVersion.CSharp11));
        string basePath = Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location)!;

        string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        IEnumerable<string> referencePaths = Directory.EnumerateFiles(assemblyDir)
            .Where(x => Path.GetExtension(x) == ".dll")
            .Where(x => !x.EndsWith("Krotus.CommandLine.CodeGenerators.dll"))
            .Append(typeof(object).GetTypeInfo().Assembly.Location)
            .Append(typeof(Console).GetTypeInfo().Assembly.Location)
            .Append(Path.Combine(basePath, "System.Runtime.dll"))
            .Append(Path.Combine(basePath, "System.Runtime.Extensions.dll"))
            .Append(Path.Combine(basePath, "mscorlib.dll"))
            .Append(Path.Combine(basePath, "System.ComponentModel.dll"))
            .Distinct();

        IEnumerable<PortableExecutableReference> executableReferences = referencePaths
            .Where(x => File.Exists(x))
            .Select(x => MetadataReference.CreateFromFile(x));

        CSharpCompilation inCompilation = CSharpCompilation.Create(Path.GetRandomFileName(),
            new[] { syntaxTree },
            executableReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Create the driver that will control the generation, passing in our generator
        CliCommandBuilderCodeGenerator generator = new();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the generation pass
        // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
        driver = driver.RunGeneratorsAndUpdateCompilation(inCompilation, out var outputCompilation, out var diagnostics);

        // Or we can look at the results directly:
        GeneratorDriverRunResult runResult = driver.GetRunResult();

        using var memoryStream = new MemoryStream();
        EmitResult compilationResult = outputCompilation.Emit(memoryStream);
        if (!compilationResult.Success)
        {
            var errors = compilationResult.Diagnostics
                .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
                .ToList();
            Assert.Fail($"Failed to compile code, errors: {errors}");
        }
        else
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            AssemblyLoadContext assemblyContext = new(Path.GetRandomFileName(), true);
            Assembly assembly = assemblyContext.LoadFromStream(memoryStream);

            Type type = assembly.GetType("TestNamespace.Program")!;
            MethodInfo method = type.GetMethod("TestEntry")!;
            Task<string> result = (Task<string>?)method.Invoke(null, BindingFlags.InvokeMethod, Type.DefaultBinder, [args], null)!;

            assemblyContext.Unload();

            Assert.AreEqual(expectedResult, result.Result);
        }
    }
}
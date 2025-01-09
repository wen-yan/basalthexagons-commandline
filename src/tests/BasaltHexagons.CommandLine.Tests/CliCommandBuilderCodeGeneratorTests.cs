using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BasaltHexagons.CommandLine.CodeGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BasaltHexagons.CommandLine.Tests;

[TestClass]
public class CliCommandBuilderCodeGeneratorTests
{
    // CSharp 5 starts supporting `await`
    [DataTestMethod]
    [DataRow(LanguageVersion.CSharp1)]
    [DataRow(LanguageVersion.CSharp2)]
    [DataRow(LanguageVersion.CSharp3)]
    [DataRow(LanguageVersion.CSharp4)]
    public Task Initialize_Basic_1_4_Test(LanguageVersion langVersion)
    {
        return RunTest(langVersion, "basic-csharp-1-4.cs", [("MyNamespace.FsCommandBuilder.g.cs", "basic-csharp-1-4-expected.cs")], compilerDiagnostics: CompilerDiagnostics.None);
    }

    [DataTestMethod]
    [DataRow(LanguageVersion.CSharp5)]
    public Task Initialize_Basic_5_Test(LanguageVersion langVersion)
    {
        return RunTest(langVersion, "basic-csharp-5-7.cs", [("MyNamespace.FsCommandBuilder.g.cs", "basic-csharp-5-expected.cs")]);
    }

    [DataTestMethod]
    [DataRow(LanguageVersion.CSharp6)]
    [DataRow(LanguageVersion.CSharp7)]
    [DataRow(LanguageVersion.CSharp7_1)]
    [DataRow(LanguageVersion.CSharp7_2)]
    [DataRow(LanguageVersion.CSharp7_3)]
    public Task Initialize_Basic_6_7_Test(LanguageVersion langVersion)
    {
        return RunTest(langVersion, "basic-csharp-5-7.cs", [("MyNamespace.FsCommandBuilder.g.cs", "basic-csharp-6-7-expected.cs")]);
    }

    [DataTestMethod]
    [DataRow(LanguageVersion.CSharp8)]
    [DataRow(LanguageVersion.CSharp9)]
    [DataRow(LanguageVersion.CSharp10)]
    [DataRow(LanguageVersion.CSharp11)]
    [DataRow(LanguageVersion.LatestMajor)]
    [DataRow(LanguageVersion.Latest)]
    [DataRow(LanguageVersion.Default)]
    public Task Initialize_Basic_8_plus_Test(LanguageVersion langVersion)
    {
        return RunTest(langVersion, "basic-csharp-8+.cs",
        [
            ("MyNamespace.FsCommandBuilder.g.cs", "basic-csharp-8+-expected.cs"),
        ]);
    }

    [DataTestMethod]
    [DataRow(LanguageVersion.CSharp8)]
    [DataRow(LanguageVersion.CSharp9)]
    [DataRow(LanguageVersion.CSharp10)]
    [DataRow(LanguageVersion.CSharp11)]
    [DataRow(LanguageVersion.LatestMajor)]
    [DataRow(LanguageVersion.Latest)]
    [DataRow(LanguageVersion.Default)]
    public Task Initialize_Nullable_8_plus_Test(LanguageVersion langVersion)
    {
        return RunTest(langVersion, "nullable-csharp-8+.cs",
        [
            ("MyNamespace.FsCommandBuilder.g.cs", "nullable-csharp-8+-expected.cs"),
        ]);
    }

    [DataTestMethod]
    [DataRow(LanguageVersion.CSharp8)]
    [DataRow(LanguageVersion.CSharp9)]
    [DataRow(LanguageVersion.CSharp10)]
    [DataRow(LanguageVersion.CSharp11)]
    [DataRow(LanguageVersion.LatestMajor)]
    [DataRow(LanguageVersion.Latest)]
    [DataRow(LanguageVersion.Default)]
    public Task Initialize_GlobalOption_8_plus_Test(LanguageVersion langVersion)
    {
        return RunTest(langVersion, "globaloption-csharp-8+.cs",
        [
            ("MyNamespace.Level1CommandBuilder.g.cs", "globaloption-csharp-8+-expected-level1.cs"),
            ("MyNamespace.Level2CommandBuilder.g.cs", "globaloption-csharp-8+-expected-level2.cs"),
            ("MyNamespace.Level3CommandBuilder.g.cs", "globaloption-csharp-8+-expected-level3.cs"),
        ]);
    }

    [DataTestMethod]
    [DataRow(LanguageVersion.CSharp8)]
    [DataRow(LanguageVersion.CSharp9)]
    [DataRow(LanguageVersion.CSharp10)]
    [DataRow(LanguageVersion.CSharp11)]
    [DataRow(LanguageVersion.LatestMajor)]
    [DataRow(LanguageVersion.Latest)]
    [DataRow(LanguageVersion.Default)]
    public Task Initialize_NoCommand_8_plus_Test(LanguageVersion langVersion)
    {
        return RunTest(langVersion, "nocommand-csharp-8+.cs", [("MyNamespace.FsCommandBuilder.g.cs", "nocommand-csharp-8+-expected.cs")]);
    }

    [DataTestMethod]
    [DataRow(LanguageVersion.CSharp8)]
    [DataRow(LanguageVersion.CSharp9)]
    [DataRow(LanguageVersion.CSharp10)]
    [DataRow(LanguageVersion.CSharp11)]
    [DataRow(LanguageVersion.LatestMajor)]
    [DataRow(LanguageVersion.Latest)]
    [DataRow(LanguageVersion.Default)]
    public Task Initialize_NoCommand_NoOptions_8_plus_Test(LanguageVersion langVersion)
    {
        return RunTest(langVersion, "nocommand-nooptions-csharp-8+.cs", [("MyNamespace.FsCommandBuilder.g.cs", "nocommand-nooptions-csharp-8+-expected.cs")]);
    }

    private static async Task RunTest(LanguageVersion langVersion, string source, (string expectedFileName, string expected)[] expectedGenerated, CompilerDiagnostics compilerDiagnostics = CompilerDiagnostics.Errors)
    {
        string sourceCode = await LoadEmbeddedResources(source);

        CSharpSourceGeneratorTest<CliCommandBuilderCodeGenerator, DefaultVerifier> test = new()
        {
            TestState =
            {
                Sources = { sourceCode },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(CliCommandBuilder).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.CommandLine.Command).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(IServiceProvider).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(ServiceProvider).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(AsyncServiceScope).Assembly.Location),
                },
            },
            SolutionTransforms =
            {
                (solution, projectId) =>
                {
                    Project? project = solution.GetProject(projectId);
                    CSharpParseOptions? parseOptions = project?.ParseOptions as CSharpParseOptions;
                    solution = solution.WithProjectParseOptions(projectId, parseOptions?.WithLanguageVersion(langVersion) ?? CSharpParseOptions.Default);
                    return solution;
                },
            },
            CompilerDiagnostics = compilerDiagnostics,
        };

        foreach ((string expectedFileName, string expected) in expectedGenerated)
        {
            string filename = Path.Combine("BasaltHexagons.CommandLine.CodeGenerators", "BasaltHexagons.CommandLine.CodeGenerators.CliCommandBuilderCodeGenerator", expectedFileName);
            SourceText content = SourceText.From(LoadEmbeddedResources(expected).Result, Encoding.UTF8);

            test.TestState.GeneratedSources.Add((filename, content));
        }

        await test.RunAsync();
    }

    private static async Task<string> LoadEmbeddedResources(string resourceName)
    {
        await using Stream stream = Assembly.GetExecutingAssembly()
                                        .GetManifestResourceStream($"BasaltHexagons.CommandLine.Tests.CliCommandBuilderCodeGenerateTestsSources.{resourceName}") ??
                                    throw new ApplicationException($"Can't load embedded resources: {resourceName}");

        using TextReader reader = new StreamReader(stream);
        string content = await reader.ReadToEndAsync();

        content = content.Replace("\r\n", "\n");
        content = content.Replace("\r", "\n");
        content = content.Replace("\n", "\r\n");
        return content;
    }
}
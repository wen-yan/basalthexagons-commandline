using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Basalt.CommandLine.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Basalt.CommandLine.CodeGenerator;

[Generator]
public class CliCommandBuilderCodeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var items = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                typeof(CliCommandBuilderAttribute).FullName!,
                predicate: static (SyntaxNode syntaxNode, CancellationToken cancel) => IsSyntaxTargetForGeneration(syntaxNode, cancel),
                transform: static (GeneratorAttributeSyntaxContext syntaxContext, CancellationToken cancel) => GetSemanticTarget(syntaxContext, cancel))
            .Where(static x => x != null);

        context.RegisterSourceOutput(items, static (SourceProductionContext ctx, CliCommandBuilderType? item) =>
        {
            if (item != null)
                GenerateOutput(ctx, item);
        });
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode, CancellationToken cancellation)
    {
        if (!syntaxNode.IsKind(SyntaxKind.ClassDeclaration))
            return false;

        if (syntaxNode is not ClassDeclarationSyntax classDeclaration)
            return false;

        if (classDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.AbstractKeyword)))
            return false;

        return true;
    }

    private static CliCommandBuilderType? GetSemanticTarget(GeneratorAttributeSyntaxContext context, CancellationToken cancellation)
    {
        if (context.SemanticModel.GetDeclaredSymbol(context.TargetNode) is not INamedTypeSymbol commandBuilderTypeSymbol)
            return null;

        if (commandBuilderTypeSymbol.BaseType == null)
            return null;

        INamedTypeSymbol? baseTypeSymbol = CliCommandBuilderType.GetCommandBuilderBaseType(commandBuilderTypeSymbol);
        if (baseTypeSymbol == null)
            return null;

        try
        {
            return new CliCommandBuilderType(commandBuilderTypeSymbol, baseTypeSymbol);
        }
        catch
        {
            return null;
        }
    }

    private static void GenerateOutput(SourceProductionContext context, CliCommandBuilderType cliCommandBuilderType)
    {
        static string GenerateOptionsClass(CliCommandBuilderType cliCommandBuilderType)
        {
            static IEnumerable<string> GenerateFromAncestors(CliCommandBuilderType cliCommandBuilderType)
            {
                while (cliCommandBuilderType.Parent != null)
                {
                    cliCommandBuilderType = cliCommandBuilderType.Parent;
                    foreach (OptionsProperty property in cliCommandBuilderType.OptionsProperties.Where(x => x.CliCommandSymbolType == CliCommandSymbolType.GlobalOption))
                    {
                        yield return $"// from {cliCommandBuilderType.CliCommandBuilderTypeSymbol}";
                        yield return "[global::Basalt.CommandLine.Annotations.CliCommandSymbol(global::Basalt.CommandLine.Annotations.CliCommandSymbolType.FromGlobalOption)]";
                        yield return $"public {property.PropertySymbol.Type.ToFullyQualifiedFormatString()} {property.PropertySymbol.Name} {{ get; init; }}";
                    }
                }
            }

            if (cliCommandBuilderType.OptionsTypeSymbol == null)
                return string.Empty;
            var properties = GenerateFromAncestors(cliCommandBuilderType);

            return @$"
#nullable disable
partial class {cliCommandBuilderType.OptionsTypeSymbol.Name}
{{
    {string.Join("\r\n    ", properties)}
}}
#nullable restore
";
        }

        string GenerateCommandLineSymbolProperties()
        {
            static string? GenerateCommandLineSymbolProperty(OptionsProperty property)
            {
                return property.CliCommandSymbolType switch
                {
                    CliCommandSymbolType.Option => $"private global::System.CommandLine.Option<{property.PropertySymbol.Type.ToFullyQualifiedFormatString()}> {property.PropertySymbol.Name}Option {{ get; }}",
                    CliCommandSymbolType.GlobalOption => $"internal global::System.CommandLine.Option<{property.PropertySymbol.Type.ToFullyQualifiedFormatString()}> {property.PropertySymbol.Name}Option {{ get; }}",
                    CliCommandSymbolType.Argument => $"private global::System.CommandLine.Argument<{property.PropertySymbol.Type.ToFullyQualifiedFormatString()}> {property.PropertySymbol.Name}Argument {{ get; }}",
                    CliCommandSymbolType.FromGlobalOption => null,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            if (cliCommandBuilderType.OptionsTypeSymbol == null)
                return string.Empty;

            var properties = cliCommandBuilderType.OptionsProperties
                .Select(x => GenerateCommandLineSymbolProperty(x))
                .Where(x => x != null)
                .Cast<string>();
            return string.Join("\r\n    ", properties);
        }

        string GenerateEnrichCommand()
        {
            static string? GenerateOptionsPropertyEnrich(OptionsProperty property)
            {
                return property.CliCommandSymbolType switch
                {
                    CliCommandSymbolType.Option => $"command.AddOption({property.PropertySymbol.Name}Option);",
                    CliCommandSymbolType.GlobalOption => $"command.AddGlobalOption({property.PropertySymbol.Name}Option);",
                    CliCommandSymbolType.Argument => $"command.AddArgument({property.PropertySymbol.Name}Argument);",
                    CliCommandSymbolType.FromGlobalOption => null,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            if (cliCommandBuilderType.OptionsTypeSymbol == null)
                return string.Empty;

            var properties = cliCommandBuilderType.OptionsProperties
                .Select(x => GenerateOptionsPropertyEnrich(x))
                .Where(x => x != null)
                .Cast<string>();
            return string.Join("\r\n        ", properties);
        }

        string GenerateOptionPropertiesInit()
        {
            static IEnumerable<string> OptionPropertiesInit(CliCommandBuilderType builder)
            {
                foreach (OptionsProperty property in builder.OptionsProperties)
                {
                    string name = property.PropertySymbol.Name;
                    string? result = property.CliCommandSymbolType switch
                    {
                        CliCommandSymbolType.Option => $"{name} = parseResult.GetValueForOption(this.{name}Option)",
                        CliCommandSymbolType.GlobalOption => $"{name} = parseResult.GetValueForOption(this.{name}Option)",
                        CliCommandSymbolType.Argument => $"{name} = parseResult.GetValueForArgument(this.{name}Argument)",
                        _ => null,
                    };
                    if (result != null)
                        yield return result;
                }
            }

            static IEnumerable<string> GenerateOptionPropertiesFromAncestors(CliCommandBuilderType builder)
            {
                while (builder.Parent != null)
                {
                    foreach (OptionsProperty property in builder.Parent.OptionsProperties.Where(x => x.CliCommandSymbolType == CliCommandSymbolType.GlobalOption))
                    {
                        string name = property.PropertySymbol.Name;
                        yield return $"{name} = parseResult.GetValueForOption(this.GetRequiredParentBuilder<{property.CliCommandBuildTypeSymbol.CliCommandBuilderTypeSymbol.ToFullyQualifiedFormatString()}>().{name}Option)";
                    }

                    builder = builder.Parent;
                }
            }

            IEnumerable<string> properties = OptionPropertiesInit(cliCommandBuilderType);
            IEnumerable<string> propertiesFromAncestors = GenerateOptionPropertiesFromAncestors(cliCommandBuilderType);
            return string.Join(",\r\n                ", propertiesFromAncestors.Concat(properties));
        }

        string GenerateCommandHandler()
        {
            if (cliCommandBuilderType.CommandTypeSymbol == null)
                return string.Empty;

            return $@"
        command.SetHandler(async (global::System.CommandLine.Invocation.InvocationContext invocationContext) =>
        {{
            global::System.CommandLine.Parsing.ParseResult parseResult = invocationContext.BindingContext.ParseResult;

            {cliCommandBuilderType.OptionsTypeSymbol!.ToFullyQualifiedFormatString()} options = new()
            {{
                {GenerateOptionPropertiesInit()}
            }};

            await using global::Microsoft.Extensions.DependencyInjection.AsyncServiceScope scope = this.ServiceProvider.CreateAsyncScope();

            global::Basalt.CommandLine.CommandContext context = scope.ServiceProvider.GetRequiredService<global::Basalt.CommandLine.CommandContext>();
            context.InvocationContext = invocationContext;
            context.Options = options;

            {cliCommandBuilderType.CommandTypeSymbol.ToFullyQualifiedFormatString()} command =
                scope.ServiceProvider.GetRequiredService<{cliCommandBuilderType.CommandTypeSymbol.ToFullyQualifiedFormatString()}>();

            await command.Execute();
        }});";
        }

        string code = @$"
using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace {cliCommandBuilderType.CliCommandBuilderTypeSymbol.ContainingNamespace};

#nullable enable

{GenerateOptionsClass(cliCommandBuilderType)}

partial class {cliCommandBuilderType.CliCommandBuilderTypeSymbol.Name}
{{
    protected string Description {{ get; }}

    {GenerateCommandLineSymbolProperties()}

    protected override global::System.CommandLine.Command BuildCliCommandCore()
    {{
        string name = this.GetCliCommandBuilderAttribute().Name;
        global::System.CommandLine.Command command = this.CreateCliCommand(name, this.Description);

        {GenerateEnrichCommand()}
        {GenerateCommandHandler()}

        this.OnCommandLineBuilt(command);
        return command;
    }}

    partial void OnCommandLineBuilt(Command command);
}}
#nullable restore
            ";

        string fileName = cliCommandBuilderType.CliCommandBuilderTypeSymbol.ToDisplayString(new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
        context.AddSource($"{fileName}.g.cs", code);
    }


    [DebuggerDisplay("{CliCommandBuilderTypeSymbol} - <{CommandTypeSymbol?.Name}, {OptionsTypeSymbol?.Name}>")]
    private class CliCommandBuilderType
    {
        public CliCommandBuilderType(INamedTypeSymbol cliCommandBuilderTypeSymbol, INamedTypeSymbol? cliCommandBuilderBaseTypeSymbol = null)
        {
            this.CliCommandBuilderTypeSymbol = cliCommandBuilderTypeSymbol;

            (this.CommandTypeSymbol, this.OptionsTypeSymbol) = GetCommandAndOptionsType(cliCommandBuilderTypeSymbol, cliCommandBuilderBaseTypeSymbol);
            this.OptionsProperties = this.GetSettableOptionsPropertySymbols();
            this.Parent = this.GetParentCommandBuilder();
        }

        public INamedTypeSymbol CliCommandBuilderTypeSymbol { get; }
        public INamedTypeSymbol? CommandTypeSymbol { get; }
        public INamedTypeSymbol? OptionsTypeSymbol { get; }
        public IEnumerable<OptionsProperty> OptionsProperties { get; }
        public CliCommandBuilderType? Parent { get; }

        public static INamedTypeSymbol? GetCommandBuilderBaseType(INamedTypeSymbol typeSymbol)
        {
            while (true)
            {
                INamedTypeSymbol? baseTypeSymbol = typeSymbol.BaseType;
                if (baseTypeSymbol == null)
                    return null;

                if (baseTypeSymbol is { Name: "CliCommandBuilder", ContainingNamespace: { Name: "CommandLine", ContainingNamespace: { Name: "Basalt" } } })
                    return baseTypeSymbol;

                typeSymbol = baseTypeSymbol;
            }
        }

        private static (INamedTypeSymbol?, INamedTypeSymbol?) GetCommandAndOptionsType(INamedTypeSymbol cliCommandBuilderTypeSymbol, INamedTypeSymbol? cliCommandBuilderBaseTypeSymbol = null)
        {
            INamedTypeSymbol? baseTypeSymbol = cliCommandBuilderBaseTypeSymbol ?? GetCommandBuilderBaseType(cliCommandBuilderTypeSymbol);
            if (baseTypeSymbol == null)
                throw new ArgumentException(nameof(cliCommandBuilderTypeSymbol));

            var templateArguments = baseTypeSymbol.GetTypeParameterSymbols();

            INamedTypeSymbol? commandSymbol = templateArguments.TryGetValue("TCommand", out var cmdSymbol) ? cmdSymbol : null;
            INamedTypeSymbol? optionsSymbol = templateArguments.TryGetValue("TOptions", out var optSymbol) ? optSymbol : null;

            return (commandSymbol, optionsSymbol);
        }

        private IEnumerable<OptionsProperty> GetSettableOptionsPropertySymbols()
        {
            if (this.OptionsTypeSymbol == null)
                return Enumerable.Empty<OptionsProperty>();

            return this.OptionsTypeSymbol.GetMembers()
                .Where(x => x is IPropertySymbol)
                .Cast<IPropertySymbol>()
                .Where(x => x.SetMethod != null)
                .Select(x =>
                {
                    AttributeData? attributeData = x.GetAttributes()
                        .Where(x =>
                        {
                            if (x.AttributeClass == null) return false;
                            if (x.AttributeClass.ToString() != typeof(CliCommandSymbolAttribute).ToString())
                                return false;
                            return true;
                        })
                        .FirstOrDefault();

                    CliCommandSymbolType commandLineSymbolType = CliCommandSymbolType.Option;
                    if (attributeData?.ConstructorArguments.Any() ?? false)
                    {
                        object? value = attributeData.ConstructorArguments.First().Value;
                        if (value != null)
                            commandLineSymbolType = (CliCommandSymbolType)value;
                    }

                    return new OptionsProperty(this, x, commandLineSymbolType);
                })
                .ToList();
        }

        private CliCommandBuilderType? GetParentCommandBuilder()
        {
            AttributeData? attributeData = this.CliCommandBuilderTypeSymbol.GetAttributes()
                .FirstOrDefault(x => x.AttributeClass?.ToString() == typeof(CliCommandBuilderAttribute).FullName);
            INamedTypeSymbol? parentCommandBuilder = attributeData?.ConstructorArguments[1].Value as INamedTypeSymbol;
            return parentCommandBuilder == null ? null : new CliCommandBuilderType(parentCommandBuilder);
        }
    }

    [DebuggerDisplay("Property = {PropertySymbol}, CommandLineSymbolType = {CliCommandSymbolType}")]
    private class OptionsProperty
    {
        public OptionsProperty(CliCommandBuilderType cliCommandBuildTypeSymbol, IPropertySymbol propertySymbol, CliCommandSymbolType commandLineSymbolType)
        {
            this.CliCommandBuildTypeSymbol = cliCommandBuildTypeSymbol;
            this.PropertySymbol = propertySymbol;
            this.CliCommandSymbolType = commandLineSymbolType;
        }

        public CliCommandBuilderType CliCommandBuildTypeSymbol { get; }
        public IPropertySymbol PropertySymbol { get; }
        public CliCommandSymbolType CliCommandSymbolType { get; }
    }
}
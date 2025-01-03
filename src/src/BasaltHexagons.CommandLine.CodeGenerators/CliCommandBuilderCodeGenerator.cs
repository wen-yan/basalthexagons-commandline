using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BasaltHexagons.CommandLine.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BasaltHexagons.CommandLine.CodeGenerators;

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

        LanguageVersion languageVersion = ((CSharpParseOptions)context.TargetNode.SyntaxTree.Options).LanguageVersion;
        NullableContextOptions nullableOptions = context.SemanticModel.Compilation.Options.NullableContextOptions;

        try
        {
            return new CliCommandBuilderType(languageVersion, nullableOptions, commandBuilderTypeSymbol, baseTypeSymbol);
        }
        catch
        {
            return null;
        }
    }

    private static void GenerateOutput(SourceProductionContext context, CliCommandBuilderType cliCommandBuilderType)
    {
        if (cliCommandBuilderType.LanguageVersion < LanguageVersion.CSharp5)
        {
            AddSource(context, cliCommandBuilderType.CliCommandBuilderTypeSymbol, "#error BasaltHexagons.CommandLine doesn't support csharp version < 5");
        }
        else
        {
            string code = $"""
                           {GenerateOptionsClass(context, cliCommandBuilderType)}
                           {GenerateBuilderClass(context, cliCommandBuilderType)}
                           """;

            AddSource(context, cliCommandBuilderType.CliCommandBuilderTypeSymbol, code);
        }
    }

    private static string GenerateOptionsClass(SourceProductionContext context, CliCommandBuilderType cliCommandBuilderType)
    {
        static IEnumerable<(CliCommandBuilderType CliCommandBuilderType, OptionsProperty OptionsProperty)> GetPropertiesFromAncestors(CliCommandBuilderType cliCommandBuilderType)
        {
            while (cliCommandBuilderType.Parent != null)
            {
                cliCommandBuilderType = cliCommandBuilderType.Parent;
                foreach (OptionsProperty property in cliCommandBuilderType.OptionsProperties.Where(x => x.CliCommandSymbolType == CliCommandSymbolType.GlobalOption))
                    yield return (cliCommandBuilderType, property);
            }
        }

        static IEnumerable<string> GeneratePropertiesFromAncestors(LanguageVersion languageVersion, IEnumerable<(CliCommandBuilderType, OptionsProperty)> properties)
        {
            foreach ((CliCommandBuilderType cliCommandBuilderType, OptionsProperty property) in properties)
            {
                yield return $"// from {cliCommandBuilderType.CliCommandBuilderTypeSymbol}";
                yield return "[global::BasaltHexagons.CommandLine.Annotations.CliCommandSymbol(global::BasaltHexagons.CommandLine.Annotations.CliCommandSymbolType.FromGlobalOption)]";
                yield return $"public {property.PropertySymbol.Type.ToFullyQualifiedFormatString(property.PropertySymbol.NullableAnnotation)} {property.PropertySymbol.Name} {{ get; }}";
            }
        }

        if (cliCommandBuilderType.OptionsTypeSymbol == null)
            return string.Empty;

        List<(CliCommandBuilderType CliCommandBuilderType, OptionsProperty OptionsProperty)> optionsPropertiesFromAncestors = GetPropertiesFromAncestors(cliCommandBuilderType).ToList();
        List<OptionsProperty> optionsAllProperties = cliCommandBuilderType.OptionsProperties.Concat(optionsPropertiesFromAncestors.Select(x => x.OptionsProperty)).ToList();

        string argumentsStr = string.Join(",\r\n", optionsAllProperties
            .Select(property => $"{property.PropertySymbol.Type.ToFullyQualifiedFormatString(property.PropertySymbol.NullableAnnotation)} {property.PropertySymbol.Name}"));

        string propertyListInitStr = string.Join("\r\n", optionsAllProperties
            .Select(property => $"this.{property.PropertySymbol.Name} = {property.PropertySymbol.Name};"));

        string propertyListStr = string.Join("\r\n", GeneratePropertiesFromAncestors(cliCommandBuilderType.LanguageVersion, optionsPropertiesFromAncestors));

        return $$"""
                 namespace {{cliCommandBuilderType.OptionsTypeSymbol.ContainingNamespace}}
                 {
                     partial class {{cliCommandBuilderType.OptionsTypeSymbol.Name}}
                     {
                         public {{cliCommandBuilderType.OptionsTypeSymbol.Name}}(
                             {{argumentsStr}}
                             )
                         {
                             {{propertyListInitStr}}
                         }
                         {{propertyListStr}}
                     }
                 }
                 """;
    }

    private static string GenerateBuilderClass(SourceProductionContext context, CliCommandBuilderType cliCommandBuilderType)
    {
        static string GenerateCommandLineSymbolProperties(CliCommandBuilderType cliCommandBuilderType)
        {
            if (cliCommandBuilderType.OptionsTypeSymbol == null)
                return string.Empty;

            var properties = cliCommandBuilderType.OptionsProperties
                .Select(property => property.CliCommandSymbolType switch
                {
                    CliCommandSymbolType.Option => GenerateReadonlyProperty(cliCommandBuilderType.LanguageVersion, "private",
                        $"global::System.CommandLine.Option<{property.PropertySymbol.Type.ToFullyQualifiedFormatString(property.PropertySymbol.NullableAnnotation)}>", $"{property.PropertySymbol.Name}Option"),

                    CliCommandSymbolType.GlobalOption => GenerateReadonlyProperty(cliCommandBuilderType.LanguageVersion, "internal",
                        $"global::System.CommandLine.Option<{property.PropertySymbol.Type.ToFullyQualifiedFormatString(property.PropertySymbol.NullableAnnotation)}>", $"{property.PropertySymbol.Name}Option"),

                    CliCommandSymbolType.Argument => GenerateReadonlyProperty(cliCommandBuilderType.LanguageVersion, "private",
                        $"global::System.CommandLine.Argument<{property.PropertySymbol.Type.ToFullyQualifiedFormatString(property.PropertySymbol.NullableAnnotation)}>", $"{property.PropertySymbol.Name}Argument"),

                    CliCommandSymbolType.FromGlobalOption => null,
                    _ => throw new ArgumentOutOfRangeException()
                })
                .Where(x => x != null)
                .Cast<string>();
            return string.Join("\r\n", properties);
        }

        static string GenerateEnrichCommand(CliCommandBuilderType cliCommandBuilderType)
        {
            if (cliCommandBuilderType.OptionsTypeSymbol == null)
                return string.Empty;

            var properties = cliCommandBuilderType.OptionsProperties
                .Select(property => property.CliCommandSymbolType switch
                {
                    CliCommandSymbolType.Option => $"cliCommand.AddOption({property.PropertySymbol.Name}Option);",
                    CliCommandSymbolType.GlobalOption => $"cliCommand.AddGlobalOption({property.PropertySymbol.Name}Option);",
                    CliCommandSymbolType.Argument => $"cliCommand.AddArgument({property.PropertySymbol.Name}Argument);",
                    CliCommandSymbolType.FromGlobalOption => null,
                    _ => throw new ArgumentOutOfRangeException()
                })
                .Where(x => x != null)
                .Cast<string>();
            return string.Join("\r\n", properties);
        }

        static string GenerateCommandHandler(CliCommandBuilderType cliCommandBuilderType)
        {
            string GenerateOptionPropertiesInit()
            {
                static IEnumerable<string> OptionPropertiesInit(CliCommandBuilderType builder)
                {
                    foreach (OptionsProperty property in builder.OptionsProperties)
                    {
                        string name = property.PropertySymbol.Name;
                        string? result = property.CliCommandSymbolType switch
                        {
                            CliCommandSymbolType.Option => $"{name}: parseResult.GetValueForOption(this.{name}Option)",
                            CliCommandSymbolType.GlobalOption => $"{name}: parseResult.GetValueForOption(this.{name}Option)",
                            CliCommandSymbolType.Argument => $"{name}: parseResult.GetValueForArgument(this.{name}Argument)",
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
                            yield return
                                $"{name}: parseResult.GetValueForOption(this.GetRequiredParentBuilder<{property.CliCommandBuildTypeSymbol.CliCommandBuilderTypeSymbol.ToFullyQualifiedFormatString(NullableAnnotation.None)}>().{name}Option)";
                        }

                        builder = builder.Parent;
                    }
                }

                IEnumerable<string> properties = OptionPropertiesInit(cliCommandBuilderType);
                IEnumerable<string> propertiesFromAncestors = GenerateOptionPropertiesFromAncestors(cliCommandBuilderType);
                return string.Join(",\r\n                ", propertiesFromAncestors.Concat(properties));
            }

            if (cliCommandBuilderType.CommandTypeSymbol == null)
                return string.Empty;

            return $$"""
                     cliCommand.SetHandler(async (global::System.CommandLine.Invocation.InvocationContext invocationContext) =>
                     {
                         global::System.CommandLine.Parsing.ParseResult parseResult = invocationContext.BindingContext.ParseResult;
                     
                         {{cliCommandBuilderType.OptionsTypeSymbol!.ToFullyQualifiedFormatString(NullableAnnotation.None)}} options = new {{cliCommandBuilderType.OptionsTypeSymbol!.ToFullyQualifiedFormatString(NullableAnnotation.None)}}(
                            {{GenerateOptionPropertiesInit()}}
                         );
                     
                         {{(cliCommandBuilderType.LanguageVersion.AsyncUsingSupported() ? "await" : string.Empty)}} using (global::Microsoft.Extensions.DependencyInjection.AsyncServiceScope scope = this.ServiceProvider.CreateAsyncScope())
                         {
                             global::BasaltHexagons.CommandLine.CommandContext context = scope.ServiceProvider.GetRequiredService<global::BasaltHexagons.CommandLine.CommandContext>();
                             context.InvocationContext = invocationContext;
                             context.Options = options;
                         
                             {{cliCommandBuilderType.CommandTypeSymbol.ToFullyQualifiedFormatString(NullableAnnotation.None)}} command =
                                 scope.ServiceProvider.GetRequiredService<{{cliCommandBuilderType.CommandTypeSymbol.ToFullyQualifiedFormatString(NullableAnnotation.None)}}>();
                         
                             await command.ExecuteAsync();
                         }
                     });
                     """;
        }

        return $$"""
                 namespace {{cliCommandBuilderType.CliCommandBuilderTypeSymbol.ContainingNamespace}}
                 {
                     using System.CommandLine;
                     using Microsoft.Extensions.DependencyInjection;
                 
                     partial class {{cliCommandBuilderType.CliCommandBuilderTypeSymbol.Name}}
                     {
                         {{GenerateReadonlyProperty(cliCommandBuilderType.LanguageVersion, "protected", "string", "Description")}}
                 
                         {{GenerateCommandLineSymbolProperties(cliCommandBuilderType)}}
                 
                         protected override global::System.CommandLine.Command BuildCliCommandCore()
                         {
                             string name = this.GetCliCommandBuilderAttribute().Name;
                             global::System.CommandLine.Command cliCommand = this.CreateCliCommand(name, this.Description);
                 
                             {{GenerateEnrichCommand(cliCommandBuilderType)}}
                 
                             {{GenerateCommandHandler(cliCommandBuilderType)}}
                 
                             this.OnCommandLineBuilt(cliCommand);
                             return cliCommand;
                         }
                 
                         partial void OnCommandLineBuilt(Command command);
                     }
                 }
                 """;
    }

    private static string GenerateReadonlyProperty(LanguageVersion languageVersion, string access, string type, string name)
    {
        return languageVersion.ReadonlyAutomaticallyImplementedPropertiesSupported()
            ? $"{access} {type} {name} {{ get; }}"
            : access == "private"
                ? $"{access} {type} {name} {{ get; set; }}"
                : $"{access} {type} {name} {{ get; private set; }}";
    }

    private static void AddSource(SourceProductionContext context, string fileName, string code)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        SourceText formatedCode = tree.GetRoot(default).NormalizeWhitespace().GetText();
        string formattedCode = formatedCode.ToString();

        context.AddSource($"{fileName}.g.cs", formattedCode);
    }

    private static void AddSource(SourceProductionContext context, INamedTypeSymbol typeSymbol, string code)
    {
        string fileName = typeSymbol.ToDisplayString(new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
        AddSource(context, fileName, code);
    }

    [DebuggerDisplay("{CliCommandBuilderTypeSymbol} - <{CommandTypeSymbol?.Name}, {OptionsTypeSymbol?.Name}>")]
    private class CliCommandBuilderType
    {
        public CliCommandBuilderType(LanguageVersion languageVersion, NullableContextOptions nullableOptions, INamedTypeSymbol cliCommandBuilderTypeSymbol, INamedTypeSymbol? cliCommandBuilderBaseTypeSymbol = null)
        {
            this.LanguageVersion = languageVersion;
            this.NullableContextOptions = nullableOptions;
            this.CliCommandBuilderTypeSymbol = cliCommandBuilderTypeSymbol;

            (this.CommandTypeSymbol, this.OptionsTypeSymbol) = GetCommandAndOptionsType(cliCommandBuilderTypeSymbol, cliCommandBuilderBaseTypeSymbol);
            this.OptionsProperties = this.GetOptionsPropertySymbols();
            this.Parent = this.GetParentCommandBuilder();
        }

        public LanguageVersion LanguageVersion { get; }
        public NullableContextOptions NullableContextOptions { get; set; }
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

                if (baseTypeSymbol is { Name: "CliCommandBuilder", ContainingNamespace: { Name: "CommandLine", ContainingNamespace.Name: "BasaltHexagons" } })
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

        private IEnumerable<OptionsProperty> GetOptionsPropertySymbols()
        {
            if (this.OptionsTypeSymbol == null)
                return Enumerable.Empty<OptionsProperty>();

            return this.OptionsTypeSymbol.GetMembers()
                .Where(x => x is IPropertySymbol)
                .Cast<IPropertySymbol>()
                .Select(x =>
                {
                    AttributeData? attributeData = x.GetAttributes()
                        .FirstOrDefault(y => y.AttributeClass?.ToString() == typeof(CliCommandSymbolAttribute).ToString());

                    CliCommandSymbolType? commandLineSymbolType = null;
                    if (attributeData?.ConstructorArguments.Any() ?? false)
                    {
                        object? value = attributeData.ConstructorArguments.First().Value;
                        if (value != null)
                            commandLineSymbolType = (CliCommandSymbolType)value;
                    }

                    return commandLineSymbolType == null ? null : new OptionsProperty(this, x, commandLineSymbolType.Value);
                })
                .Where(x => x != null)
                .Cast<OptionsProperty>()
                .ToList();
        }

        private CliCommandBuilderType? GetParentCommandBuilder()
        {
            AttributeData? attributeData = this.CliCommandBuilderTypeSymbol.GetAttributes()
                .FirstOrDefault(x => x.AttributeClass?.ToString() == typeof(CliCommandBuilderAttribute).FullName);

            if ((attributeData?.ConstructorArguments.Length ?? 0) < 2)
                return null;
            INamedTypeSymbol? parentCommandBuilder = attributeData?.ConstructorArguments[1].Value as INamedTypeSymbol;
            return parentCommandBuilder == null ? null : new CliCommandBuilderType(this.LanguageVersion, this.NullableContextOptions, parentCommandBuilder);
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
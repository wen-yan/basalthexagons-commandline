using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace BasaltHexagons.CommandLine.CodeGenerators;

internal static class SymbolExtensions
{
    public static IReadOnlyDictionary<string, INamedTypeSymbol> GetTypeParameterSymbols(this INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.TypeParameters.Select((x, i) => (x.Name, Index: i))
            .Join(typeSymbol.TypeArguments.Select((x, i) => (Type: x, Index: i)), x => x.Index, x => x.Index, (x, y) => (x.Name, Type: y.Type as INamedTypeSymbol))
            .Where(x => x.Type is not null)
            .Select(x => (x.Name, Type: x.Type!))
            .ToDictionary(x => x.Name, x => x.Type);
    }

    public static string ToFullyQualifiedFormatString(this ISymbol symbol) => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
}
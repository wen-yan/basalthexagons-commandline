using Microsoft.CodeAnalysis.CSharp;

namespace BasaltHexagons.CommandLine.CodeGenerators;

internal static class LanguageVersionExtensions
{
    public static bool ReadonlyAutomaticallyImplementedPropertiesSupported(this LanguageVersion languageVersion)
    {
        // error CS8026: Feature 'readonly automatically implemented properties' is not available in C# 5. Please use language version 6 or greater.
        return languageVersion >= LanguageVersion.CSharp6;
    }

    public static bool UsingDeclarationsSupported(this LanguageVersion languageVersion)
    {
        // error CS8059: Feature 'using declarations' is not available in C# 6. Please use language version 8.0 or greater.
        return languageVersion >= LanguageVersion.CSharp8;
    }

    public static bool AsyncUsingSupported(this LanguageVersion languageVersion)
    {
        // error CS8026: Feature 'asynchronous using' is not available in C# 5. Please use language version 8.0 or greater.
        return languageVersion >= LanguageVersion.CSharp8;
    }
}
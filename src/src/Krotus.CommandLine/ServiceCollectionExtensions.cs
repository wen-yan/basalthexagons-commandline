using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Reflection;
using Krotus.CommandLine.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Krotus.CommandLine;

/// <summary>
/// Utility class for Krotus.CommandLine dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register required components to use Krotus.CommandLine
    /// </summary>
    /// <param name="serviceCollection">IServiceCollection object</param>
    /// <param name="rootCommandName">Root cli command name, default is `CliCommandBuilderAttribute.DefaultRootCommandName`</param>
    /// <param name="assembly">Assembly in which search cli command builders and commands</param>
    /// <returns>IServiceCollection object</returns>
    public static IServiceCollection AddCommandLineSupport(this IServiceCollection serviceCollection, string rootCommandName = CliCommandBuilderAttribute.DefaultRootCommandName, Assembly? assembly = null)
    {
        return serviceCollection
            .AddScoped<CommandContext>()
            .AddCliCommandBuilders(assembly)
            .AddCommands(assembly)
            .AddRootCliCommand(rootCommandName);
    }

    private static IServiceCollection AddCliCommandBuilders(this IServiceCollection serviceCollection, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetEntryAssembly() ?? throw new ApplicationException("Can't get entry assembly");
        IEnumerable<Type> commandBuilderTypes = assembly.GetTypes()
                .Where(x => x is { IsClass : true, IsAbstract: false, IsGenericType: false })
                .Where(x => x.GetInterfaces().Contains(typeof(ICliCommandBuilder)))
            ;

        foreach (Type type in commandBuilderTypes)
        {
            CliCommandBuilderAttribute? attribute = CliCommandBuilderAttribute.GetFromType(type);
            if (attribute == null)
                continue;

            // command builders
            string key = attribute.CommandKey;
            serviceCollection.AddKeyedSingleton(typeof(ICliCommandBuilder), key, type);
            serviceCollection.AddSingleton<ICliCommandBuilder>(serviceProvider => serviceProvider.GetRequiredKeyedService<ICliCommandBuilder>(key));
        }

        return serviceCollection;
    }

    private static IServiceCollection AddCommands(this IServiceCollection serviceCollection, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetEntryAssembly() ?? throw new ApplicationException("Can't get entry assembly");
        IEnumerable<Type> commandTypes = assembly.GetTypes()
                .Where(x => x is { IsClass : true, IsAbstract: false, IsGenericType: false })
                .Where(x => x.GetInterfaces().Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == typeof(ICommand<>)))
            ;

        foreach (Type type in commandTypes)
        {
            serviceCollection.AddScoped(type);
        }

        return serviceCollection;
    }

    private static IServiceCollection AddRootCliCommand(this IServiceCollection serviceCollection, string rootCommandName)
    {
        serviceCollection.AddSingleton<RootCommand>(serviceProvider =>
        {
            if (serviceProvider.GetKeyedService<ICliCommandBuilder>(rootCommandName)?.CliCommand is not RootCommand rootCommand)
                throw new ApplicationException("No root cli command");

            return rootCommand;
        });
        return serviceCollection;
    }
}
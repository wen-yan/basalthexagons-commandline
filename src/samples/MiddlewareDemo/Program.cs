using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Threading.Tasks;
using BasaltHexagons.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MiddlewareDemo;

static class Program
{
    static async Task<int> Main(string[] args)
    {
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);

        hostBuilder
            .ConfigureServices((context, services) =>
            {
                services
                    .AddCommandLineSupport()
                    .SetupCommandLineMiddleware();
            });

        IHost host = hostBuilder.Build();
        Parser parser = host.Services.GetRequiredService<Parser>();
        return await parser.InvokeAsync(args);
    }

    private static IServiceCollection SetupCommandLineMiddleware(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<Parser>(serviceProvider =>
        {
            RootCommand rootCommand = serviceProvider.GetRequiredService<RootCommand>();
            CommandLineBuilder commandLineBuilder = new(rootCommand);

            commandLineBuilder.AddMiddleware(async static (context, next) =>
            {
                Stopwatch stopWatch = new();
                stopWatch.Start();

                try
                {
                    await next(context);
                }
                finally
                {
                    stopWatch.Stop();
                    await Console.Out.WriteLineAsync($"Command finished in {stopWatch.ElapsedMilliseconds}ms");
                }
            });
            commandLineBuilder.UseDefaults();
            Parser parser = commandLineBuilder.Build();
            return parser;
        });
    }
}
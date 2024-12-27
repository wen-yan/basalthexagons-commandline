using System.CommandLine;
using System.Threading.Tasks;
using BasaltHexagons.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SimpleUsage;

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
                    .AddSingleton<IDemoService, DemoService>();
            });

        IHost host = hostBuilder.Build();
        RootCommand rootCommand = host.Services.GetRequiredService<RootCommand>();
        return await rootCommand.InvokeAsync(args);
    }
}
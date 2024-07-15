using System;
using System.Threading.Tasks;

namespace SimpleUsage;

interface IDemoService
{
    ValueTask WriteLineAsync(string message);
}

class DemoService : IDemoService
{
    public async ValueTask WriteLineAsync(string message)
    {
        await Console.Out.WriteLineAsync(message);
    }
}
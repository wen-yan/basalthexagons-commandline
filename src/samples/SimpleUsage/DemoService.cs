using System;
using System.Threading.Tasks;

namespace SimpleUsage;

interface IDemoService
{
    ValueTask WriteLine(string message);
}

class DemoService : IDemoService
{
    public async ValueTask WriteLine(string message)
    {
        await Console.Out.WriteLineAsync(message);
    }
}
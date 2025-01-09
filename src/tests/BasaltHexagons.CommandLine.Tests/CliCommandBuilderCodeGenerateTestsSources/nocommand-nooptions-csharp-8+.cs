using System;
using System.Threading.Tasks;
using BasaltHexagons.CommandLine;
using BasaltHexagons.CommandLine.Annotations;

namespace MyNamespace
{
    [CliCommandBuilder("fs", null)]
    partial class FsCommandBuilder : CliCommandBuilder
    {
        public FsCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider) {}
    }
}
using DragonLib.CommandLine;
using Scarlet;
using Scarlet.Structures.Id;
using Serilog;

namespace Witch;

internal class Program {
    private static void Main(string[] args) {
        var x = FixIdRegistry.IdTable;

        Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Console()
                    .CreateLogger();

        using var perf = new PerformanceCounter<Program>();
        Command.Run(out _, out _);
        perf.Stop();

        IPerformanceCounter.PrintAll();
    }
}

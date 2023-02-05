using DragonLib.CommandLine;
using Scarlet;
using Serilog;

namespace Witch;

internal class Program {
    private static void Main(string[] args) {
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

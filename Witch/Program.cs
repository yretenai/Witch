using Scarlet.Archive;
using Serilog;

namespace Witch;

internal class Program {
    private static void Main(string[] args) {
        if (args.Length < 1) {
            Console.WriteLine("Usage: Witch.exe <path to game>");
            return;
        }

        Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Console()
                    .CreateLogger();

        foreach (var arg in args) {
            var earcFiles = Directory.GetFiles(Path.Combine(arg, "datas"), "*.earc", SearchOption.AllDirectories);

            foreach (var earcFile in earcFiles) {
                using var earc = new EARC(earcFile);

                foreach (var file in earc.FileEntries) {
                    Log.Information(file.GetPath(earc.Buffer));
                }
            }
        }
    }
}

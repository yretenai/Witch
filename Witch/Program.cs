using DragonLib;
using Scarlet;
using Scarlet.Archive;
using Scarlet.Structures.Archive;
using Serilog;

namespace Witch;

internal class Program {
    private static void Main(string[] args) {
        if (args.Length < 2) {
            Console.WriteLine("Usage: Witch.exe <path to game> <path to output>");
            return;
        }

        Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Console()
                    .CreateLogger();

        var earcFiles = Directory.GetFiles(Path.Combine(args[0], "datas"), "*.earc", SearchOption.AllDirectories);
        var target = new DirectoryInfo(args[1]);
        using var _perf = new PerformanceCounter(args[1]);
        foreach (var earcFile in earcFiles) {
            using var earc = new EARC(earcFile);

            foreach (var file in earc.FileEntries) {
                if ((file.Flags & EARCFileFlags.Reference) != 0) {
                    continue;
                }

                if ((file.Flags & EARCFileFlags.Deleted) != 0) {
                    continue;
                }

                if ((file.Flags & EARCFileFlags.Loose) != 0) {
                    continue;
                }

                var path = file.GetPath(earc.Buffer);
                var outputPath = Path.Combine(target.FullName, path);
                outputPath.EnsureDirectoryExists();

                Log.Information("Extracting {File}", path);

                using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                using var input = earc.ReadFile(file);
                output.Write(input.Span);
            }
        }
    }
}

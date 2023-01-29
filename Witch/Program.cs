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
        var ememFiles = Directory.GetFiles(Path.Combine(args[0], "datas"), "*.emem", SearchOption.AllDirectories);
        var files = new List<string>(earcFiles.Length + ememFiles.Length);
        files.AddRange(earcFiles);
        files.AddRange(ememFiles);

        var target = new DirectoryInfo(args[1]).FullName;
        using var _perf = new PerformanceCounter<Program>();
        foreach (var earcFile in files) {
            using var earc = new EARC(earcFile, earcFile.EndsWith(".emem", StringComparison.OrdinalIgnoreCase));

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
                if (path.StartsWith("$archives")) {
                    path = path[10..];
                }

                path = path.Trim('/', '\\');
                var outputPath = Path.Combine(target, path);
                outputPath.EnsureDirectoryExists();

                Log.Information("Extracting {File}", path);

                using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                using var input = earc.ReadFile(file);
                output.Write(input.Span);
            }
        }

        Log.Information("Done");
        _perf.Stop();

        IPerformanceCounter.PrintAll();
    }
}

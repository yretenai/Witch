using DragonLib;
using Scarlet;
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

        using var _perf = new PerformanceCounter<Program>();

        Log.Information("Loading {Count} archives", files.Count);
        foreach (var earcFile in files) {
            Log.Information("Loading {File}", earcFile);
            ResourceManager.Instance.LoadEARC(earcFile);
        }

        Log.Information("Building file table");
        ResourceManager.Instance.Build();

        // todo: make this a command
        Log.Information("Extracting files");
        var target = new DirectoryInfo(args[1]).FullName;
        foreach (ResourceManager.FileReference reference in ResourceManager.Instance.IdTable.Values) {
            var (earc, file) = reference.Deconstruct();

            var path = file.GetPath(earc.Buffer);
            if (path.StartsWith("$archives")) {
                path = path[10..];
            }

            path = path.Trim('/', '\\');
            var outputPath = target + '/' + path;
            outputPath.EnsureDirectoryExists();

            Log.Information("Extracting {File}", path);

            using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using var input = earc.ReadFile(file);
            output.Write(input.Span);
        }

        Log.Information("Done");
        _perf.Stop();

        IPerformanceCounter.PrintAll();
    }
}

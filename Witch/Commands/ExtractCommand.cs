using DragonLib;
using DragonLib.CommandLine;
using Scarlet;
using Serilog;

namespace Witch.Commands;

[Command(typeof(ExtractFlags), "extract", "Extracts the files in the EARC")]
public class ExtractCommand : EARCCommand {
    public ExtractCommand(ExtractFlags flags) : base(flags) {
        Log.Information("Extracting files");
        var target = new DirectoryInfo(flags.OutputDir).FullName;
        foreach (var reference in AssetManager.Instance.IdTable.Values) {
            var (archive, file) = reference.Deconstruct();

            var path = file.GetPath(archive.Buffer);
            if (path.StartsWith("$archives")) {
                path = path[10..];
            }

            path = path.Trim('/', '\\');
            var outputPath = target + '/' + path;
            outputPath.EnsureDirectoryExists();

            Log.Information("Extracting {File}", path);

            using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using var input = archive.Read(file);
            output.Write(input.Span);
        }
    }
}

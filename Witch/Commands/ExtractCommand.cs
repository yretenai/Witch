using System.Diagnostics;
using DragonLib.CommandLine;
using Scarlet;
using Scarlet.Structures;
using Scarlet.Structures.Archive;
using Serilog;

namespace Witch.Commands;

[Command(typeof(ExtractFlags), "extract", "Extracts the files in the EARC")]
public class ExtractCommand : EARCCommand {
    public ExtractCommand(ExtractFlags flags) : base(flags) {
        var target = new DirectoryInfo(flags.OutputDir).FullName;
        var archives = Path.Combine(flags.InstallDir, "datas");

        Log.Information("Building directory structure...");
        foreach (var reference in AssetManager.Instance.UriTable.Values.Where(reference => reference.Exists).Select(x => Path.GetDirectoryName(ScarletHelpers.StripPath(x.DataPath))!).Distinct()) {
            var path = reference;
            if (path == "$archives") {
                continue;
            }

            if (path.StartsWith("$archives")) {
                path = path[10..];
            }

            path = path.Trim('/', '\\');
            var outputPath = target + '/' + path;
            if (!Directory.Exists(outputPath)) {
                Directory.CreateDirectory(outputPath);
            }
        }

        foreach (var reference in AssetManager.Instance.UriTable.Values.Where(reference => reference.Exists).Select(x => Path.GetDirectoryName(x.File.GetPath(x.Archive.Buffer))!).Distinct()) {
            var path = reference;
            if (path == "$archives") {
                continue;
            }

            if (path.StartsWith("$archives")) {
                path = path[10..];
            }

            path = path.Trim('/', '\\');
            var outputPath = target + '/' + path;
            if (!Directory.Exists(outputPath)) {
                Directory.CreateDirectory(outputPath);
            }
        }

        var archiveLookup = new Dictionary<string, string>(AssetManager.Instance.UriTable.Count);
        Log.Information("Extracting...");
        foreach (var reference in AssetManager.Instance.UriTable.Values.Where(reference => reference.Exists)) {
            var (archive, file) = reference.Deconstruct();

            if (reference.File.Size == 0 || (reference.File.Flags & EbonyArchiveFileFlags.Reference) != 0) {
                continue;
            }

            if ((reference.File.Flags & EbonyArchiveFileFlags.Loose) != 0) {
                Debugger.Break();
            }

            if ((reference.File.Flags & EbonyArchiveFileFlags.Deleted) != 0) {
                Debugger.Break();
            }

            if ((reference.File.Flags & EbonyArchiveFileFlags.Patched) != 0) {
                Debugger.Break();
            }

            var path = file.GetPath(archive.Buffer);
            if (path.StartsWith("$archives")) {
                path = path[10..];
            }

            path = path.Trim('/', '\\', '@');
            var outputPath = target + '/' + path;
            archiveLookup[file.GetPath(archive.Buffer)] = outputPath;

            Log.Information("Extracting {File}", path);

            using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using var input = archive.Read(file);
            output.Write(input.Span);
        }

        Log.Information("Linking...");
        foreach (var reference in AssetManager.Instance.UriTable.Values.Where(reference => reference.Exists)) {
            var (archive, file) = reference.Deconstruct();

            if ((reference.File.Flags & EbonyArchiveFileFlags.Reference) == 0) {
                continue;
            }

            var archivePath = file.GetPath(archive.Buffer);
            if (!archiveLookup.TryGetValue(archivePath, out var linkTarget)) {
                if (archivePath.StartsWith("$archives")) {
                    linkTarget = archives + '/' + archivePath[10..];
                } else {
                    Debugger.Break();
                    continue;
                }
            }

            var path = ScarletHelpers.StripPath(reference.DataPath);
            path = path.Trim('/', '\\', '@');
            var extension = Path.GetFileName(linkTarget).Split('.', 2, StringSplitOptions.TrimEntries)[1];
            path = Path.GetDirectoryName(path) + "/" + Path.GetFileName(path).Split('.', 2, StringSplitOptions.TrimEntries)[0] + '.' + extension;
            path = path.Trim('/', '\\', '@');
            var outputPath = target + '/' + path;
            if (File.Exists(outputPath)) {
                continue;
            }

            Log.Information("Linking {File} to {Target}", path, archivePath);
            File.CreateSymbolicLink(outputPath, linkTarget);
        }
    }
}

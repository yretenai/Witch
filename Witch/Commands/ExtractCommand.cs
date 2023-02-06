﻿using System.Diagnostics;
using DragonLib;
using DragonLib.CommandLine;
using Scarlet;
using Scarlet.Structures;
using Scarlet.Structures.Archive;
using Serilog;

namespace Witch.Commands;

[Command(typeof(ExtractFlags), "extract", "Extracts the files in the EARC")]
public class ExtractCommand : EARCCommand {
    public ExtractCommand(ExtractFlags flags) : base(flags) {
        Log.Information("Extracting files");
        var target = new DirectoryInfo(flags.OutputDir).FullName;
        var archives = Path.Combine(flags.InstallDir, "datas");

        // pre build directory structure
        foreach (var reference in AssetManager.Instance.UriTable.Values.Where(reference => reference.Exists)) {
            var (archive, file) = reference.Deconstruct();
            if ((reference.File.Flags & EbonyArchiveFileFlags.Reference) != 0) {
                var path = ScarletHelpers.StripPath(reference.DataPath);
                path = path.Trim('/', '\\');
                var outputPath = target + '/' + path;
                outputPath.EnsureDirectoryExists();
            } else {
                var path = file.GetPath(archive.Buffer);
                if (path.StartsWith("$archives")) {
                    path = path[10..];
                }

                path = path.Trim('/', '\\');
                var outputPath = target + '/' + path;
                outputPath.EnsureDirectoryExists();
            }
        }

        // export real files
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

            path = path.Trim('/', '\\');
            var outputPath = target + '/' + path;

            Log.Information("Extracting {File}", path);

            using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using var input = archive.Read(file);
            output.Write(input.Span);
        }

        // make link hierarchy
        foreach (var reference in AssetManager.Instance.UriTable.Values.Where(reference => reference.Exists)) {
            var (archive, file) = reference.Deconstruct();

            if ((reference.File.Flags & EbonyArchiveFileFlags.Reference) == 0) {
                continue;
            }

            var archivePath = file.GetPath(archive.Buffer);
            var linkTarget = archivePath.StartsWith("$archives") ? archives + '/' + archivePath[10..] : target + '/' + archivePath;

            if (!File.Exists(linkTarget)) {
                Debugger.Break();
            }

            var path = ScarletHelpers.StripPath(reference.DataPath);
            path = path.Trim('/', '\\');
            var outputPath = target + '/' + path;
            Log.Information("Linking {File} to {Target}", path, archivePath);
            File.CreateSymbolicLink(outputPath, linkTarget);
        }
    }
}

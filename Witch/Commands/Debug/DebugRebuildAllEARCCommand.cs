using DragonLib.CommandLine;
using Scarlet;
using Scarlet.Archive;
using Serilog;

namespace Witch.Commands.Debug;

[Command(typeof(WitchFlags), "rebuild-all", "", "debug", true)]
public class DebugRebuildAllEARCCommand : EARCCommand {
    public DebugRebuildAllEARCCommand(WitchFlags flags) : base(flags) {
        foreach (var (_, earc) in AssetManager.Instance.Archives) {
            var builder = new EbonyArchiveBuilder(earc);
            Log.Information("Rebuilding {EARC}", earc.DataPath);
            using var output = new FileStream(Path.GetFileNameWithoutExtension(earc.DataPath) + ".earc", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            builder.Build(output);
        }
    }
}

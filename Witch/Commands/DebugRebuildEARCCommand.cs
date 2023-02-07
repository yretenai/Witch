using DragonLib.CommandLine;
using Scarlet;
using Scarlet.Archive;
using Scarlet.Structures;

namespace Witch.Commands;

[Command(typeof(WitchFlags), "rebuild-1", "", "debug", true)]
public class DebugRebuildEARCCommand : EARCCommand {
    public DebugRebuildEARCCommand(WitchFlags flags) : base(flags) {
        var testEarc = AssetManager.Instance.Archives[new AssetId("data://c000.ebex", TypeIdRegistry.EARC)];
        var builder = new EbonyArchiveBuilder(testEarc);
        using var output = new FileStream("test.earc", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        builder.Build(output);
        output.Seek(0, SeekOrigin.Begin);
        using var test = new EbonyArchive(testEarc.AssetId, testEarc.DataPath, output);
    }
}

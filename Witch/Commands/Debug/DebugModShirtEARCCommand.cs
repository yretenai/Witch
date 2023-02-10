using CommunityToolkit.HighPerformance.Buffers;
using DragonLib.CommandLine;
using Scarlet;
using Scarlet.Archive;
using Scarlet.Structures;
using Scarlet.Structures.Archive;
using Serilog;

namespace Witch.Commands.Debug;

[Command(typeof(WitchFlags), "mod-shirt", "", "debug", true)]
public class DebugModShirtEARCCommand : EARCCommand {
    public DebugModShirtEARCCommand(WitchFlags flags) : base(flags) {
        var testEarc = AssetManager.Instance.Archives[new AssetId("data://c000.ebex", TypeIdRegistry.EARC)];
        var builder = new EbonyArchiveBuilder(testEarc);

        var modId = new AssetId("data://mods.ebex@", TypeIdRegistry.EARC);
        builder.AddOrReplaceFile(new EbonyArchiveBuilder.RebuildRecord(modId, EbonyArchiveFileFlags.AutoLoad | EbonyArchiveFileFlags.Reference, "$archives/mods.earc", "data://mods.ebex@"));

        var erepId = new AssetId("data://c000.erep", TypeIdRegistry.EREP);
        var erepTargetId = new AssetId("data://c000.ebex", TypeIdRegistry.EARC);

        var modTexId = new AssetId("data://mods/test.btex", TypeIdRegistry.BTEX);
        AssetId.IdTable[modTexId] = "data://mods/test.btex";

        var erepBuilder = new EbonyReplaceBuilder(AssetManager.Instance.Replacements[erepTargetId]);
        foreach (var originalTexId in AssetId.IdTable.Where(x => x.Value.Contains("hu000_000_inner_shirts_1001_b", StringComparison.Ordinal)).Select(x => x.Key)) {
            erepBuilder.Replace(originalTexId, modTexId);
        }

        builder.ReplaceFile(erepId, erepBuilder.Build);

        using var output = new FileStream("c000.earc", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        builder.Build(output);

        var modEarc = new EbonyArchiveBuilder();
        modEarc.AddOrReplaceFile(new EbonyArchiveBuilder.RebuildRecord(modTexId, EbonyArchiveFileFlags.None, "mods/test.btex", "data://mods/test.btex") {
            DataDelegate = (_ => {
                                var bytes = File.ReadAllBytes(flags.Positionals[^1]);
                                var owner = MemoryOwner<byte>.Allocate(bytes.Length);
                                bytes.CopyTo(owner.Span);
                                return owner;
                            }),
        });

        using var modOutput = new FileStream("mods.earc", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        modEarc.Build(modOutput);
    }
}

using DragonLib.CommandLine;
using Scarlet;
using Scarlet.Archive;
using Scarlet.Structures;
using Scarlet.Structures.Archive;

namespace Witch.Commands.Debug;

[Command(typeof(WitchFlags), "mod-text", "", "debug", true)]
public class DebugModTextEARCCommand : EARCCommand {
    public DebugModTextEARCCommand(WitchFlags flags) : base(flags) {
        var testEarc = AssetManager.Instance.Archives[new AssetId("data://c000.ebex", TypeIdRegistry.EARC)];
        var builder = new EbonyArchiveBuilder(testEarc);

        var modId = new AssetId("data://mods.ebex@", TypeIdRegistry.EARC);
        builder.AddOrReplaceFile(new EbonyArchiveBuilder.RebuildRecord(modId, EbonyArchiveFileFlags.AutoLoad | EbonyArchiveFileFlags.Reference, "$archives/mods.earc", "data://mods.ebex@"));
        var erepId = new AssetId("data://c000.erep", TypeIdRegistry.EREP);
        var erepTargetId = new AssetId("data://c000.ebex", TypeIdRegistry.EARC);

        var modMessageId = new AssetId("data://mods/text_us_9c9439df.parambin", TypeIdRegistry.PARAMBIN);
        AssetId.IdTable[modMessageId] = "data://mods/text_us_9c9439df.parambin";
        var originalMessageId = new AssetId("data://param/bin/text_us_9c9439df.parambin", TypeIdRegistry.PARAMBIN);
        var erepBuilder = new EbonyReplaceBuilder(AssetManager.Instance.Replacements[erepTargetId]);
        erepBuilder.Replace(originalMessageId, modMessageId);
        builder.ReplaceFile(erepId, erepBuilder.Build);

        using var output = new FileStream("c000.earc", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        builder.Build(output);

        var modEarc = new EbonyArchiveBuilder();
        modEarc.AddOrReplaceFile(new EbonyArchiveBuilder.RebuildRecord(modMessageId, EbonyArchiveFileFlags.None, "mods/text_us_9c9439df.parambin", "data://mods/text_us_9c9439df.parambin") {
            DataDelegate = (_ => {
                                var msg = AssetManager.Instance.Read(originalMessageId);
                                var index = msg.Span.IndexOf("BENCHMARK"u8);
                                "BEWITCHED"u8.CopyTo(msg.Span[index..]);
                                return msg;
                            }),
        });

        using var modOutput = new FileStream("mods.earc", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        modEarc.Build(modOutput);
    }
}

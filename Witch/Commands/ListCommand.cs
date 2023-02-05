using DragonLib.CommandLine;
using Scarlet;
using Scarlet.Structures.Archive;
using Serilog;

namespace Witch.Commands;

[Command(typeof(WitchFlags), "list", "Lists the files in EARC")]
public class ListCommand : EARCCommand {
    public ListCommand(WitchFlags flags) : base(flags) {
        foreach (var (fileId, resource) in AssetManager.Instance.IdTable) {
            var (archive, file) = resource.Deconstruct();
            Log.Information("{FileId:X16} = {Flags:F} : {Resource}", fileId.Value, file.Flags & (EbonyArchiveFileFlags) 0xFFFFFFFF, resource.DataPath);
        }
    }
}

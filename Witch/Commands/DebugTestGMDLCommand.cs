using DragonLib.CommandLine;
using Scarlet;
using Scarlet.Gfx;
using Scarlet.Structures;

namespace Witch.Commands;

[Command(typeof(WitchFlags), "gmdl", "", "debug", true)]
public class DebugTestGMDLCommand : EARCCommand {
    public DebugTestGMDLCommand(WitchFlags flags) : base(flags) {
        foreach (var (fileId, resource) in AssetManager.Instance.IdTable) {
            if (fileId.Type != TypeIdRegistry.GMDL_GFXBIN) {
                continue;
            }

            if (resource.TryCreate<GraphicsModel>(out var unused)) {
                // stuff.
            }
        }
    }
}

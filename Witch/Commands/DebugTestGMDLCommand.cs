using DragonLib.CommandLine;
using Scarlet;
using Scarlet.Gfx;
using Scarlet.Structures;

namespace Witch.Commands;

[Command(typeof(WitchFlags), "gmdl", "", "debug", true)]
public class DebugTestGMDLCommand : EARCCommand {
    public DebugTestGMDLCommand(WitchFlags flags) : base(flags) {
        var target = flags.Positionals.Skip(1).Select(x => x.ToLowerInvariant()).ToHashSet();

        foreach (var (fileId, resource) in AssetManager.Instance.IdTable) {
            if (fileId.Type != TypeIdRegistry.GMDL_GFXBIN) {
                continue;
            }

            if (target.Count > 0 && !target.Any(x => resource.DataPath.Contains(x, StringComparison.Ordinal))) {
                continue;
            }

            var gmdl = resource.Create<GraphicsModel>();
        }
    }
}

using Scarlet;
using Serilog;

namespace Witch.Commands;

public abstract class EARCCommand {
    protected EARCCommand(WitchFlags flags) {
        AssetManager.DetectGame(flags.InstallDir);

        var earcFiles = Directory.GetFiles(Path.Combine(flags.InstallDir, "datas"), "*.earc", SearchOption.AllDirectories);
        var ememFiles = Directory.GetFiles(Path.Combine(flags.InstallDir, "datas"), "*.emem", SearchOption.AllDirectories);
        var files = new List<string>(earcFiles.Length + ememFiles.Length);
        files.AddRange(earcFiles);
        files.AddRange(ememFiles);

        Log.Information("Loading {Count} archives", files.Count);
        Parallel.ForEach(files, (earcFile) => {
                                    Log.Information("Loading {File}", earcFile);
                                    AssetManager.Instance.LoadArchive(earcFile);
                                });

        Log.Information("Building file table");
        AssetManager.Instance.Build();

        Log.Information("Running command...");
    }
}

using DragonLib.CommandLine;

namespace Witch;

public record WitchFlags : CommandLineFlags {
    [Flag("install-dir", Positional = 0, IsRequired = true)]
    public string InstallDir { get; set; } = null!;
}

public record ExtractFlags : WitchFlags {
    [Flag("output-dir", Positional = 1, IsRequired = true)]
    public string OutputDir { get; set; } = null!;
}

namespace Scarlet.Structures.Gfx;

public record struct GraphicsBinaryHeader {
    public const uint WitchVersion = 20220707;
    public const uint BlackVersion = 20160705;
    public const uint BlackDemoVersion = 20150713;

    public uint Version { get; init; }
    public Dictionary<string, string> Dependencies { get; init; }
    public AssetId[] Ids { get; init; }
}

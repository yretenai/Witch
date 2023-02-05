namespace Scarlet.Structures.Gfx.Model;

public record struct GraphicsBinaryHeader {
    public const uint WitchVersion = 20220707;
    public const uint BlackVersion = 20160705;
    public const uint BlackDemoVersion = 20150713;

    public uint Version { get; set; }
    public Dictionary<string, string> Dependencies { get; set; }
    public AssetId[] Ids { get; set; }
}

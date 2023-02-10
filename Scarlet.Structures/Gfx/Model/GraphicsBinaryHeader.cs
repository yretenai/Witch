namespace Scarlet.Structures.Gfx.Model;

public record struct GraphicsBinaryHeader {
    public const uint SCARLET_VERSION = 20220707;
    public const uint BLACK_VERSION = 20160705;
    public const uint BLACK_DEMO_VERSION = 20150713;

    public uint Version { get; set; }
    public Dictionary<string, string> Dependencies { get; set; }
    public AssetId[] Ids { get; set; }
}

using Scarlet.Structures.Archive;

namespace Scarlet.Archive;

// EREP stands for "Replace"
// The entire file is a single array of 2 Ids, the first being the destination, the second being the source.
public readonly record struct EbonyReplace : IAsset {
    public EbonyReplace() => Replacements = new Dictionary<AssetId, AssetId>();

    public EbonyReplace(AssetId assetId, MemoryOwner<byte> data) {
        if (assetId.Type.Value is not TypeIdRegistry.EREP) {
            throw new TypeIdMismatchException(assetId, TypeIdRegistry.EREP);
        }

        AssetId = assetId;
        var blitReplacements = new BlitStruct<EbonyReplacement>(data, 0, (uint) data.Length >> 4);
        Replacements = new Dictionary<AssetId, AssetId>(blitReplacements.Length);

        foreach (var replacement in blitReplacements.Span) {
            Replacements[replacement.AssetId] = replacement.ReplacementAssetId;
        }
    }

    public AssetId AssetId { get; }
    public Dictionary<AssetId, AssetId> Replacements { get; }
}

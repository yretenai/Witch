using CommunityToolkit.HighPerformance.Buffers;
using Scarlet.Structures;
using Scarlet.Structures.Archive;

namespace Scarlet.Archive;

// EREP stands for "Replace"
// The entire file is a single array of 2 Ids, the first being the destination, the second being the source.
public readonly record struct EbonyReplace : IDisposable {
    public EbonyReplace() {
        Buffer = MemoryOwner<byte>.Empty;
        BlitReplacements = BlitStruct<EbonyReplacement>.Empty;
        Replacements = new Dictionary<AssetId, AssetId>();
    }

    public EbonyReplace(MemoryOwner<byte> data) {
        Buffer = data;
        BlitReplacements = new BlitStruct<EbonyReplacement>(Buffer, 0, (uint) data.Length >> 4);
        Replacements = new Dictionary<AssetId, AssetId>(BlitReplacements.Length);

        foreach (var replacement in BlitReplacements.Span) {
            Replacements[replacement.AssetId] = replacement.ReplacementAssetId;
        }
    }

    public MemoryOwner<byte> Buffer { get; }
    public BlitStruct<EbonyReplacement> BlitReplacements { get; }
    public Dictionary<AssetId, AssetId> Replacements { get; }

    public void Dispose() {
        Buffer.Dispose();
        BlitReplacements.Dispose();
    }
}

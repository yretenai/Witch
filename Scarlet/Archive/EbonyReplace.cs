using CommunityToolkit.HighPerformance.Buffers;
using Scarlet.Structures;
using Scarlet.Structures.Archive;

namespace Scarlet.Archive;

// EREP stands for "Replace"
// The entire file is a single array of 2 FileIds, the first being the destination, the second being the source.
public readonly record struct EbonyReplace : IDisposable {
    public EbonyReplace() {
        Buffer = MemoryOwner<byte>.Empty;
        BlitReplacements = BlitStruct<EbonyReplacement>.Empty;
    }

    public EbonyReplace(MemoryOwner<byte> data) {
        Buffer = data;
        BlitReplacements = new BlitStruct<EbonyReplacement>(Buffer, 0, (uint) data.Length >> 4);
    }

    public MemoryOwner<byte> Buffer { get; }
    public BlitStruct<EbonyReplacement> BlitReplacements { get; }
    public Span<EbonyReplacement> Replacements => BlitReplacements.Span;

    public void Dispose() {
        Buffer.Dispose();
        BlitReplacements.Dispose();
    }
}

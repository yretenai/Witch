using CommunityToolkit.HighPerformance.Buffers;
using Scarlet.Structures;

namespace Scarlet.Archive;

// PACK seems like it is a dependency graph for EARC files, but I don't know what it does for certain.
public sealed class PACK : IDisposable {
    public PACK() {
        Buffer = MemoryOwner<byte>.Empty;
        BlitRows = BlitStruct<byte>.Empty;
    }

    public PACK(MemoryOwner<byte> pack) {
        Buffer = pack;
    }

    public MemoryOwner<byte> Buffer { get; }
    public object Header { get; } = null!; // todo
    public BlitStruct<byte> BlitRows { get; } // todo
    public Span<byte> Rows => BlitRows.Span;

    public void Dispose() {
        Buffer.Dispose();
        BlitRows.Dispose();
    }
}

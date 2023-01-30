using CommunityToolkit.HighPerformance.Buffers;
using Scarlet.Structures;
using Scarlet.Structures.Archive;

namespace Scarlet.Archive;

// EREP stands for either "Repair" or "Reference Protection".
// Every ID in an EREP file is an ID used by a reference file (i.e. a file that simply points to another file).
// In my limited testing modding files that aren't in the EREP file doesn't seem to cause any issues.
// However, I'm not sure if this is the case for all files.
public readonly record struct EREP : IDisposable {
    public EREP() {
        Buffer = MemoryOwner<byte>.Empty;
        BlitRows = BlitStruct<EREPRow>.Empty;
    }

    public EREP(MemoryOwner<byte> erep) {
        Buffer = erep;
        BlitRows = new BlitStruct<EREPRow>(Buffer, 0, (uint) erep.Length >> 4);
    }

    public MemoryOwner<byte> Buffer { get; }
    public BlitStruct<EREPRow> BlitRows { get; }
    public Span<EREPRow> Rows => BlitRows.Span;

    public void Dispose() {
        Buffer.Dispose();
        BlitRows.Dispose();
    }
}

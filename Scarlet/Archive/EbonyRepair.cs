using CommunityToolkit.HighPerformance.Buffers;
using Scarlet.Structures;
using Scarlet.Structures.Archive;

namespace Scarlet.Archive;

// EREP stands for either "Repair" or "Reference Package/Protection".
// Every ID in an EREP file is an ID used by a reference file (i.e. a file that simply points to another file).
// In my limited testing modding files that aren't in the EREP file doesn't seem to cause any issues.
// However, I'm not sure if this is the case for all files.
public readonly record struct EbonyRepair : IDisposable {
    public EbonyRepair() {
        Buffer = MemoryOwner<byte>.Empty;
        BlitRows = BlitStruct<EbonyRepairRow>.Empty;
    }

    public EbonyRepair(MemoryOwner<byte> data) {
        Buffer = data;
        BlitRows = new BlitStruct<EbonyRepairRow>(Buffer, 0, (uint) data.Length >> 4);
    }

    public MemoryOwner<byte> Buffer { get; }
    public BlitStruct<EbonyRepairRow> BlitRows { get; }
    public Span<EbonyRepairRow> Rows => BlitRows.Span;

    public void Dispose() {
        Buffer.Dispose();
        BlitRows.Dispose();
    }
}

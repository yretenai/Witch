using System.Collections;
using CommunityToolkit.HighPerformance.Buffers;
using Scarlet.Structures;
using Scarlet.Structures.Archive;

namespace Scarlet.Archive;

public class EREP : IDisposable {
    public EREP(MemoryOwner<byte> erep) {
        Buffer = erep;
        BlitRows = new BlitStruct<EREPRow>(Buffer, 0, (uint)erep.Length >> 4);
    }

    public MemoryOwner<byte> Buffer { get; }
    public BlitStruct<EREPRow> BlitRows { get; }
    public Span<EREPRow> Rows => BlitRows.Span;

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~EREP() {
        Dispose(false);
    }
    protected virtual void ReleaseUnmanagedResources() { }

    protected virtual void Dispose(bool disposing) {
        ReleaseUnmanagedResources();
        if (disposing) {
            Buffer.Dispose();
            BlitRows.Dispose();
        }
    }
}

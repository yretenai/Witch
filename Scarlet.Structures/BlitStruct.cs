using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;

namespace Scarlet.Structures;

// from LibHac
public readonly struct BlitStruct<T> : IDisposable where T : unmanaged {
    public static BlitStruct<T> Empty => default;

    private readonly MemoryOwner<byte> Buffer;
    private readonly int Offset;
    private readonly int Count;

    public int Length => Count / Unsafe.SizeOf<T>();

    public ref T Value {
        get {
            Debug.Assert(Count >= Unsafe.SizeOf<T>());
            return ref Unsafe.As<byte, T>(ref Buffer.Span[Offset]);
        }
    }

    public BlitStruct(int elementCount) {
        if (elementCount <= 0) {
            throw new ArgumentOutOfRangeException(nameof(elementCount));
        }

        Offset = 0;
        Count = ScarletHelpers.QueryByteLength<T>(elementCount);
        Buffer = MemoryOwner<byte>.Allocate(Count);
    }

    public BlitStruct(MemoryOwner<byte> buffer, int byteSkip, uint elementCount) {
        if (buffer.Length - byteSkip < Unsafe.SizeOf<T>() && elementCount > 0) {
            throw new ArgumentOutOfRangeException(nameof(buffer));
        }

        if (byteSkip > buffer.Length) {
            throw new ArgumentOutOfRangeException(nameof(byteSkip));
        }
        
        Count = ScarletHelpers.QueryByteLength<T>((int) elementCount);

        if (Count > buffer.Length - byteSkip) {
            throw new ArgumentOutOfRangeException(nameof(elementCount));
        }

        Offset = byteSkip;
        Buffer = buffer;
    }

    public Span<T> Span => MemoryMarshal.Cast<byte, T>(ByteSpan);
    public Span<byte> ByteSpan => Buffer.Span.Slice(Offset, Count);
    public BlitSpan<T> BlitSpan => new(ByteSpan);

    public void Dispose() {
        Buffer.Dispose();
    }
}

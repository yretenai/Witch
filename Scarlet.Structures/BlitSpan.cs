using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Scarlet.Structures;

// from LibHac
public readonly ref struct BlitSpan<T> where T : unmanaged {
    public static BlitSpan<T> Empty => default;
    private readonly Span<T> Buffer;

    public int Length => Buffer.Length;

    public ref T Value {
        get {
            Debug.Assert(Buffer.Length > 0);
            return ref MemoryMarshal.GetReference(Buffer);
        }
    }

    public ref T this[int index] => ref Buffer[index];

    public BlitSpan(Span<T> data) {
        if (data.Length == 0) {
            throw new ArgumentOutOfRangeException(nameof(data));
        }

        Buffer = data;
    }

    public BlitSpan(Span<byte> data) {
        if (data.Length < Unsafe.SizeOf<T>()) {
            throw new ArgumentOutOfRangeException(nameof(data));
        }

        Buffer = MemoryMarshal.Cast<byte, T>(data);
    }

    public BlitSpan(Memory<T> data) {
        if (data.Length == 0) {
            throw new ArgumentOutOfRangeException(nameof(data));
        }

        Buffer = data.Span;
    }

    public BlitSpan(Memory<byte> data) {
        if (data.Length < Unsafe.SizeOf<T>()) {
            throw new ArgumentOutOfRangeException(nameof(data));
        }

        Buffer = MemoryMarshal.Cast<byte, T>(data.Span);
    }

    public BlitSpan(ref T data) => Buffer = new Span<T>(ref data);
    public Span<T> Span => Buffer;
    public Span<byte> ByteSpan => MemoryMarshal.Cast<T, byte>(Buffer);
    public Span<byte> GetByteSpan(int elementIndex) => MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref Buffer[elementIndex]), Unsafe.SizeOf<T>());
}

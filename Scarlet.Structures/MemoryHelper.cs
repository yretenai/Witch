using System.Runtime.CompilerServices;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;

namespace Scarlet.Structures;

public static class MemoryHelper {
    public static int QueryByteLength<T>(int elementCount) => Unsafe.SizeOf<T>() * elementCount;

    public static string GetString(MemoryOwner<byte> buffer, int offset) {
        var span = buffer.Span[offset..];
        var length = span.IndexOf((byte) 0);
        if (length < 0) {
            length = span.Length;
        }

        return Encoding.UTF8.GetString(span[..length]);
    }
}

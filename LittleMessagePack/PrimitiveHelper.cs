using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace LittleMessagePack;

internal static class PrimitiveHelper {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ushort ReadUInt16(bool useBigEndian, Span<byte> data, ref int offset) {
        var value = useBigEndian ? BinaryPrimitives.ReadUInt16BigEndian(data[offset..]) : BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
        offset += sizeof(ushort);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static short ReadInt16(bool useBigEndian, Span<byte> data, ref int offset) {
        var value = useBigEndian ? BinaryPrimitives.ReadInt16BigEndian(data[offset..]) : BinaryPrimitives.ReadInt16LittleEndian(data[offset..]);
        offset += sizeof(short);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint ReadUInt32(bool useBigEndian, Span<byte> data, ref int offset) {
        var value = useBigEndian ? BinaryPrimitives.ReadUInt32BigEndian(data[offset..]) : BinaryPrimitives.ReadUInt32LittleEndian(data[offset..]);
        offset += sizeof(uint);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int ReadInt32(bool useBigEndian, Span<byte> data, ref int offset) {
        var value = useBigEndian ? BinaryPrimitives.ReadInt32BigEndian(data[offset..]) : BinaryPrimitives.ReadInt32LittleEndian(data[offset..]);
        offset += sizeof(int);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong ReadUInt64(bool useBigEndian, Span<byte> data, ref int offset) {
        var value = useBigEndian ? BinaryPrimitives.ReadUInt64BigEndian(data[offset..]) : BinaryPrimitives.ReadUInt64LittleEndian(data[offset..]);
        offset += sizeof(ulong);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static long ReadInt64(bool useBigEndian, Span<byte> data, ref int offset) {
        var value = useBigEndian ? BinaryPrimitives.ReadInt64BigEndian(data[offset..]) : BinaryPrimitives.ReadInt64LittleEndian(data[offset..]);
        offset += sizeof(long);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float ReadFloat32(bool useBigEndian, Span<byte> data, ref int offset) {
        var value = useBigEndian ? BinaryPrimitives.ReadSingleBigEndian(data[offset..]) : BinaryPrimitives.ReadSingleLittleEndian(data[offset..]);
        offset += sizeof(float);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double ReadFloat64(bool useBigEndian, Span<byte> data, ref int offset) {
        var value = useBigEndian ? BinaryPrimitives.ReadDoubleBigEndian(data[offset..]) : BinaryPrimitives.ReadDoubleLittleEndian(data[offset..]);
        offset += sizeof(double);
        return value;
    }
}

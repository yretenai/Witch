using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;

namespace Scarlet.Structures.Archive;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x28)]
public record struct EbonyArchiveFile {
    public static int StructSize { get; } = Unsafe.SizeOf<EbonyArchiveFile>();

    public AssetId Id;
    public int Size;
    public int CompressedSize;
    public EbonyArchiveFileFlags Flags;
    public int DataPathOffset;
    public long DataOffset;
    public int PathOffset;
    public byte Type;
    public byte Locale;
    public ushort Seed;

    public readonly string GetDataPath(MemoryOwner<byte> buffer) => ScarletHelpers.GetString(buffer, DataPathOffset);
    public readonly string GetPath(MemoryOwner<byte> buffer) => ScarletHelpers.GetString(buffer, PathOffset);
}

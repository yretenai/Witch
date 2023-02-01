using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Scarlet.Structures.Archive;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x40)]
public record struct EbonyArchiveHeader {
    public static int Size { get; } = Unsafe.SizeOf<EbonyArchiveHeader>();

    public uint Magic;
    public short VersionMajor;
    public short VersionMinor;
    public uint FileCount;
    public uint BlockSize;
    public uint FATOffset;
    public uint DNTOffset;
    public uint FNTOffset;
    public uint DataOffset;
    public EbonyArchiveFlags Flags;
    public uint ChunkSize;
    public ulong Checksum;
}

using System.Runtime.InteropServices;

namespace Scarlet.Structures.Archive;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x40)]
public record struct EARCHeader {
    public const uint MagicValue = 0x46415243; // FARC - File Archive
    public const ulong ChecksumXOR1 = 0xCBF29CE484222325;
    public const ulong ChecksumXOR2 = 0x8B265046EDA33E8A;
    public uint ANTOffset;
    public uint BlockSize;
    public ulong Checksum;
    public uint ChunkSize;
    public uint DataOffset;
    public uint FATOffset;
    public uint FileCount;
    public EARCFlags Flags;
    public uint FNTOffset;

    public uint Magic;
    public int Version;
}

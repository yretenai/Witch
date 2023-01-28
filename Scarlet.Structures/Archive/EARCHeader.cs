using System.Runtime.InteropServices;

namespace Scarlet.Structures.Archive;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x40)]
public record struct EARCHeader {
    public const uint MagicValue = 0x46415243; // FARC - File Archive
    public const ulong ChecksumXOR1 = 0xCBF29CE484222325;
    public const ulong ChecksumXOR2 = 0x8B265046EDA33E8A;

    public uint Magic;
    public int Version;
    public uint FileCount;
    public uint BlockSize;
    public uint FATOffset;
    public uint DNTOffset;
    public uint FNTOffset;
    public uint DataOffset;
    public EARCFlags Flags;
    public uint ChunkSize;
    public ulong Checksum;
}

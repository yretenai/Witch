using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;

namespace Scarlet.Structures.Archive;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x28)]
public record struct EARCFile {
    public ulong Checksum;
    public int Size;
    public int CompressedSize;
    public EARCFileFlags Flags;
    public int PathOffset;
    public long DataOffset;
    public int ArchivePathOffset;
    public byte Type;
    public byte Locale;
    public ushort Key;

    public string GetPath(MemoryOwner<byte> buffer) => MemoryHelper.GetString(buffer, PathOffset);
    public string GetArchivePath(MemoryOwner<byte> buffer) => MemoryHelper.GetString(buffer, ArchivePathOffset);
}

using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;

namespace Scarlet.Structures.Archive;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x28)]
public record struct EARCFile {
    public int ArchivePathOffset;
    public ulong Checksum;
    public int CompressedSize;
    public long DataOffset;
    public EARCFileFlags Flags;
    public ushort Key;
    public byte Locale;
    public int PathOffset;
    public int Size;
    public byte Type;

    public string GetPath(MemoryOwner<byte> buffer) => MemoryHelper.GetString(buffer, PathOffset);
    public string GetArchivePath(MemoryOwner<byte> buffer) => MemoryHelper.GetString(buffer, ArchivePathOffset);
}

using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;

namespace Scarlet.Structures.Archive;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x28)]
public record struct EARCFile {
    public FileId Id;
    public int Size;
    public int CompressedSize;
    public EARCFileFlags Flags;
    public int DataPathOffset;
    public long DataOffset;
    public int PathOffset;
    public byte Type;
    public byte Locale;
    public ushort Key;

    public string GetDataPath(MemoryOwner<byte> buffer) => MemoryHelper.GetString(buffer, DataPathOffset);
    public string GetPath(MemoryOwner<byte> buffer) => MemoryHelper.GetString(buffer, PathOffset);
}

using System.Runtime.InteropServices;

namespace Scarlet.Structures.Gfx;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct SectionDataBinary {
    public uint Magic;
    public uint Type;
    public int Count;
    public ushort Reserved;
    public ushort DataOffset;
    public uint FileSize;
}

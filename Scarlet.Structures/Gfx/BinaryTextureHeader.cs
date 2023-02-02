using System.Runtime.InteropServices;

namespace Scarlet.Structures.Gfx;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x20)]
public record struct BinaryTextureHeader {
    public uint Magic;
    public uint SectionSize;
    public uint SurfaceSize;
    public ushort Version;
    public byte Platform;
    public byte Flags;
    public ushort ImageCount;
    public ushort ImageStride;
    public int ImageOffset;
}

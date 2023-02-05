using System.Runtime.InteropServices;

namespace Scarlet.Structures.Gfx.Texture;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x38)]
public record struct BinaryTextureImageHeader {
    public ushort Width;
    public ushort Height;
    public ushort Pitch;
    public BinaryTextureFormat Format;
    public byte MipCount;
    public byte Depth;
    public byte Dimension;
    public byte Flags;
    public int SurfaceSize;
    public uint SurfaceStride;
    public int DataOffset;
    public int SurfaceOffset;
    public int NameOffset;
    public int DataSize;
    public byte FullMipCount;
    public int FullDataSize;
    public int Reserved1;
    public int Reserved2;
    public int TileMode;
    public ushort ArraySize;
}

using System.Runtime.InteropServices;

namespace Scarlet.Structures;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 3)]
public record struct Version24 {
    public byte Major;
    public byte Minor;
    public byte Patch;
}

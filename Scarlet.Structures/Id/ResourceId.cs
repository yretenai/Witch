using System.Runtime.InteropServices;

namespace Scarlet.Structures.Id;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
public readonly record struct ResourceId {
    public readonly FixId Type;
    public readonly FixId Subtype;
    public readonly FixId Version;
    private readonly uint Flag;

    public override string ToString() => $"{Type}:{Subtype}:{Version}";
}

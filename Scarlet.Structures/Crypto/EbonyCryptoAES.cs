using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DragonLib;

namespace Scarlet.Structures.Crypto;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct EbonyCryptoAES {
    public ulong IV1;
    public ulong IV2;
    public ulong Zero;
    public ulong Zero2;

    public Span<byte> Key => new Span<EbonyCryptoAES>(ref Unsafe.AsRef(this)).AsBytes()[..16];
}

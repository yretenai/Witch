using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Scarlet.Structures.Crypto;

namespace Scarlet.Crypto;

public static class EbonyCrypto {
    public static Stream Decrypt(Stream data, byte[] key) {
        var encryption = (EbonyCryptoType) data.ReadByte();
        Debug.Assert(encryption <= EbonyCryptoType.Shuffle, "encryption == CryptoType.Shuffle");

        try {
            return encryption switch {
                       EbonyCryptoType.None    => data,
                       EbonyCryptoType.AES     => DecryptAES(data, key),
                       EbonyCryptoType.Shuffle => DecryptShuffle(data, key),
                       _                       => throw new NotSupportedException(),
                   };
        } finally {
            if (encryption != EbonyCryptoType.None) {
                data.Dispose();
            }
        }
    }

    public static Stream DecryptAES(Stream data, byte[] key) {
        using var aes = Aes.Create();
        aes.Key = key;
        var iv = new Span<EbonyCryptoAES>(ref Unsafe.AsRef(new EbonyCryptoAES()));
        var ivb = iv.AsBytes();
        data.Seek(-(ivb.Length + 1), SeekOrigin.End);
        data.ReadExactly(ivb);
        aes.IV = iv[0].Key.ToArray();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.None;

        data.Position = 0;
        using var decryptor = aes.CreateDecryptor();
        var block = new byte[data.Length - (ivb.Length + 1)];
        data.ReadExactly(block);
        return new MemoryStream(decryptor.TransformFinalBlock(block, 0, block.Length));
    }

    public static Stream DecryptShuffle(Stream data, byte[] key) => throw new NotSupportedException("Shuffle encryption is not supported");

    public static MemoryOwner<byte> Decrypt(MemoryOwner<byte> data, byte[] key) {
        var encryption = (EbonyCryptoType) data.Span[^1];
        Debug.Assert(encryption <= EbonyCryptoType.Shuffle, "encryption == CryptoType.Shuffle");

        try {
            return encryption switch {
                       EbonyCryptoType.None    => data,
                       EbonyCryptoType.AES     => DecryptAES(data, key),
                       EbonyCryptoType.Shuffle => DecryptShuffle(data, key),
                       _                       => throw new NotSupportedException(),
                   };
        } finally {
            if (encryption != EbonyCryptoType.None) {
                data.Dispose();
            }
        }
    }

    public static MemoryOwner<byte> DecryptAES(MemoryOwner<byte> data, byte[] key) {
        using var aes = Aes.Create();
        aes.Key = key;
        var iv = MemoryMarshal.Read<EbonyCryptoAES>(data.Span[^(Unsafe.SizeOf<EbonyCryptoAES>() + 1)..]);
        aes.IV = iv.Key.ToArray();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.None;

        var block = MemoryOwner<byte>.Allocate(data.Length - (Unsafe.SizeOf<EbonyCryptoAES>() + 1));
        try {
            aes.DecryptCbc(data.Span[..block.Length], iv.Key, PaddingMode.None).CopyTo(block.Span);
            return block;
        } catch {
            block.Dispose();
            throw;
        }
    }

    public static MemoryOwner<byte> DecryptShuffle(MemoryOwner<byte> data, byte[] key) => throw new NotSupportedException("Shuffle encryption is not supported");
}

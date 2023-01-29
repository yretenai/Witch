using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using CommunityToolkit.HighPerformance.Buffers;
using DragonLib.Hash;
using DragonLib.Hash.Basis;
using K4os.Compression.LZ4.Streams;
using Scarlet.Structures;
using Scarlet.Structures.Archive;

namespace Scarlet.Archive;

public class EARC : IDisposable {
    private const uint MagicValue = 0x46415243; // FARC - File Archive
    private const ulong ChecksumXOR1 = 0xCBF29CE484222325;
    private const ulong ChecksumXOR2 = 0x8B265046EDA33E8A;
    private const uint SeedExpansion = 0x41C64E6D;
    private const uint SeedOffset = 0x3039;
    private static readonly byte[] EMEMKey = { 0x50, 0x16, 0xec, 0xa2, 0x58, 0x3d, 0x8e, 0xdd, 0x44, 0xfc, 0x15, 0x78, 0x4c, 0x9e, 0x2c, 0xcb };
    private static readonly byte[] EARCKey = { 0x9C, 0x6C, 0x5D, 0x41, 0x15, 0x52, 0x3F, 0x17, 0x5A, 0xD3, 0xF8, 0xB7, 0x75, 0x58, 0x1E, 0xCF };

    public unsafe EARC(string path, bool isEMEM = false) {
        using var _perf = new PerformanceCounter<PerformanceHost.EARC>();
        Stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        if (isEMEM) {
            Stream.Seek(-1, SeekOrigin.End);
            var encryption = (EARCEncryption) Stream.ReadByte();
            Debug.Assert(encryption == EARCEncryption.AES, "encryption == EARCEncryption.AES");

            using var aes = Aes.Create();
            aes.Key = EMEMKey;
            var iv = new byte[16];
            Stream.Seek(-33, SeekOrigin.End);
            Stream.ReadExactly(iv);
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;

            Stream.Position = 0;
            using var decryptor = aes.CreateDecryptor();
            var block = new byte[Stream.Length - 33];
            Stream.ReadExactly(block);
            Stream.Dispose();
            Stream = new MemoryStream(decryptor.TransformFinalBlock(block, 0, block.Length));
        }

        if (Stream.Length < Unsafe.SizeOf<EARCHeader>()) {
            Stream.Close();
            Stream.Dispose();
            throw new InvalidDataException("File is too small to be an EARC archive.");
        }

        var header = stackalloc EARCHeader[1];
        var blitHeader = new BlitSpan<EARCHeader>(ref header[0]);
        Stream.ReadExactly(blitHeader.GetByteSpan(0));

        if (header->Magic != MagicValue) {
            Stream.Close();
            Stream.Dispose();
            throw new InvalidDataException("File is not an EARC archive.");
        }

        Stream.Position = 0;
        Buffer = MemoryOwner<byte>.Allocate((int) header->DataOffset);
        Stream.ReadExactly(Buffer.Span);
        BlitFileEntries = new BlitStruct<EARCFile>(Buffer, (int) header->FATOffset, header->FileCount);

        if (header->Version < 0) {
            using var _perfDeobfuscate = new PerformanceCounter<PerformanceHost.EARC.Deobfuscate>();
            header->Version &= 0x7FFFFFFF;
            var key = header->Checksum ^ ChecksumXOR1;
            if ((header->Flags & EARCFlags.AdvanceChecksum) != 0) {
                key ^= ChecksumXOR2;
            }

            using var fnv = FowlerNollVo.Create((FNV64Basis) key);
            var blitFileEntries = BlitFileEntries.BlitSpan;
            for (var i = 0; i < blitFileEntries.Length; i++) {
                ref var file = ref blitFileEntries[i];
                var bytes = MemoryMarshal.Cast<byte, ulong>(blitFileEntries.GetByteSpan(i));
                if ((file.Flags & EARCFileFlags.SkipObfuscation) == 0) {
                    (file.Size, file.CompressedSize) = (file.CompressedSize, file.Size);
                    bytes[1] ^= fnv.HashNext(file.Id);
                    bytes[3] ^= fnv.HashNext(~file.Id);
                    (file.Size, file.CompressedSize) = (file.CompressedSize, file.Size);
                }
            }
        }

        Header = header[0];
    }

    public Stream Stream { get; }
    public MemoryOwner<byte> Buffer { get; }
    public EARCHeader Header { get; }
    private BlitStruct<EARCFile> BlitFileEntries { get; }
    public Span<EARCFile> FileEntries => BlitFileEntries.Span;

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~EARC() {
        Dispose(false);
    }

    protected virtual void ReleaseUnmanagedResources() { }

    protected virtual void Dispose(bool disposing) {
        ReleaseUnmanagedResources();
        if (disposing) {
            Stream.Close();
            Stream.Dispose();
            Buffer.Dispose();
            BlitFileEntries.Dispose();
        }
    }

    public unsafe MemoryOwner<byte> ReadFile(in EARCFile file) {
        using var _perf = new PerformanceCounter<PerformanceHost.EARC.Read>();
        Stream.Position = file.DataOffset;

        var expandedKey = file.Seed == 0 ? 0ul : ((ulong) file.Seed * SeedExpansion + SeedOffset) * SeedExpansion + SeedOffset;
        Debug.Assert(expandedKey == 0, "expandedKey == 0"); // note: validate if xoring with expandedKey is the same.

        MemoryOwner<byte> buffer;

        if ((file.Flags & EARCFileFlags.Encrypted) != 0) {
            Stream.Position = file.DataOffset + file.CompressedSize - 1;
            var encryption = (EARCEncryption) Stream.ReadByte();
            Debug.Assert(encryption == EARCEncryption.AES, "encryption == EARCEncryption.AES");

            Stream.Position = file.DataOffset + file.CompressedSize - 33;

            using var aes = Aes.Create();
            aes.Key = EARCKey;

            var iv = new byte[16];
            var iv64 = MemoryMarshal.Cast<byte, ulong>(iv);
            Stream.ReadExactly(iv);
            iv64[0] ^= expandedKey;
            aes.IV = iv;
            aes.Padding = PaddingMode.None;
            Stream.Position = file.DataOffset;

            using var decryptor = aes.CreateDecryptor();

            buffer = MemoryOwner<byte>.Allocate(file.CompressedSize - 33);

            try {
                Stream.ReadExactly(buffer.Span);
                var array = buffer.DangerousGetArray();
                var decrypted = decryptor.TransformFinalBlock(array.Array!, array.Offset, array.Count).AsSpan();
                decrypted.CopyTo(buffer.Span);
            } catch {
                Buffer.Dispose();
                throw;
            }
        } else {
            buffer = MemoryOwner<byte>.Allocate(file.CompressedSize);

            try {
                Stream.ReadExactly(buffer.Span);
            } catch {
                Buffer.Dispose();
                throw;
            }
        }

        if (file.Seed != 0 && (file.Flags & EARCFileFlags.Encrypted) != 0) {
            var u64 = MemoryMarshal.Cast<byte, ulong>(buffer.Span);
            u64[0] ^= expandedKey;
        }

        _perf.Stop();

        if ((file.Flags & EARCFileFlags.Compressed) == 0) {
            return buffer;
        }

        try {
            using var _perfDecrypt = new PerformanceCounter<PerformanceHost.EARC.Decompress>();
            var flags = file.Flags;
            if ((flags & EARCFileFlags.HasCompressType) == 0) {
                flags &= EARCFileFlags.HasCompressType;
                flags &= (EARCFileFlags) ((uint) EARCCompressionType.Zlib << 29);
            } else {
                Debug.Assert((EARCCompressionType) ((uint) flags >> 7) != EARCCompressionType.Zlib, "Zlib compression is an assumption.");
            }

            var decompressed = MemoryOwner<byte>.Allocate(file.Size);
            using var inputPin = buffer.Memory.Pin();
            using var input = new UnmanagedMemoryStream((byte*) inputPin.Pointer, buffer.Length);

            switch ((EARCCompressionType) ((uint) flags >> 29)) {
                case EARCCompressionType.Zlib: {
                    input.Position = 2; // skip zlib header

                    try {
                        using var zlib = new DeflateStream(input, CompressionMode.Decompress);
                        zlib.ReadExactly(decompressed.Span);
                    } catch {
                        decompressed.Dispose();
                        throw;
                    }

                    break;
                }
                case EARCCompressionType.LZ4Stream: {
                    try {
                        using var lz = LZ4Stream.Decode(input);
                        lz.ReadExactly(decompressed.Span);
                    } catch {
                        decompressed.Dispose();
                        throw;
                    }

                    break;
                }
                default: {
                    decompressed.Dispose();
                    throw new NotSupportedException("Unknown compression type.");
                }
            }

            return decompressed;
        } finally {
            buffer.Dispose();
        }
    }
}

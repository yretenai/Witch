using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using CommunityToolkit.HighPerformance.Buffers;
using DragonLib;
using DragonLib.Hash;
using DragonLib.Hash.Basis;
using K4os.Compression.LZ4.Streams;
using Scarlet.Structures;
using Scarlet.Structures.Archive;
using Serilog;

namespace Scarlet.Archive;

public readonly record struct EbonyArchive : IDisposable {
    private const uint MagicValue = 0x46415243; // FARC - File Archive
    private const ulong ChecksumXOR1 = 0xCBF29CE484222325;
    private const ulong ChecksumXOR2 = 0x8B265046EDA33E8A;
    private const uint SeedExpansion = 0x41C64E6D;
    private const uint SeedOffset = 0x3039;
    private static readonly byte[] MemoryKey = { 0x50, 0x16, 0xec, 0xa2, 0x58, 0x3d, 0x8e, 0xdd, 0x44, 0xfc, 0x15, 0x78, 0x4c, 0x9e, 0x2c, 0xcb };
    private static readonly byte[] ArchiveKey = { 0x9C, 0x6C, 0x5D, 0x41, 0x15, 0x52, 0x3F, 0x17, 0x5A, 0xD3, 0xF8, 0xB7, 0x75, 0x58, 0x1E, 0xCF };

    // EARC stands for "Archive", it's used to store files in a compressed format to reduce the size of the game and disk load times.
    // EMEM stands for "Memory", it's used to store virtual PACK files to help with the loading of the game (i assume.)
    public unsafe EbonyArchive(string path, bool isMem = false) {
        using var _perf = new PerformanceCounter<PerformanceHost.EbonyArchive>();
        Stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        if (isMem) { // zero idea why this is encrypted, but it is.
            Stream.Seek(-1, SeekOrigin.End);
            var encryption = (EbonyArchiveEncryption) Stream.ReadByte();
            Debug.Assert(encryption == EbonyArchiveEncryption.AES, "encryption == EbonyArchiveEncryption.AES");

            using var aes = Aes.Create();
            aes.Key = MemoryKey;
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

        if (Stream.Length < EbonyArchiveHeader.Size) {
            Stream.Close();
            Stream.Dispose();
            throw new InvalidDataException("File is too small to be an EARC archive.");
        }

        Span<EbonyArchiveHeader> header = stackalloc EbonyArchiveHeader[1];
        Stream.ReadExactly(header.AsBytes());

        if (header[0].Magic != MagicValue) {
            Stream.Close();
            Stream.Dispose();
            throw new InvalidDataException("File is not an EARC archive.");
        }

        Stream.Position = 0;
        Buffer = MemoryOwner<byte>.Allocate((int) header[0].DataOffset);
        Stream.ReadExactly(Buffer.Span);
        BlitFileEntries = new BlitStruct<EbonyArchiveFile>(Buffer, (int) header[0].FATOffset, header[0].FileCount);

        if (header[0].VersionMinor < 0) {
            using var _perfDeobfuscate = new PerformanceCounter<PerformanceHost.EbonyArchive.Deobfuscate>();
            header[0].VersionMinor &= 0x7FFF;
            var key = header[0].Checksum ^ ChecksumXOR1;
            if ((header[0].Flags & EbonyArchiveFlags.AdvanceChecksum) != 0) {
                key ^= ChecksumXOR2;
            }

            using var fnv = FowlerNollVo.Create((FNV64Basis) key);
            var blitFileEntries = BlitFileEntries.BlitSpan;
            for (var i = 0; i < blitFileEntries.Length; i++) {
                ref var file = ref blitFileEntries[i];
                var bytes = MemoryMarshal.Cast<byte, ulong>(blitFileEntries.GetByteSpan(i));
                if ((file.Flags & EbonyArchiveFileFlags.SkipObfuscation) == 0) {
                    (file.Size, file.CompressedSize) = (file.CompressedSize, file.Size);
                    bytes[1] ^= fnv.HashNext(file.Id);
                    bytes[3] ^= fnv.HashNext(~file.Id);
                    (file.Size, file.CompressedSize) = (file.CompressedSize, file.Size);
                }
            }
        }

        Header = header[0];

        var size = FileEntries[^1].DataOffset + FileEntries[^1].CompressedSize;
        var checksum = CalculateHash(Stream, size);
        if (checksum == 0) {
            Log.Warning("EARC Checksum Validation Skipped! No sane person should hash a large file!");
        } else if (checksum != Header.Checksum) {
            Log.Error("EARC Checksum validation failed! Header = {Checksum:X16} Witch = {OurChecksum:X16}", Header.Checksum, checksum);
        } else {
            Log.Information("EARC Checksum verified! Header = {Checksum:X16} Witch = {OurChecksum:X16}", Header.Checksum, checksum);
        }
    }

    public Stream Stream { get; }
    public MemoryOwner<byte> Buffer { get; }
    public EbonyArchiveHeader Header { get; }
    private BlitStruct<EbonyArchiveFile> BlitFileEntries { get; }
    public Span<EbonyArchiveFile> FileEntries => BlitFileEntries.Span;

    public void Dispose() {
        Stream.Close();
        Stream.Dispose();
        Buffer.Dispose();
        BlitFileEntries.Dispose();
    }

    public static ulong CalculateHashXV(Stream stream, long dataSize) {
        if (dataSize < EbonyArchiveHeader.Size) {
            return 0;
        }

        var position = stream.Position;

        stream.Position = 0;

        try {
            using var buffer = MemoryOwner<byte>.Allocate(0x40);
            stream.ReadExactly(buffer.Span);
            var buffer64 = MemoryMarshal.Cast<byte, ulong>(buffer.Span);
            buffer64[5] = 0;
            var sha = MemoryMarshal.Cast<byte, ulong>(SHA256.HashData(buffer.Span).AsSpan());
            return sha[0] ^ sha[1] ^ sha[3];
        } finally {
            stream.Position = position;
        }
    }

    public static ulong CalculateHash(Stream stream, long dataSize, bool force = false) {
        if (dataSize > int.MaxValue && !force) {
            return 0;
        }

        const uint CHUNK_SIZE = 0x8000000;
        const ulong XOR_CONST = 0x7575757575757575UL;
        const uint STATIC_SEED0 = 0xb16949df;
        const uint STATIC_SEED1 = 0x104098f5;
        const uint STATIC_SEED2 = 0x9eb9b68b;
        const uint STATIC_SEED3 = 0x3120f7cb;

        var position = stream.Position;

        stream.Position = EbonyArchiveHeader.Size;

        try {
            var size = dataSize - EbonyArchiveHeader.Size;

            // var blocks = (buffer.Length >> 4) - 1;

            // hash up to MAX_SIZE (128 MiB) of data in CHUNK_SIZE (8MiB) chunks and hash them into a 128-bit hash array.
            var blocks = (int) (size.Align(CHUNK_SIZE) >> 27); // 0x8000000 -> 0x1

            using var chunk = MemoryOwner<byte>.Allocate((int) CHUNK_SIZE);
            using var hashList = MemoryOwner<byte>.Allocate((blocks + 1) << 4); // 1 -> 16
            for (var i = 0; i < blocks; ++i) {
                var offset = (long) i << 27; // 1 -> 0x8000000
                var length = (int) Math.Min(size - offset, CHUNK_SIZE);
                stream.ReadExactly(chunk.Span[..length]);
                MD5.HashData(chunk.Span[..length]).CopyTo(hashList.Span[(i << 4)..]);
            }

            var hashSeed = MemoryMarshal.Cast<byte, uint>(hashList.Span[^16..]);
            if (blocks > 8) {
                var seed = (ulong) size ^ XOR_CONST;
                // xorshift64.
                for (var i = 0; i < 4; ++i) {
                    seed ^= seed << 13;
                    seed ^= seed >> 7;
                    seed ^= seed << 17;
                    hashSeed[i] = (uint) seed;
                }
            } else {
                // some magic numbers fished up from leviathan's egg pond or someth
                hashSeed[0] = STATIC_SEED0;
                hashSeed[1] = STATIC_SEED1;
                hashSeed[2] = STATIC_SEED2;
                hashSeed[3] = STATIC_SEED3;
            }

            // SHA256 the hash list and overlay the 256-bit hash into 64-bits.
            // one might wonder, why would you not just use Murmur64 or xxHash64 as it's faster
            // since we only care about data integrity, we don't need the security benefits of SHA
            var sha = MemoryMarshal.Cast<byte, ulong>(SHA256.HashData(hashList.Span).AsSpan());
            return sha[0] ^ sha[1] ^ sha[2] ^ sha[3];
        } finally {
            stream.Position = position;
        }
    }

    public unsafe MemoryOwner<byte> Read(in EbonyArchiveFile file) {
        using var _perf = new PerformanceCounter<PerformanceHost.EbonyArchive.Read>();
        Stream.Position = file.DataOffset;

        var expandedKey = file.Seed == 0 ? 0ul : ((ulong) file.Seed * SeedExpansion + SeedOffset) * SeedExpansion + SeedOffset;
        Debug.Assert(expandedKey == 0, "expandedKey == 0"); // note: validate if xoring with expandedKey is the same.

        MemoryOwner<byte> buffer;

        if ((file.Flags & EbonyArchiveFileFlags.Encrypted) != 0) {
            Stream.Position = file.DataOffset + file.CompressedSize - 1;
            var encryption = (EbonyArchiveEncryption) Stream.ReadByte();
            Debug.Assert(encryption == EbonyArchiveEncryption.AES, "encryption == EbonyArchiveEncryption.AES");

            Stream.Position = file.DataOffset + file.CompressedSize - 33;

            using var aes = Aes.Create();
            aes.Key = ArchiveKey;

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
                buffer.Dispose();
                throw;
            }
        } else {
            buffer = MemoryOwner<byte>.Allocate(file.CompressedSize);

            try {
                Stream.ReadExactly(buffer.Span);
            } catch {
                buffer.Dispose();
                throw;
            }
        }

        if (file.Seed != 0 && (file.Flags & EbonyArchiveFileFlags.Encrypted) != 0) {
            var u64 = MemoryMarshal.Cast<byte, ulong>(buffer.Span);
            u64[0] ^= expandedKey;
        }

        _perf.Stop();

        if ((file.Flags & EbonyArchiveFileFlags.Compressed) == 0) {
            return buffer;
        }

        try {
            using var _perfDecrypt = new PerformanceCounter<PerformanceHost.EbonyArchive.Decompress>();
            var flags = file.Flags;
            if ((flags & EbonyArchiveFileFlags.HasCompressType) == 0) {
                flags &= EbonyArchiveFileFlags.HasCompressType;
                flags &= (EbonyArchiveFileFlags) ((uint) EbonyArchiveCompressionType.Zlib << 29);
            } else {
                Debug.Assert((EbonyArchiveCompressionType) ((uint) flags >> 7) != EbonyArchiveCompressionType.Zlib, "Zlib compression is an assumption.");
            }

            var decompressed = MemoryOwner<byte>.Allocate(file.Size);
            using var inputPin = buffer.Memory.Pin();
            using var input = new UnmanagedMemoryStream((byte*) inputPin.Pointer, buffer.Length);

            switch ((EbonyArchiveCompressionType) ((uint) flags >> 29)) {
                case EbonyArchiveCompressionType.Zlib: {
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
                case EbonyArchiveCompressionType.LZ4Stream: {
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

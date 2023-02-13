using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using DragonLib.Hash;
using DragonLib.Hash.Basis;
using K4os.Compression.LZ4.Streams;
using Scarlet.Crypto;
using Scarlet.Structures.Archive;
using Scarlet.Structures.Id;
using Serilog;

namespace Scarlet.Archive;

public readonly record struct EbonyArchive : IAsset, IDisposable {
    internal const uint MAGIC = 0x46415243; // FARC - File Archive
    internal const ulong CSUM_XOR1 = 0xCBF29CE484222325;
    internal const ulong CSUM_XOR2 = 0x8B265046EDA33E8A;
    private const uint SEED_EXPANSION = 0x41C64E6D;
    private const uint SEED_OFFSET = 0x3039;
    private static readonly byte[] EMEM_KEY = { 0x50, 0x16, 0xec, 0xa2, 0x58, 0x3d, 0x8e, 0xdd, 0x44, 0xfc, 0x15, 0x78, 0x4c, 0x9e, 0x2c, 0xcb };
    private static readonly byte[] EARC_KEY = { 0x9C, 0x6C, 0x5D, 0x41, 0x15, 0x52, 0x3F, 0x17, 0x5A, 0xD3, 0xF8, 0xB7, 0x75, 0x58, 0x1E, 0xCF };

    // EARC stands for "Archive", it's used to store files in a compressed format to reduce the size of the game and disk load times.
    // EMEM stands for "Memory", it's used to store virtual PACK files to help with the loading of the game (i assume.)
    public unsafe EbonyArchive(AssetId id, string dataPath, Stream stream) {
        AssetId = id;
        DataPath = dataPath;

        using var _perf = new PerformanceCounter<PerformanceHost.EbonyArchive>();
        Stream = stream;

        if (id.Type.Value is TypeIdRegistry.EMEM) { // zero idea why this is encrypted, but it is.
            Stream = EbonyCrypto.Decrypt(Stream, EMEM_KEY);
        }

        if (Stream.Length < EbonyArchiveHeader.StructSize) {
            Stream.Close();
            Stream.Dispose();
            throw new InvalidDataException("File is too small to be an EARC archive.");
        }

        Span<EbonyArchiveHeader> header = stackalloc EbonyArchiveHeader[1];
        Stream.ReadExactly(header.AsBytes());

        if (header[0].Magic != MAGIC) {
            Stream.Close();
            Stream.Dispose();
            throw new InvalidDataException("File is not an EARC archive.");
        }

        Stream.Position = 0;
        Buffer = MemoryOwner<byte>.Allocate((int) header[0].DataOffset);
        Stream.ReadExactly(Buffer.Span);
        BlitFileEntries = new BlitStruct<EbonyArchiveFile>(Buffer, (int) header[0].FATOffset, header[0].FileCount);

        IdMap = new Dictionary<AssetId, int>();
        var blitFileEntries = BlitFileEntries.BlitSpan;
        for (var i = 0; i < blitFileEntries.Length; i++) {
            IdMap[FileEntries[i].Id] = i;

            if (header[0].VersionMinor < 0) {
                using var _perfDeobfuscate = new PerformanceCounter<PerformanceHost.EbonyArchive.Deobfuscate>();
                var key = header[0].Checksum ^ CSUM_XOR1;
                if ((header[0].Flags & EbonyArchiveFlags.AdvanceChecksum) != 0) {
                    key ^= CSUM_XOR2;
                }

                using var fnv = FowlerNollVo.Create((FNV64Basis) key);
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

    #if DEBUG
        var size = FileEntries[^1].DataOffset + FileEntries[^1].CompressedSize;
        var checksum = CalculateHash(Stream, size);
        if (checksum == 0) {
            return;
        }

        if (checksum != Header.Checksum) {
            Log.Error("EARC Checksum validation failed! Header = {Checksum:X16} Witch = {OurChecksum:X16}", Header.Checksum, checksum);
        } else {
            Log.Information("EARC Checksum verified! Header = {Checksum:X16} Witch = {OurChecksum:X16}", Header.Checksum, checksum);
        }
    #endif
    }

    public Stream Stream { get; }
    public MemoryOwner<byte> Buffer { get; }
    public EbonyArchiveHeader Header { get; }
    private BlitStruct<EbonyArchiveFile> BlitFileEntries { get; }
    public Span<EbonyArchiveFile> FileEntries => BlitFileEntries.Span;
    public Dictionary<AssetId, int> IdMap { get; } = new();
    public string DataPath { get; init; }

    public AssetId AssetId { get; }

    public void Dispose() {
        Stream.Close();
        Stream.Dispose();
        Buffer.Dispose();
        BlitFileEntries.Dispose();
    }

    public static ulong CalculateHash(Stream stream, long dataSize, bool force = false) =>
        AssetManager.Game switch {
            EbonyGame.Black   => CalculateHashBlack(stream, dataSize),
            EbonyGame.Scarlet => CalculateHashScarlet(stream, dataSize, force),
            _                 => 0,
        };

    public static ulong CalculateHashBlack(Stream stream, long dataSize) {
        if (dataSize < EbonyArchiveHeader.StructSize) {
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

    public static ulong CalculateHashScarlet(Stream stream, long dataSize, bool force = false) {
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

        stream.Position = EbonyArchiveHeader.StructSize;

        try {
            var size = dataSize - EbonyArchiveHeader.StructSize;

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
        if (file.Size == 0) {
            return MemoryOwner<byte>.Empty;
        }

        using var _perf = new PerformanceCounter<PerformanceHost.EbonyArchive.Read>();
        Stream.Position = file.DataOffset;

        var expandedKey = file.Seed == 0 ? 0ul : ((ulong) file.Seed * SEED_EXPANSION + SEED_OFFSET) * SEED_EXPANSION + SEED_OFFSET;
        Debug.Assert(expandedKey == 0, "expandedKey == 0"); // note: validate if xoring with expandedKey is the same.

        MemoryOwner<byte> buffer;

        if ((file.Flags & EbonyArchiveFileFlags.Encrypted) != 0) {
            using var tmp = MemoryOwner<byte>.Allocate(file.CompressedSize);
            buffer = EbonyCrypto.Decrypt(tmp, EARC_KEY);
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

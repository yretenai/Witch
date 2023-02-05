using System.Diagnostics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using Scarlet.Structures;
using Scarlet.Structures.Archive;

namespace Scarlet.Archive;

// PACK files are basically just an empty EbonyArchive with only IDs.
public readonly record struct EbonyPatch : IDisposable {
    private const uint MagicValue = 0x5041434B; // PACK

    public EbonyPatch() {
        Buffer = MemoryOwner<byte>.Empty;
        BlitIDs = BlitStruct<AssetId>.Empty;
    }

    public EbonyPatch(MemoryOwner<byte> pack) {
        if (pack.Length < EbonyArchiveHeader.Size) {
            throw new InvalidDataException("File is too small to be a PACK archive.");
        }

        Header = MemoryMarshal.Read<EbonyArchiveHeader>(pack.Span);
        if (Header.Magic != MagicValue) {
            throw new InvalidDataException("File is not a PACK archive.");
        }

        Debug.Assert(Header.VersionMajor == 0, "Header.VersionMajor == 0");
        Debug.Assert(Header.VersionMinor == 0, "Header.VersionMinor == 0");
        Debug.Assert(Header.BlockSize == 0, "Header.BlockSize == 0");
        Debug.Assert(Header.FATOffset == 0, "Header.FATOffset == 0");
        Debug.Assert(Header.DNTOffset == 0, "Header.DNTOffset == 0");
        Debug.Assert(Header.FNTOffset == 0, "Header.FNTOffset == 0");
        Debug.Assert(Header.Flags == 0, "Header.Flags == 0");
        Debug.Assert(Header.ChunkSize == 0, "Header.ChunkSize == 0");
        Debug.Assert(Header.Checksum == 0, "Header.Checksum == 0");

        Buffer = pack.Slice((int) Header.DataOffset, (int) (Header.FileCount * 8));
        BlitIDs = new BlitStruct<AssetId>(Buffer, 0, Header.FileCount);
    }

    public MemoryOwner<byte> Buffer { get; }
    public EbonyArchiveHeader Header { get; }
    public BlitStruct<AssetId> BlitIDs { get; }
    public Span<AssetId> IDs => BlitIDs.Span;

    public void Dispose() {
        Buffer.Dispose();
        BlitIDs.Dispose();
    }
}

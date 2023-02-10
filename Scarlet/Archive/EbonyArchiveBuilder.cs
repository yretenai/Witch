using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using DragonLib;
using Scarlet.Exceptions;
using Scarlet.Structures;
using Scarlet.Structures.Archive;
using Serilog;

namespace Scarlet.Archive;

public readonly record struct EbonyArchiveBuilder {
    public delegate IMemoryOwner<byte> DataDelegate(RebuildRecord file);

    public EbonyArchiveBuilder(EbonyArchive archive) {
        Archive = archive;
        Header = archive.Header;

        Records.EnsureCapacity(archive.FileEntries.Length);
        foreach (var entry in archive.FileEntries) {
            var record = new RebuildRecord(entry.Id, entry.Flags, entry.GetPath(archive.Buffer), entry.GetDataPath(archive.Buffer));

            Records.Add(record);

            FileLookup[entry.Id] = record;
        }
    }

    public EbonyArchiveBuilder() {
        Header = new EbonyArchiveHeader {
            VersionMajor = 20,
            VersionMinor = 3,
            BlockSize = 16,
            ChunkSize = 16,
            Flags = EbonyArchiveFlags.None,
        };
    }

    private Dictionary<AssetId, RebuildRecord> FileLookup { get; } = new();

    private EbonyArchive Archive { get; }
    private EbonyArchiveHeader Header { get; }
    private List<RebuildRecord> Records { get; } = new();

    private IMemoryOwner<byte> GetFile(RebuildRecord file) {
        var entry = FileLookup[file.AssetId];
        if (entry.DataDelegate != null) {
            return entry.DataDelegate(file);
        }

        return Archive.IdMap.TryGetValue(entry.AssetId, out var value) ? Archive.Read(Archive.FileEntries[value]) : MemoryOwner<byte>.Empty;
    }

    public void AddOrReplaceFile(RebuildRecord entry) {
        if (!FileLookup.ContainsKey(entry.AssetId)) {
            Records.Add(entry);
        }

        FileLookup[entry.AssetId] = entry;
    }

    public void ReplaceFile(AssetId assetId, DataDelegate @delegate) {
        if (!FileLookup.ContainsKey(assetId)) {
            throw new AssetIdNotFoundException();
        }

        FileLookup[assetId] = FileLookup[assetId] with { DataDelegate = @delegate };
    }

    public void Build(Stream output) {
        output.SetLength(0);

        var fileTableOffset = (uint) EbonyArchiveHeader.StructSize;
        var dataPathOffset = (uint) (fileTableOffset + EbonyArchiveFile.StructSize * Records.Count).Align(Header.BlockSize);
        var pathOffset = (uint) (dataPathOffset + Records.Sum(x => Encoding.UTF8.GetByteCount(x.DataPath) + 1)).Align(Header.BlockSize);
        var dataOffset = (uint) (pathOffset + Records.Sum(x => Encoding.UTF8.GetByteCount(x.Path) + 1)).Align(Header.BlockSize);

        var prebuild = MemoryOwner<byte>.Allocate((int) dataOffset);

        var header = new EbonyArchiveHeader {
            Magic = EbonyArchive.MAGIC,
            VersionMajor = Header.VersionMajor,
            VersionMinor = Header.VersionMinor,
            FileCount = (uint) Records.Count,
            BlockSize = Header.BlockSize,
            FATOffset = fileTableOffset,
            DNTOffset = dataPathOffset,
            FNTOffset = pathOffset,
            DataOffset = dataOffset,
            Flags = Header.Flags,
            ChunkSize = Header.ChunkSize,
            Checksum = 0,
        };
        MemoryMarshal.Write(prebuild.Span, ref header);

        output.SetLength(dataOffset);
        output.Seek(dataOffset, SeekOrigin.Begin);

        var fileTableCursor = fileTableOffset;
        var dataPathCursor = dataPathOffset;
        var pathCursor = pathOffset;
        var dataSize = (long) dataOffset;
        var fileSizeReal = dataSize;

        foreach (var record in Records.OrderBy(x => x.AssetId.Type.Value is TypeIdRegistry.EREP)) {
            Log.Information("Writing {Record}", record.DataPath);
            using var buffer = GetFile(record);
            var nextOffset = dataSize + buffer.Memory.Length.Align((int) header.BlockSize) + header.BlockSize;

            var plainFlags = record.Flags & (EbonyArchiveFileFlags.AutoLoad | EbonyArchiveFileFlags.Reference | EbonyArchiveFileFlags.Loose | EbonyArchiveFileFlags.Patched | EbonyArchiveFileFlags.Deleted | EbonyArchiveFileFlags.SkipObfuscation);
            if (header.VersionMinor < 0) {
                plainFlags |= EbonyArchiveFileFlags.SkipObfuscation;
            }

            var file = new EbonyArchiveFile {
                Id = record.AssetId,
                Size = buffer.Memory.Length,
                CompressedSize = buffer.Memory.Length,
                Flags = plainFlags,
                DataPathOffset = (int) dataPathCursor,
                DataOffset = dataSize,
                PathOffset = (int) pathCursor,
                Type = 0,
                Locale = 0,
                Seed = 0,
            };

            fileSizeReal = file.DataOffset + buffer.Memory.Length;

            MemoryMarshal.Write(prebuild.Span[(int) fileTableCursor..], ref file);
            fileTableCursor += (uint) EbonyArchiveFile.StructSize;

            var dataPath = Encoding.UTF8.GetBytes(record.DataPath);
            dataPath.CopyTo(prebuild.Span[(int) dataPathCursor..]);
            dataPathCursor += (uint) (dataPath.Length + 1);

            var path = Encoding.UTF8.GetBytes(record.Path);
            path.CopyTo(prebuild.Span[(int) pathCursor..]);
            pathCursor += (uint) (path.Length + 1);

            output.Write(buffer.Memory.Span);
            output.Seek(nextOffset - 1, SeekOrigin.Begin);
            output.WriteByte(0);

            dataSize = nextOffset;
        }

        output.Seek(0, SeekOrigin.Begin);
        output.Write(prebuild.Span);
        output.Seek(0, SeekOrigin.Begin);
        header.Checksum = EbonyArchive.CalculateHash(output, fileSizeReal, true);
        var span = new Span<EbonyArchiveHeader>(ref header);
        output.Write(span.AsBytes());
    }

    public record RebuildRecord(AssetId AssetId, EbonyArchiveFileFlags Flags, string Path, string DataPath) {
        public DataDelegate? DataDelegate { get; set; }
    }
}

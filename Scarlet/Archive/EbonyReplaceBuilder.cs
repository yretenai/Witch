using System.Buffers;
using System.Buffers.Binary;
using CommunityToolkit.HighPerformance.Buffers;
using Scarlet.Exceptions;
using Scarlet.Structures;

namespace Scarlet.Archive;

public readonly record struct EbonyReplaceBuilder {
    public EbonyReplaceBuilder(EbonyReplace replace) {
        Replacements = replace.Replacements;
    }

    public EbonyReplaceBuilder() {
        Replacements = new Dictionary<AssetId, AssetId>();
    }

    public IMemoryOwner<byte> Build(EbonyArchiveBuilder.RebuildRecord file) {
        if (file.AssetId.Type.Value is not TypeIdRegistry.EREP) {
            throw new TypeIdMismatchException("Expected to build an EREP file!");
        }

        var size = Replacements.Count * 16;
        var buffer = MemoryOwner<byte>.Allocate(size);

        var offset = 0;
        foreach (var (key, value) in Replacements.OrderBy(x => x.Key.Path)) {
            BinaryPrimitives.WriteUInt64LittleEndian(buffer.Span[offset..], key.Value);
            BinaryPrimitives.WriteUInt64LittleEndian(buffer.Span[(offset + 8)..], value.Value);
            offset += 16;
        }

        return buffer;
    }

    public Dictionary<AssetId, AssetId> Replacements { get; init; }
}

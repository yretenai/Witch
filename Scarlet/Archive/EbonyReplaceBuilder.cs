using System.Buffers;
using System.Buffers.Binary;
using CommunityToolkit.HighPerformance.Buffers;
using Scarlet.Exceptions;
using Scarlet.Structures;
using Serilog;

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

    private Dictionary<AssetId, AssetId> Replacements { get; }

    public void Replace(AssetId source, AssetId target) {
        Log.Information("Replacing {Source} with {Target}", source, target);
        Replacements[source] = target;

        var rebounces = Replacements.Where(x => x.Value == source).Select(x => x.Key).ToArray();
        foreach (var rebounce in rebounces) {
            Log.Information("Bouncing {Source} to {Target}", rebounce, target);
            Replacements[rebounce] = target;
        }
    }
}

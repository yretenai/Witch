using System.Buffers;
using Scarlet.Exceptions;
using Scarlet.Structures;
using Scarlet.Structures.Gfx;

namespace Scarlet.Gfx;

// GMDL stands for either "GPU" or "Graphics" Model.
public readonly record struct GraphicsModel : IAsset {
    public GraphicsModel(AssetId assetId, IMemoryOwner<byte> buffer) {
        if (assetId.Type.Value is not TypeIdRegistry.GMDL or TypeIdRegistry.GMDL_HAIR or TypeIdRegistry.GMDL_GFXBIN) {
            throw new TypeIdMismatchException(assetId, TypeIdRegistry.GMDL);
        }

        AssetId = assetId;

        var msgPack = new MessagePackBuffer(buffer.Memory);

        Header = new GraphicsBinaryHeader {
            Version = msgPack.Read<uint>(),
            Dependencies = msgPack.Read<Dictionary<string, string>>() ?? new Dictionary<string, string>(0),
            Ids = msgPack.Read<AssetId[]>() ?? Array.Empty<AssetId>(),
        };

        if (Header.Version is not GraphicsBinaryHeader.WitchVersion) {
            throw new NotSupportedException($"gmdl version {Header.Version} is not supported!");
        }
    }

    public AssetId AssetId { get; }
    public GraphicsBinaryHeader Header { get; }
}

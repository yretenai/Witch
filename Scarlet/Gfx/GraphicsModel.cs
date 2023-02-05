using System.Buffers;
using Scarlet.Exceptions;
using Scarlet.Structures;
using Scarlet.Structures.Gfx.Model;

namespace Scarlet.Gfx;

// GMDL stands for either "GPU" or "Graphics" Model.
public readonly record struct GraphicsModel : IAsset {
    public GraphicsModel(AssetId assetId, IMemoryOwner<byte> buffer) {
        if (assetId.Type.Value is not (TypeIdRegistry.GMDL or TypeIdRegistry.GMDL_HAIR or TypeIdRegistry.GMDL_GFXBIN)) {
            throw new TypeIdMismatchException(assetId, TypeIdRegistry.GMDL);
        }

        AssetId = assetId;

        var msgPack = new MessagePackBuffer(buffer.Memory);

        Header = msgPack.Read<GraphicsBinaryHeader>();

        Data = Header.Version switch {
                   GraphicsBinaryHeader.WitchVersion        => msgPack.Read<GraphicsModelData_Witch>()!,
                   GraphicsBinaryHeader.BlackVersion        => throw new NotImplementedException(),
                   >= GraphicsBinaryHeader.BlackDemoVersion => throw new NotImplementedException(),
                   _                                        => throw new NotSupportedException($"gmdl version {Header.Version} is not supported!"),
               };
    }

    public AssetId AssetId { get; }
    public GraphicsBinaryHeader Header { get; }
    public IGraphicsModelData Data { get; }
}

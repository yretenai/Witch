using System.Buffers;
using Scarlet.Structures;
using Scarlet.Structures.Gfx;

namespace Scarlet.Gfx;

public readonly record struct GraphicsModel {
    public GraphicsModel(IMemoryOwner<byte> buffer) {
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

    public GraphicsBinaryHeader Header { get; }
}

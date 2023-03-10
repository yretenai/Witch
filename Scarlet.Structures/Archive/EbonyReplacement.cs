using System.Runtime.InteropServices;
using Scarlet.Structures.Id;

namespace Scarlet.Structures.Archive;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct EbonyReplacement {
    public AssetId AssetId { get; init; }
    public AssetId ReplacementAssetId { get; init; }
}

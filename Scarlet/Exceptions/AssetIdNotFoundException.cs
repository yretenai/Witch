using Scarlet.Structures;

namespace Scarlet.Exceptions;

public sealed class AssetIdNotFoundException : Exception {
    public AssetIdNotFoundException(string message, Exception innerException, AssetId assetId) : base(message, innerException) => AssetId = assetId;

    public AssetIdNotFoundException(AssetId assetId) : base($"AssetId {assetId} is not found") => AssetId = assetId;

    public AssetIdNotFoundException(string message, AssetId assetId) : base(message) => AssetId = assetId;

    public AssetIdNotFoundException() { }

    public AssetIdNotFoundException(string message) : base(message) { }

    public AssetIdNotFoundException(string message, Exception innerException) : base(message, innerException) { }

    public AssetId AssetId { get; }
}

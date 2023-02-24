using System.Text;
using DragonLib.Hash;
using DragonLib.Hash.Basis;

namespace Scarlet.Structures.Id;

public readonly record struct AssetId {
    public AssetId(ulong value) => Value = value;
    public AssetId(uint typeId, ulong path) => Value = ((ulong) typeId << 44) | (path & 0xFFFFFFFFFFF);
    public AssetId(uint typeId) => Value = (ulong) typeId << 44;
    public AssetId(string path) : this(path, System.IO.Path.GetExtension(path)[1..]) { }

    public AssetId(string path, string ext) {
        using var fnv = FowlerNollVo.CreateAlternate((FNV64Basis) 0x14650FB0739D0383);
        var pathHash = fnv.ComputeHashValue(Encoding.UTF8.GetBytes(path));

        fnv.Reset(0x14650FB0739D0383);
        var typeHash = fnv.ComputeHashValue(Encoding.UTF8.GetBytes(ext.TrimEnd('@')));

        Value = (typeHash << 44) | (pathHash & 0xFFFFFFFFFFF);
    }

    public AssetId(string path, uint typeHash) {
        using var fnv = FowlerNollVo.CreateAlternate((FNV64Basis) 0x14650FB0739D0383);
        var pathHash = fnv.ComputeHashValue(Encoding.UTF8.GetBytes(path));

        Value = ((ulong) typeHash << 44) | (pathHash & 0xFFFFFFFFFFF);
    }

    public static AssetId Zero { get; } = new(0);
    public ulong Value { get; init; }
    public bool IsNull => Value == 0;

    public TypeId Type => (TypeId) (Value >> 44);
    public ulong Path => Value & 0xFFFFFFFFFFF;
    public static AssetId Parse(string value) => ulong.Parse(value);
    public static implicit operator AssetId(ulong value) => new(value: value);
    public static implicit operator AssetId(TypeId value) => new(value: (ulong) value.Value << 44);
    public static implicit operator ulong(AssetId id) => id.Value;
    public override string ToString() => AssetIdRegistry.IdTable.TryGetValue(this, out var result) || AssetIdRegistry.IdTable.TryGetValue(Path, out result) ? result + $"{{{Type}}}" : Path.ToString("x11") + $".{Type}";
    public bool Equals(AssetId? other) => other?.Value == Value;
    public override int GetHashCode() => Value.GetHashCode();
}

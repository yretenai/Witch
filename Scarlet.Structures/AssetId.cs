using System.Text;
using DragonLib.Hash;
using DragonLib.Hash.Basis;

namespace Scarlet.Structures;

public readonly record struct AssetId {
    public ulong Value { get; init; }

    public TypeId Type => (TypeId)(Value >> 44);
    public ulong Path => Value & 0xFFFFFFFFFFF;

    public AssetId(ulong value) => Value = value;
    public AssetId(uint typeId, ulong path) => Value = (ulong)typeId << 44 | (path & 0xFFFFFFFFFFF);
    public AssetId(string path) : this(path, System.IO.Path.GetExtension(path)[1..]) { }
    public AssetId(string path, string ext) {
        using var fnv = FowlerNollVo.CreateAlternate((FNV64Basis) 0x14650FB0739D0383);
        var pathHash = fnv.ComputeHashValue(Encoding.UTF8.GetBytes(path.TrimEnd('@')));

        fnv.Reset(0x14650FB0739D0383);
        var typeHash = fnv.ComputeHashValue(Encoding.UTF8.GetBytes(ext.TrimEnd('@')));

        Value = typeHash << 44 | (pathHash & 0xFFFFFFFFFFF);
    }

    public static AssetId Parse(string value) => ulong.Parse(value);
    public static implicit operator AssetId(ulong value) => new() { Value = value };
    public static implicit operator AssetId(TypeId value) => new() { Value = (ulong)value.Value << 44 };
    public static implicit operator ulong(AssetId id) => id.Value;
    public static Dictionary<ulong, string> IdTable { get; set; } = new();
    public override string ToString() => IdTable.TryGetValue(this, out var result) || IdTable.TryGetValue(Path, out result) ? result : Value.ToString("X16");
    public bool Equals(AssetId? other) => other?.Value == Value;
    public override int GetHashCode() => Value.GetHashCode();
}

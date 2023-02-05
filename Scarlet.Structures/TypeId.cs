using System.Text;
using DragonLib.Hash;
using DragonLib.Hash.Basis;

namespace Scarlet.Structures;

public readonly record struct TypeId {
    public uint Value { get; init; }

    public TypeId(ulong value) => Value = (uint) (value >> 44);
    public TypeId(uint typeId) => Value = typeId;
    public TypeId(string path) {
        using var fnv = FowlerNollVo.CreateAlternate((FNV64Basis) 0x14650FB0739D0383);
        Value = (uint) (fnv.ComputeHashValue(Encoding.UTF8.GetBytes(path.TrimEnd('@'))) & 0xFFFFF);
    }

    public static implicit operator TypeId(uint value) => new() { Value = value };
    public static implicit operator uint(TypeId id) => id.Value;
    public static TypeId Parse(string value) => uint.Parse(value);
    public override string ToString() => TypeIdRegistry.IdTable.TryGetValue(this, out var result) ? result : Value.ToString("X16");
    public bool Equals(TypeId? other) => other?.Value == Value;
    public override int GetHashCode() => Value.GetHashCode();
}

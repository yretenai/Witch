namespace Scarlet.Structures.Id;

public readonly record struct FixId(uint Value) {
    public static TypeId Zero { get; } = new(0);
    public bool IsNull => Value == 0;

    public static implicit operator FixId(uint value) => new() { Value = value };
    public static implicit operator uint(FixId id) => id.Value;
    public static FixId Parse(string value) => uint.Parse(value);
    public override string ToString() => FixIdRegistry.IdTable.TryGetValue(this, out var result) ? result : Value.ToString("x8");
    public bool Equals(TypeId? other) => other?.Value == Value;
    public override int GetHashCode() => Value.GetHashCode();
}

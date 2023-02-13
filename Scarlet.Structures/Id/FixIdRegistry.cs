namespace Scarlet.Structures.Id;

public static class FixIdRegistry {
    static FixIdRegistry() {
        RegistryHelper.LoadRegistry(nameof(FixId), IdTable, uint.TryParse);
    }

    public static Dictionary<uint, string> IdTable { get; set; } = new();

    public const uint NULL = 0x01000000;
}

using System.Globalization;
using System.Reflection;

namespace Scarlet.Structures.Id;

internal static class RegistryHelper {
    public delegate bool TryParse<T>(string text, NumberStyles style, IFormatProvider? provider, out T value);

    public static void LoadRegistry<T>(string name, Dictionary<T, string> idTable, TryParse<T> parse) where T : notnull {
        var assembly = Assembly.GetExecutingAssembly();
        using var resource = assembly.GetManifestResourceStream($"Scarlet.Structures.Id.{name}.registry");
        if (resource == null) {
            return;
        }

        using var reader = new StreamReader(resource);
        reader.ReadLine(); // skip header.
        while (reader.ReadLine() is { } line) {
            var parts = line.Split('#', StringSplitOptions.TrimEntries)[0].Split(',', 2, StringSplitOptions.TrimEntries);
            if (parts.Length < 2 || parts[0].Length == 0 || parts[1].Length == 0) {
                continue;
            }

            if (parse(parts[0], NumberStyles.HexNumber, null, out var value)) {
                if (!idTable.ContainsKey(value)) {
                    idTable[value] = parts[1];
                }
            }
        }
    }
}

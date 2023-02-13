using DragonLib.CommandLine;
using Scarlet.Structures;
using Scarlet.Structures.Id;
using Serilog;

namespace Witch.Commands.Debug;

[Command(typeof(CommandLineFlags), "brute-typeid", "", "debug", true)]
public class DebugBruteTypeId {
    public DebugBruteTypeId(CommandLineFlags flags) {
        var tests = new HashSet<TypeId>();
        foreach (var positional in flags.Positionals.Skip(2)) {
            tests.Add(TypeId.Parse(positional));
        }

        if (tests.Count == 0) {
            return;
        }

        for (var i = 1; i < 10; ++i) {
            var text = new char[i];
            Array.Fill(text, '.');
            IterateKey(tests, 0, text);
        }
    }

    private static void IterateKey(IReadOnlySet<TypeId> tests, int i, char[] k) {
        for (var j = 0; j < 27; j++) {
            k[i] = j == 26 ? '.' : (char) ('a' + j);

            if (i < k.Length - 1) {
                IterateKey(tests, i + 1, k);
            } else {
                var str = new string(k);
                var type = new TypeId(str);

                if (tests.Contains(type)) {
                    Log.Information("{Test} = {Hash}", str, type.Value);
                }
            }
        }
    }
}

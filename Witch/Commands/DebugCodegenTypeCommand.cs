using System.Text;
using DragonLib.CommandLine;
using Scarlet;
using Scarlet.Structures;
using Serilog;

namespace Witch.Commands;

[Command(typeof(WitchFlags), "codegen-typeid", "", "debug", true)]
public class DebugCodegenTypeCommand : EARCCommand {
    public DebugCodegenTypeCommand(WitchFlags flags) : base(flags) {
        var ids = TypeIdRegistry.IdTable;
        foreach (var (fileId, resource) in AssetManager.Instance.IdTable) {
            var (archive, file) = resource.Deconstruct();
            var dataPath = resource.DataPath;
            var realPath = file.GetPath(archive.Buffer);
            var dataName = Path.GetFileName(dataPath);
            var realName = Path.GetFileName(realPath);

            if (!ids.ContainsKey(fileId.Type)) {
                var attempt = 0;
                var stage = 0;
                var ext = Path.GetExtension(realName)[1..].TrimEnd('@');
                var test = new TypeId(ext);

                // heuristically find the type id from the paths.
                while (test != fileId.Type) {
                    switch (attempt++) {
                        case 0:
                            ext = Path.GetExtension(dataName)[1..].TrimEnd('@');
                            test = new TypeId(ext);
                            break;

                        case 1:
                            ids[test] = ext;
                            ext = realName[realName.IndexOf('.', StringComparison.Ordinal)..][1..].TrimEnd('@');
                            test = new TypeId(ext);
                            break;

                        default:
                            var index = ext.IndexOf('.', StringComparison.Ordinal);
                            if (index == -1) {
                                if (stage++ == 1) {
                                    stage = -1;
                                    Log.Error("Cannot find type id for paths {Path} {Uri} and hash {Id:X16}", dataName, realName, fileId.Type.Value);
                                    break;
                                }

                                ext = dataName[index..][1..].TrimEnd('@');
                                test = new TypeId(ext);
                            }

                            break;
                    }
                }

                if (stage > -1) {
                    ids[fileId.Type] = ext;
                }
            }
        }

        var dictionary = new StringBuilder();
        var consts = new StringBuilder();

        foreach (var (typeId, ext) in ids.OrderBy(x => x.Value)) {
            dictionary.AppendLine($"        {{ 0x{typeId:X}u, \"{ext}\" }},");
            consts.AppendLine($"    public const uint {ext.ToUpper().Replace('.', '_')} = 0x{typeId:X}u;");
        }

        Console.Error.WriteLine("public static class TypeIdRegistry {");
        Console.Error.WriteLine("    public static Dictionary<uint, string> IdTable { get; set; } = new() {");
        Console.Error.Write(dictionary.ToString());
        Console.Error.WriteLine("    };");
        Console.Error.WriteLine();
        Console.Error.Write(consts.ToString());
        Console.Error.WriteLine("}");
    }
}

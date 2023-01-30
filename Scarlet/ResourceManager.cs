using System.Collections;
using CommunityToolkit.HighPerformance.Buffers;
using Scarlet.Archive;
using Scarlet.Structures;
using Scarlet.Structures.Archive;

namespace Scarlet;

public sealed class ResourceManager : IDisposable {
    public static ResourceManager Instance { get; } = new();

    public List<EARC> EARC { get; } = new();
    public EREP? EREP { get; private set; }
    public Hashtable FileTable { get; private set; } = new();
    public Hashtable IdTable { get; private set; } = new();
    public Hashtable UriTable { get; private set; } = new();
    public Hashtable UriLookup { get; private set; } = new();

    public void Dispose() {
        foreach (var archive in EARC) {
            archive.Dispose();
        }

        EREP?.Dispose();
    }

    public void LoadEARC(string file) {
        var archive = new EARC(file, file.EndsWith(".emem", StringComparison.OrdinalIgnoreCase));
        EARC.Add(archive);
    }

    public bool TryResolveDataPath(string dataPath, out FileReference reference) {
        if (UriLookup[dataPath] is not string path) {
            reference = default;
            return false;
        }

        reference = (FileReference) FileTable[path]!;
        return true;
    }

    public void Build() {
        using var _perf = new PerformanceCounter<PerformanceHost.ResourceManager>();

        var count = (int) EARC.Sum(x => x.Header.FileCount);
        FileTable = new Hashtable(count);
        IdTable = new Hashtable(count);
        UriTable = new Hashtable(count);
        UriLookup = new Hashtable(count);

        for (var archiveIndex = 0; archiveIndex < EARC.Count; archiveIndex++) {
            var archive = EARC[archiveIndex];
            for (var fileIndex = 0; fileIndex < archive.FileEntries.Length; fileIndex++) {
                var file = archive.FileEntries[fileIndex];
                var dataPath = file.GetDataPath(archive.Buffer);
                var path = file.GetPath(archive.Buffer);
                UriTable[file.Id] = dataPath;
                UriLookup[dataPath] = path;

                if ((file.Flags & EARCFileFlags.Reference) != 0) {
                    continue;
                }

                if ((file.Flags & EARCFileFlags.Deleted) != 0) {
                    continue;
                }

                if ((file.Flags & EARCFileFlags.Loose) != 0) {
                    continue;
                }

                var reference = new FileReference(archiveIndex, fileIndex);
                FileTable[path] = reference;
                FileTable[dataPath] = reference;
                IdTable[file.Id] = reference;

                if (dataPath.EndsWith(".erep")) {
                    EREP = ReadFile<EREP>(archive, file);
                }
            }
        }

        FileId.IdTable = UriTable;
    }

    public T? ReadFile<T>(in string path) where T : class, new() {
        if (FileTable[path] is FileReference reference) {
            var archive = EARC[reference.EARCIndex];
            return ReadFile<T>(archive, archive.FileEntries[reference.FileIndex]);
        }

        return null;
    }

    public T? ReadFile<T>(in FileId path) where T : class, new() {
        if (IdTable[path] is FileReference reference) {
            var archive = EARC[reference.EARCIndex];
            return ReadFile<T>(archive, archive.FileEntries[reference.FileIndex]);
        }

        return null;
    }

    public static T? ReadFile<T>(EARC earc, in EARCFile file) where T : class, new() {
        var data = earc.ReadFile(file);
        var instance = Activator.CreateInstance(typeof(T), data) as T;
        if (instance is not IDisposable) {
            data.Dispose();
        }

        return instance;
    }


    public MemoryOwner<byte>? ReadFile(in string path) {
        if (FileTable[path] is FileReference reference) {
            var archive = EARC[reference.EARCIndex];
            return archive.ReadFile(archive.FileEntries[reference.FileIndex]);
        }

        return null;
    }

    public MemoryOwner<byte>? ReadFile(in FileId path) {
        if (IdTable[path] is FileReference reference) {
            var archive = EARC[reference.EARCIndex];
            return archive.ReadFile(archive.FileEntries[reference.FileIndex]);
        }

        return null;
    }

    public readonly record struct FileReference(int EARCIndex, int FileIndex) {
        public EARC EARC => Instance.EARC[EARCIndex];
        public EARCFile File => EARC.FileEntries[FileIndex];

        public (EARC EARC, EARCFile File) Deconstruct() => (EARC, File);
    }
}

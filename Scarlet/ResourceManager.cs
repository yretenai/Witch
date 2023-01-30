using System.Collections;
using CommunityToolkit.HighPerformance.Buffers;
using Scarlet.Archive;
using Scarlet.Structures;
using Scarlet.Structures.Archive;

namespace Scarlet;

public class ResourceManager : IDisposable {
    public readonly record struct FileReference(int ArchiveIndex, int FileIndex);

    public static ResourceManager Instance { get; } = new();

    public List<EARC> Archives { get; } = new();
    public EREP? Repair { get; private set; }
    public Hashtable FileTable { get; private set; } = new();
    public Hashtable IdTable { get; private set; } = new();
    public Hashtable UriTable { get; private set; } = new();

    public void LoadEARC(string file) {
        var archive = new EARC(file, file.EndsWith(".emem", StringComparison.OrdinalIgnoreCase));
        Archives.Add(archive);
    }

    public void Build() {
        using var _perf = new PerformanceCounter<PerformanceHost.ResourceManager>();

        var count = (int) Archives.Sum(x => x.Header.FileCount);
        FileTable = new Hashtable(count);
        IdTable = new Hashtable(count);
        UriTable = new Hashtable(count);

        for (var archiveIndex = 0; archiveIndex < Archives.Count; archiveIndex++) {
            var archive = Archives[archiveIndex];
            for (var fileIndex = 0; fileIndex < archive.FileEntries.Length; fileIndex++) {
                var file = archive.FileEntries[fileIndex];
                var path = file.GetDataPath(archive.Buffer);
                UriTable[file.Id] = path;

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
                FileTable[file.GetPath(archive.Buffer)] = reference;
                FileTable[path] = reference;
                IdTable[file.Id] = reference;

                if (path.EndsWith(".erep")) {
                    Repair = ReadFile<EREP>(archive, file);
                }
            }
        }

        FileId.IdTable = UriTable;
    }

    public T? ReadFile<T>(in string path) where T : class, new() {
        if (FileTable[path] is FileReference reference) {
            var archive = Archives[reference.ArchiveIndex];
            return ReadFile<T>(archive, archive.FileEntries[reference.FileIndex]);
        }

        return null;
    }

    public T? ReadFile<T>(in FileId path) where T : class, new() {
        if (IdTable[path] is FileReference reference) {
            var archive = Archives[reference.ArchiveIndex];
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
            var archive = Archives[reference.ArchiveIndex];
            return archive.ReadFile(archive.FileEntries[reference.FileIndex]);
        }

        return null;
    }

    public MemoryOwner<byte>? ReadFile(in FileId path) {
        if (IdTable[path] is FileReference reference) {
            var archive = Archives[reference.ArchiveIndex];
            return archive.ReadFile(archive.FileEntries[reference.FileIndex]);
        }

        return null;
    }

    protected void ReleaseUnmanagedResources() { }

    protected virtual void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            foreach (var archive in Archives) {
                archive.Dispose();
            }

            Repair?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ResourceManager()
    {
        Dispose(false);
    }
}

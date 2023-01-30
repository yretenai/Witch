using System.Collections;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.HighPerformance.Buffers;
using Scarlet.Archive;
using Scarlet.Structures;
using Scarlet.Structures.Archive;

namespace Scarlet;

public sealed class ResourceManager : IDisposable {
    public static ResourceManager Instance { get; } = new();

    public List<EbonyArchive> Archives { get; } = new();
    public List<EbonyRepair> Repairs { get; } = new();
    public Hashtable FileTable { get; private set; } = new();
    public Hashtable IdTable { get; private set; } = new();
    public Hashtable UriTable { get; private set; } = new();
    public Hashtable UriLookup { get; private set; } = new();

    public void Dispose() {
        foreach (var archive in Archives) {
            archive.Dispose();
        }

        foreach (var repair in Repairs) {
            repair.Dispose();
        }
    }

    public void LoadArchive(string file) {
        var archive = new EbonyArchive(file, file.EndsWith(".emem", StringComparison.OrdinalIgnoreCase));
        Archives.Add(archive);
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

        var count = (int) Archives.Sum(x => x.Header.FileCount);
        FileTable = new Hashtable(count);
        IdTable = new Hashtable(count);
        UriTable = new Hashtable(count);
        UriLookup = new Hashtable(count);

        for (var archiveIndex = 0; archiveIndex < Archives.Count; archiveIndex++) {
            var archive = Archives[archiveIndex];
            for (var fileIndex = 0; fileIndex < archive.FileEntries.Length; fileIndex++) {
                var file = archive.FileEntries[fileIndex];
                var dataPath = file.GetDataPath(archive.Buffer);
                var path = file.GetPath(archive.Buffer);
                UriTable[file.Id] = dataPath;
                UriLookup[dataPath] = path;

                if ((file.Flags & EbonyArchiveFileFlags.Reference) != 0) {
                    continue;
                }

                if ((file.Flags & EbonyArchiveFileFlags.Deleted) != 0) {
                    continue;
                }

                if ((file.Flags & EbonyArchiveFileFlags.Loose) != 0) {
                    continue;
                }

                var reference = new FileReference(archiveIndex, fileIndex);
                FileTable[path] = reference;
                FileTable[dataPath] = reference;
                IdTable[file.Id] = reference;

                if (dataPath.EndsWith(".erep")) {
                    if (TryCreate<EbonyRepair>(archive, file, out var data)) {
                        Repairs.Add(data);
                    }
                }
            }
        }

        FileId.IdTable = UriTable;
    }

    public bool TryCreate<T>(in string path, [MaybeNullWhen(false)] out T instance) where T : new() {
        if (FileTable[path] is FileReference reference) {
            var archive = Archives[reference.ArchiveIndex];
            return TryCreate(archive, archive.FileEntries[reference.FileIndex], out instance);
        }

        instance = default;
        return false;
    }

    public bool TryCreate<T>(in FileId path, [MaybeNullWhen(false)] out T instance) where T : new() {
        if (IdTable[path] is FileReference reference) {
            var archive = Archives[reference.ArchiveIndex];
            return TryCreate(archive, archive.FileEntries[reference.FileIndex], out instance);
        }

        instance = default;
        return false;
    }

    public static bool TryCreate<T>(EbonyArchive earc, in EbonyArchiveFile file, [MaybeNullWhen(false)] out T instance) where T : new() {
        var data = earc.Read(file);
        var localInstance = Activator.CreateInstance(typeof(T), data);

        if (localInstance is not IDisposable) {
            data.Dispose();
        }

        if(localInstance is T result) {
            instance = result;
            return true;
        }

        instance = default;
        return false;
    }

    public MemoryOwner<byte>? Read(in string path) {
        if (FileTable[path] is FileReference reference) {
            var archive = Archives[reference.ArchiveIndex];
            return archive.Read(archive.FileEntries[reference.FileIndex]);
        }

        return null;
    }

    public MemoryOwner<byte>? Read(in FileId path) {
        if (IdTable[path] is FileReference reference) {
            var archive = Archives[reference.ArchiveIndex];
            return archive.Read(archive.FileEntries[reference.FileIndex]);
        }

        return null;
    }

    public readonly record struct FileReference(int ArchiveIndex, int FileIndex) {
        public EbonyArchive Archive => Instance.Archives[ArchiveIndex];
        public EbonyArchiveFile File => Archive.FileEntries[FileIndex];

        public (EbonyArchive Archive, EbonyArchiveFile File) Deconstruct() => (Archive, File);
    }
}

using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.HighPerformance.Buffers;
using Scarlet.Archive;
using Scarlet.Structures;
using Scarlet.Structures.Archive;

namespace Scarlet;

public sealed class ResourceManager : IDisposable {
    public static ResourceManager Instance { get; } = new();

    public List<EbonyArchive> Archives { get; } = new();
    public List<EbonyReplace> Replacements { get; } = new();
    public Dictionary<string, FileReference> FileTable { get; } = new();
    public Dictionary<FileId, FileReference>  IdTable { get; } = new();
    public Dictionary<FileId, string>  UriTable { get; } = new();
    public Dictionary<string, string>  UriLookup { get; } = new();

    public void Dispose() {
        foreach (var archive in Archives) {
            archive.Dispose();
        }

        foreach (var replace in Replacements) {
            replace.Dispose();
        }
    }

    public void LoadArchive(string file) {
        var archive = new EbonyArchive(file, file.EndsWith(".emem", StringComparison.OrdinalIgnoreCase));
        Archives.Add(archive);
    }

    public bool TryResolveDataPath(string dataPath, out FileReference reference) {
        if (UriLookup.TryGetValue(dataPath, out var path)) {
            reference = FileTable[path];
            return true;
        }

        reference = default;
        return false;
    }

    public void Build() {
        using var _perf = new PerformanceCounter<PerformanceHost.ResourceManager>();

        var count = (int) Archives.Sum(x => x.Header.FileCount);
        FileTable.EnsureCapacity(count);
        IdTable.EnsureCapacity(count);
        UriTable.EnsureCapacity(count);
        UriLookup.EnsureCapacity(count);

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
                    if (TryCreate<EbonyReplace>(archive, file, out var data)) {
                        Replacements.Add(data);
                    }
                }
            }
        }

        FileId.IdTable = UriTable;
    }

    public bool TryCreate<T>(in string path, [MaybeNullWhen(false)] out T instance) where T : new() {
        if (FileTable.TryGetValue(path, out var reference)) {
            var archive = Archives[reference.ArchiveIndex];
            return TryCreate(archive, archive.FileEntries[reference.FileIndex], out instance);
        }

        instance = default;
        return false;
    }

    public bool TryCreate<T>(in FileId path, [MaybeNullWhen(false)] out T instance) where T : new() {
        if (IdTable.TryGetValue(path, out var reference)) {
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
        if (FileTable.TryGetValue(path, out var reference)) {
            var archive = Archives[reference.ArchiveIndex];
            return archive.Read(archive.FileEntries[reference.FileIndex]);
        }

        return null;
    }

    public MemoryOwner<byte>? Read(in FileId path) {
        if (IdTable.TryGetValue(path, out var reference)) {
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

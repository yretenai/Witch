using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.HighPerformance.Buffers;
using Scarlet.Archive;
using Scarlet.Structures;
using Scarlet.Structures.Archive;
using Serilog;

namespace Scarlet;

public sealed class AssetManager : IDisposable {
    public static AssetManager Instance { get; } = new();

    public List<EbonyArchive> Archives { get; } = new();
    public List<EbonyReplace> Replacements { get; } = new();
    public Dictionary<AssetId, FileReference> IdTable { get; } = new();
    public Dictionary<ulong, string> UriTable { get; } = new();
    public Dictionary<ulong, ulong> ExtensionTable { get; } = new();

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

    public bool TryResolveId(AssetId id, out AssetId reference) {
        if (!ExtensionTable.TryGetValue(id, out var idActual)) {
            idActual = id;
        }

        foreach (var replacement in Replacements) {
            if (replacement.Replacements.TryGetValue(idActual, out var replaced)) {
                idActual = replaced;
                break;
            }
        }

        reference = idActual;
        return IdTable.ContainsKey(reference);
    }

    public void Build() {
        using var _perf = new PerformanceCounter<PerformanceHost.AssetManager>();

        for (var archiveIndex = 0; archiveIndex < Archives.Count; archiveIndex++) {
            var archive = Archives[archiveIndex];
            for (var fileIndex = 0; fileIndex < archive.FileEntries.Length; fileIndex++) {
                var file = archive.FileEntries[fileIndex];
                var dataPath = file.GetDataPath(archive.Buffer);
                var pure = new AssetId(dataPath);
                UriTable[file.Id] = dataPath;
                if (pure != file.Id) {
                    UriTable[pure] = dataPath;
                    ExtensionTable[pure] = file.Id;
                }

                if (file.Size == 0) {
                    continue;
                }

                var reference = new FileReference(archiveIndex, fileIndex);
                IdTable[file.Id] = reference;
                if (pure != file.Id) {
                    IdTable[pure] = reference;
                }

                if (dataPath.EndsWith(".erep")) {
                    if (TryCreate<EbonyReplace>(archive, file, out var data)) {
                        Replacements.Add(data);
                    }
                }
            }
        }

        AssetId.IdTable = UriTable;
    }

    public bool TryCreate<T>(in AssetId path, [MaybeNullWhen(false)] out T instance) where T : new() {
        if (!TryResolveId(path.Value, out var pathActual)) {
            pathActual = path;
        }

        if (IdTable.TryGetValue(pathActual, out var reference)) {
            var archive = Archives[reference.ArchiveIndex];
            return TryCreate(archive, archive.FileEntries[reference.FileIndex], out instance);
        }

        instance = default;
        return false;
    }

    public static bool TryCreate<T>(EbonyArchive earc, in EbonyArchiveFile file, [MaybeNullWhen(false)] out T instance) where T : new() {
        var data = earc.Read(file);

        object? localInstance = null;
        try {
            localInstance = Activator.CreateInstance(typeof(T), data);
        } catch (Exception e) {
            Log.Error(e, "Failed to deserialize {File}", file.ToString());

            if (Debugger.IsAttached) {
                throw;
            }
        }

        if (localInstance is not IDisposable) {
            data.Dispose();
        }

        if (localInstance is T result) {
            instance = result;
            return true;
        }

        instance = default;
        return false;
    }

    public MemoryOwner<byte>? Read(in AssetId path) {
        if (!TryResolveId(path.Value, out var pathActual)) {
            pathActual = path;
        }

        if (IdTable.TryGetValue(pathActual, out var reference)) {
            var archive = Archives[reference.ArchiveIndex];
            return archive.Read(archive.FileEntries[reference.FileIndex]);
        }

        return null;
    }

    public readonly record struct FileReference(int ArchiveIndex, int FileIndex) {
        public EbonyArchive Archive => Instance.Archives[ArchiveIndex];
        public EbonyArchiveFile File => Archive.FileEntries[FileIndex];

        public (EbonyArchive Archive, EbonyArchiveFile File) Deconstruct() => (Archive, File);

        public MemoryOwner<byte> Read() => Archive.Read(File);
        public bool TryCreate<T>([MaybeNullWhen(false)] out T instance) where T : new() => AssetManager.TryCreate(Archive, File, out instance);
    }
}

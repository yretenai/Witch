using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.HighPerformance.Buffers;
using Scarlet.Archive;
using Scarlet.Exceptions;
using Scarlet.Structures;
using Scarlet.Structures.Archive;
using Serilog;

namespace Scarlet;

public sealed class AssetManager : IDisposable {
    public static AssetManager Instance { get; } = new();

    public static EbonyGame Game { get; set; } = EbonyGame.Witch;

    public ConcurrentDictionary<AssetId, EbonyArchive> Archives { get; } = new();
    public ConcurrentDictionary<AssetId, EbonyReplace> Replacements { get; } = new();
    public Dictionary<AssetId, FileReference> UriTable { get; } = new();
    public Dictionary<AssetId, FileReference> PureUriTable { get; } = new();
    public Dictionary<ulong, ulong> ExtensionTable { get; } = new();

    public void Dispose() {
        foreach (var (_, archive) in Archives) {
            archive.Dispose();
        }
    }

    public void LoadArchive(string file) {
        var isEmem = file.EndsWith(".emem", StringComparison.OrdinalIgnoreCase);
        var name = $"data://{Path.GetFileNameWithoutExtension(file)}.{(isEmem ? "emem" : "ebex")}";
        var id = new AssetId(name,  isEmem ? TypeIdRegistry.EMEM : TypeIdRegistry.EARC);
        var archive = new EbonyArchive(id, name, new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        Archives[id] = archive;
    }

    public bool TryResolveId(AssetId id, out AssetId reference) {
        if (!ExtensionTable.TryGetValue(id, out var idActual)) {
            idActual = id;
        }

        foreach (var (_, replacement) in Replacements) {
            if (replacement.Replacements.TryGetValue(idActual, out var replaced)) {
                idActual = replaced;
                break;
            }
        }

        reference = idActual;
        return UriTable.ContainsKey(reference);
    }

    public void Build() {
        using var _perf = new PerformanceCounter<PerformanceHost.AssetManager>();

        foreach (var (assetId, archive) in Archives) {
            UriTable[assetId] = new FileReference(assetId, AssetId.Zero, archive.DataPath);

            foreach (var file in archive.FileEntries) {
                var dataPath = file.GetDataPath(archive.Buffer);
                var pure = new AssetId(dataPath);
                var reference = new FileReference(assetId, file.Id, dataPath);
                UriTable[file.Id] = reference;
                PureUriTable[file.Id] = reference;
                if (pure != file.Id) {
                    UriTable[pure] = reference;
                    ExtensionTable[pure] = file.Id;
                }

                if (dataPath.EndsWith(".erep")) {
                    Replacements[assetId] = reference.Create<EbonyReplace>();
                }
            }
        }

        AssetId.IdTable = UriTable.ToDictionary(x => x.Key.Value, x => x.Value.DataPath);
    }

    public bool TryCreate<T>(in AssetId path, [MaybeNullWhen(false)] out T instance) where T : IAsset, new() {
        if (!TryResolveId(path.Value, out var pathActual)) {
            pathActual = path;
        }

        if (UriTable.TryGetValue(pathActual, out var reference)) {
            instance = reference.Create<T>();
            return true;
        }

        instance = default;
        return false;
    }

    public T Create<T>(in AssetId path) where T : IAsset, new() {
        if (!TryResolveId(path.Value, out var pathActual)) {
            pathActual = path;
        }

        if (UriTable.TryGetValue(pathActual, out var reference)) {
            return reference.Create<T>();
        }

        throw new AssetIdNotFoundException(path);
    }

    public static T Create<T>(EbonyArchive earc, in EbonyArchiveFile file) where T : IAsset, new() {
        var data = earc.Read(file);

        object? localInstance = null;
        try {
            localInstance = Activator.CreateInstance(typeof(T), file.Id, data);
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
            return result;
        }

        throw new UnreachableException();
    }

    public MemoryOwner<byte> Read(in AssetId path) {
        if (!TryResolveId(path.Value, out var pathActual)) {
            pathActual = path;
        }

        if (UriTable.TryGetValue(pathActual, out var reference)) {
            return reference.Read();
        }

        throw new AssetIdNotFoundException(path);
    }

    public bool TryRead(in AssetId path, [MaybeNullWhen(false)] out MemoryOwner<byte> buffer) {
        if (!TryResolveId(path.Value, out var pathActual)) {
            pathActual = path;
        }

        if (UriTable.TryGetValue(pathActual, out var reference)) {
            buffer = reference.Read();
            return true;
        }

        buffer = null;
        return false;
    }

    public readonly record struct FileReference(AssetId ArchiveId, AssetId FileId, string DataPath) {
        public bool Exists => !FileId.IsNull;
        public EbonyArchive Archive => Instance.Archives[ArchiveId];
        public EbonyArchiveFile File => Archive.FileEntries[Archive.IdMap[FileId]];

        public (EbonyArchive Archive, EbonyArchiveFile File) Deconstruct() => Exists ? (Archive, File) : (Archive, default);

        public MemoryOwner<byte> Read() => Archive.Read(File);
        public T Create<T>() where T : IAsset, new() => AssetManager.Create<T>(Archive, File);

        public object GetPath() => File.GetPath(Archive.Buffer);
        public object GetDataPath() => File.GetDataPath(Archive.Buffer);
    }

    public static void DetectGame(string installDir) {
        var exe = Directory.GetFiles(installDir, "*.exe", SearchOption.TopDirectoryOnly).Select(x => Path.GetFileNameWithoutExtension(x).ToLowerInvariant()).ToHashSet();

        if (exe.Contains("ffxv_s")) {
            Game = EbonyGame.Black;
            return;
        }

        if (exe.Contains("forspoken")) {
            Game = EbonyGame.Witch;
            return;
        }

        throw new InvalidOperationException("Could not determine if the installation is FFXV or FORSPOKEN. Is the path correct?");
    }
}

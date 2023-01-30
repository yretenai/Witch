namespace Scarlet.Structures.Archive;

[Flags]
public enum EbonyArchiveFileFlags : uint {
    AutoLoad = 1,
    Compressed = 2,
    Reference = 4,
    Loose = 8,
    Patched = 0x10,
    Deleted = 0x20,
    Encrypted = 0x40,
    SkipObfuscation = 0x80,
    HasCompressType = 0x10000000, // this is an assumption, Forspoken has 1 + 4 always set when compressed.
}

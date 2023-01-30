namespace Scarlet.Structures.Archive;

[Flags]
public enum EbonyArchiveFlags : uint {
    Loose = 1,
    Localized = 2,
    Debug = 4,
    AdvanceChecksum = 8,
}

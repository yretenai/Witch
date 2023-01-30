namespace Scarlet.Structures.Archive;

public enum EbonyArchiveCompressionType : uint {
    Zlib = 1, // todo: validate this
    LZ4Stream = 2,
}

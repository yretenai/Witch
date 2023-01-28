namespace Scarlet.Structures.Archive;

public enum EARCCompressionType : uint {
    Zlib = 1, // todo: validate this
    LZ4Stream = 2,
}

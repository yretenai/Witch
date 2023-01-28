﻿namespace Scarlet.Structures.Archive;

[Flags]
public enum EARCFileFlags : uint {
    AutoLoad = 1,
    Compressed = 2,
    Reference = 4,
    Loose = 8,
    Patched = 0x10,
    Deleted = 0x20,
    Encrypted = 0x40,
    SkipObofuscation = 0x80,
    HasCompressType = 0x10000000, // this is an assumption, Forspoken has 1 + 4 always set when compressed.
    CompressTypeZlib = 0x20000000, // todo: validate this
    CompressTypeLz4 = 0x40000000,
}

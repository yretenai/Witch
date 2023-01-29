﻿using System.Runtime.InteropServices;

namespace Scarlet.Structures.Archive;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct EREPRow {
    public FileId Id { get; init; }
    public ulong Checksum { get; init; }
}

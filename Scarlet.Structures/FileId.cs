﻿using System.Collections;
using System.Resources;
using System.Text;
using DragonLib.Hash;
using DragonLib.Hash.Basis;

namespace Scarlet.Structures;

public readonly record struct FileId {
    public ulong Value { get; init; }

    public uint TypeId => (uint)(Value >> 44);
    public ulong Checksum => Value & 0xFFFFFFFFFFF;

    public FileId(ulong value) => Value = value;
    public FileId(uint typeId, ulong checksum) => Value = (ulong)typeId << 44 | (checksum & 0xFFFFFFFFFFF);
    public FileId(string path) : this(path, Path.GetExtension(path)[1..]) { }

    private const ulong HashSeed64 = 1469598103934665603;
    private const ulong HashPrime64 = 1099511628211;

    public static ulong Hash64(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return bytes.Aggregate(HashSeed64, (current, b) => (current ^ b) * HashPrime64);
    }
    public FileId(string path, string ext) {
        using var fnv = FowlerNollVo.CreateAlternate((FNV64Basis) 0x14650FB0739D0383);
        var pathHash = Hash64(path);

        fnv.Reset(0x14650FB0739D0383);
        var typeHash = fnv.ComputeHashValue(Encoding.UTF8.GetBytes(ext));

        Value = typeHash << 44 | (pathHash & 0xFFFFFFFFFFF);
    }

    public static implicit operator FileId(ulong value) => new() { Value = value };
    public static implicit operator ulong(FileId id) => id.Value;
    public static Hashtable IdTable { get; set; } = new();
    public override string ToString() => IdTable[this] as string ?? Value.ToString("X16");
    public bool Equals(FileId? other) => other?.Value == Value;
    public override int GetHashCode() => Value.GetHashCode();
}

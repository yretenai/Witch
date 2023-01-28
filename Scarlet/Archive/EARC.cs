using System.Buffers;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using DragonLib.Hash;
using DragonLib.Hash.Basis;
using IronCompress;
using Scarlet.Structures;
using Scarlet.Structures.Archive;

namespace Scarlet.Archive;

public class EARC : IDisposable {
    private static readonly Iron Iron = new(ArrayPool<byte>.Shared);

    public EARC(string path) {
        using var _perf = new PerformanceCounter("EARC`ctor");
        Stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        if (Stream.Length < Unsafe.SizeOf<EARCHeader>()) {
            Stream.Close();
            Stream.Dispose();
            throw new InvalidDataException("File is too small to be an EARC archive.");
        }

        EARCHeader header = new();
        var blitHeader = new BlitSpan<EARCHeader>(ref header);
        Stream.ReadExactly(blitHeader.GetByteSpan(0));

        if (header.Magic != EARCHeader.MagicValue) {
            Stream.Close();
            Stream.Dispose();
            throw new InvalidDataException("File is not an EARC archive.");
        }

        Stream.Position = 0;
        Buffer = MemoryOwner<byte>.Allocate((int) header.DataOffset);
        Stream.ReadExactly(Buffer.Span);
        BlitFileEntries = new BlitStruct<EARCFile>(Buffer, (int) header.FATOffset, header.FileCount);

        if (header.Version < 0) {
            using var _perfDecrypt = new PerformanceCounter("EARC`DecryptFAT");
            header.Version &= 0x7FFFFFFF;
            var key = header.Checksum ^ EARCHeader.ChecksumXOR1;
            if ((header.Flags & EARCFlags.AdvanceChecksum) != 0) {
                key ^= EARCHeader.ChecksumXOR2;
            }

            using var fnv = FowlerNollVo.Create((FNV64Basis) key);
            var blitFileEntries = BlitFileEntries.BlitSpan;
            for (var i = 0; i < blitFileEntries.Length; i++) {
                ref var file = ref blitFileEntries[i];
                var bytes = MemoryMarshal.Cast<byte, ulong>(blitFileEntries.GetByteSpan(i));
                if ((file.Flags & EARCFileFlags.SkipObofuscation) == 0) {
                    (file.CompressedSize, file.Size) = (file.Size, file.CompressedSize);
                    bytes[1] ^= fnv.HashNext(file.Checksum);
                    bytes[3] ^= fnv.HashNext(~file.Checksum);
                    (file.CompressedSize, file.Size) = (file.Size, file.CompressedSize);
                }
            }
        }

        Header = header;
    }

    public Stream Stream { get; }
    public MemoryOwner<byte> Buffer { get; }
    public EARCHeader Header { get; }
    private BlitStruct<EARCFile> BlitFileEntries { get; }
    public Span<EARCFile> FileEntries => BlitFileEntries.Span;

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~EARC() {
        Dispose(false);
    }

    protected virtual void ReleaseUnmanagedResources() { }

    protected virtual void Dispose(bool disposing) {
        ReleaseUnmanagedResources();
        if (disposing) {
            Stream.Close();
            Stream.Dispose();
            Buffer.Dispose();
            BlitFileEntries.Dispose();
        }
    }

    public unsafe MemoryOwner<byte> ReadFile(EARCFile file) {
        using var _perf = new PerformanceCounter("EARC`ReadFile");
        var buffer = MemoryOwner<byte>.Allocate(file.CompressedSize);
        Stream.Position = file.DataOffset;

        try {
            Stream.ReadExactly(buffer.Span);
        } catch {
            buffer.Dispose();
            throw;
        }

        if ((file.Flags & EARCFileFlags.Encrypted) != 0) {
            throw new NotSupportedException();

            using var _perfDecrypt = new PerformanceCounter("EARC`DecryptFile");
        }

        if ((file.Flags & EARCFileFlags.Compressed) == 0) {
            return buffer;
        }

        using var _perfDecompress = new PerformanceCounter("EARC`DecompressFile");
        try {
            var type = CompressType.Zlib;
            if ((file.Flags & EARCFileFlags.HasCompressType) != 0) {
                if ((file.Flags & EARCFileFlags.CompressTypeLz4) != 0) {
                    type = CompressType.Lz4;
                } else if ((file.Flags & EARCFileFlags.CompressTypeZlib) != 0) {
                    type = CompressType.Zlib;
                }
            }

            var decompressed = MemoryOwner<byte>.Allocate(file.Size);
            switch (type) {
                case CompressType.Zlib: {
                    if (!Debugger.IsAttached) {
                        Debugger.Launch();
                    }

                    Debugger.Break();

                    using var inputPin = buffer.Memory.Pin();
                    using var input = new UnmanagedMemoryStream((byte*) inputPin.Pointer, file.CompressedSize);

                    try {
                        using var zlib = new DeflateStream(input, CompressionMode.Decompress);
                        zlib.ReadExactly(decompressed.Span);
                    } catch {
                        decompressed.Dispose();
                        throw;
                    }

                    break;
                }
                case > CompressType.None: { // IronCompress codecs, usually LZ4
                    try {
                        var result = Iron.Decompress((Codec) type, buffer.Span, file.Size);
                        if (result == null) {
                            throw new InvalidDataException("Failed to decompress data.");
                        }

                        result.AsSpan().CopyTo(decompressed.Span);
                    } catch {
                        decompressed.Dispose();
                        throw;
                    }

                    break;
                }
            }

            return decompressed;
        } finally {
            buffer.Dispose();
        }
    }

    private enum CompressType {
        Zlib = -1,
        None = 0,
        Lz4 = Codec.LZ4,
    }
}

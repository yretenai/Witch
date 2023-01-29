using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using DragonLib.Hash;
using DragonLib.Hash.Basis;
using K4os.Compression.LZ4.Streams;
using Scarlet.Structures;
using Scarlet.Structures.Archive;

namespace Scarlet.Archive;

public class EARC : IDisposable {
    public unsafe EARC(string path) {
        using var _perf = new PerformanceCounter<PerformanceHost.EARC>();
        Stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        if (Stream.Length < Unsafe.SizeOf<EARCHeader>()) {
            Stream.Close();
            Stream.Dispose();
            throw new InvalidDataException("File is too small to be an EARC archive.");
        }

        var header = stackalloc EARCHeader[1];
        var blitHeader = new BlitSpan<EARCHeader>(ref header[0]);
        Stream.ReadExactly(blitHeader.GetByteSpan(0));

        if (header->Magic != EARCHeader.MagicValue) {
            Stream.Close();
            Stream.Dispose();
            throw new InvalidDataException("File is not an EARC archive.");
        }

        Stream.Position = 0;
        Buffer = MemoryOwner<byte>.Allocate((int) header->DataOffset);
        Stream.ReadExactly(Buffer.Span);
        BlitFileEntries = new BlitStruct<EARCFile>(Buffer, (int) header->FATOffset, header->FileCount);

        if (header->Version < 0) {
            using var _perfDeobfuscate = new PerformanceCounter<PerformanceHost.EARC.Deobfuscate>();
            header->Version &= 0x7FFFFFFF;
            var key = header->Checksum ^ EARCHeader.ChecksumXOR1;
            if ((header->Flags & EARCFlags.AdvanceChecksum) != 0) {
                key ^= EARCHeader.ChecksumXOR2;
            }

            using var fnv = FowlerNollVo.Create((FNV64Basis) key);
            var blitFileEntries = BlitFileEntries.BlitSpan;
            for (var i = 0; i < blitFileEntries.Length; i++) {
                ref var file = ref blitFileEntries[i];
                var bytes = MemoryMarshal.Cast<byte, ulong>(blitFileEntries.GetByteSpan(i));
                if ((file.Flags & EARCFileFlags.SkipObfuscation) == 0) {
                    (file.Size, file.CompressedSize) = (file.CompressedSize, file.Size);
                    bytes[1] ^= fnv.HashNext(file.Id);
                    bytes[3] ^= fnv.HashNext(~file.Id);
                    (file.Size, file.CompressedSize) = (file.CompressedSize, file.Size);
                }
            }
        }

        Header = header[0];
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
        using var _perf = new PerformanceCounter<PerformanceHost.EARC.Read>();
        var buffer = MemoryOwner<byte>.Allocate(file.CompressedSize);
        Stream.Position = file.DataOffset;

        try {
            Stream.ReadExactly(buffer.Span);
        } catch {
            buffer.Dispose();
            throw;
        }

        _perf.Stop();

        if ((file.Flags & EARCFileFlags.Encrypted) != 0) {
            throw new NotSupportedException("EARC encryption is not supported.");

            using var _perfDecrypt = new PerformanceCounter<PerformanceHost.EARC.Decrypt>();
        }

        if ((file.Flags & EARCFileFlags.Compressed) == 0) {
            return buffer;
        }

        try {
            using var _perfDecrypt = new PerformanceCounter<PerformanceHost.EARC.Decompress>();
            if ((file.Flags & EARCFileFlags.HasCompressType) == 0) {
                file.Flags &= EARCFileFlags.HasCompressType;
                file.Flags &= (EARCFileFlags) ((uint) EARCCompressionType.Zlib << 29);
            } else {
                Debug.Assert((EARCCompressionType) ((uint) file.Flags >> 7) != EARCCompressionType.Zlib, "Zlib compression is an assumption.");
            }

            var decompressed = MemoryOwner<byte>.Allocate(file.Size);
            using var inputPin = buffer.Memory.Pin();
            using var input = new UnmanagedMemoryStream((byte*) inputPin.Pointer, buffer.Length);

            switch ((EARCCompressionType) ((uint) file.Flags >> 29)) {
                case EARCCompressionType.Zlib: {
                    input.Position = 2; // skip zlib header

                    try {
                        using var zlib = new DeflateStream(input, CompressionMode.Decompress);
                        zlib.ReadExactly(decompressed.Span);
                    } catch {
                        decompressed.Dispose();
                        throw;
                    }

                    break;
                }
                case EARCCompressionType.LZ4Stream: {
                    try {
                        using var lz = LZ4Stream.Decode(input);
                        lz.ReadExactly(decompressed.Span);
                    } catch {
                        decompressed.Dispose();
                        throw;
                    }

                    break;
                }
                default: {
                    decompressed.Dispose();
                    throw new NotSupportedException("Unknown compression type.");
                }
            }

            return decompressed;
        } finally {
            buffer.Dispose();
        }
    }
}

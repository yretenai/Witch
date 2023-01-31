using System.Runtime.InteropServices;

namespace Scarlet.Structures.Archive;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct EbonyReplacement {
    public FileId DestinationId { get; init; }
    public FileId SourceId { get; init; }
}

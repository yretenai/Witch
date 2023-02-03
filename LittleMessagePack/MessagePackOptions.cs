using System;
using System.Collections.Generic;
using System.Text;

namespace LittleMessagePack;

public readonly record struct MessagePackOptions() {
    public bool LittleEndian { get; init; }
    public bool LittleEndianLength { get; init; }
    public bool WriteDefault { get; init; }
    public Encoding Encoding { get; init; } = Encoding.UTF8;
    public IReadOnlyList<IMessagePackConverter> Converters { get; init; } = Array.Empty<IMessagePackConverter>();
    public IReadOnlyList<IMessagePackExtension> Extensions { get; init; } = Array.Empty<IMessagePackExtension>();
}

using System;

namespace LittleMessagePack;

public interface IMessagePackExtension {
    public bool CanRead(sbyte typeId);
    public bool HandlesType(Type target);

    object? Read(Span<byte> data, MessagePackOptions options);
    Span<byte> Write(object value, MessagePackOptions options);
}

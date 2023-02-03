using System;

namespace LittleMessagePack;

public interface IMessagePackConverter {
    bool CanConvert(Type source, Type target);
    object? Convert(Type source, object value, MessagePackOptions options);
}

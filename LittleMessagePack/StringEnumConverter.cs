using System;
using System.Diagnostics.CodeAnalysis;

namespace LittleMessagePack;

public class StringEnumConverter : IMessagePackConverter {
    public StringEnumConverter([StringSyntax(StringSyntaxAttribute.EnumFormat)] string format = "G") {
        Format = format;
    }

    public string Format { get; }

    public bool CanConvert(Type source, Type target) => source.IsEnum && target == typeof(string) || source == typeof(string) && target.IsEnum;

    public object? Convert(Type source, object value, MessagePackOptions options) {
        return value switch {
                   string str => Enum.Parse(source, str),
                   Enum @enum => @enum.ToString(Format),
                   _          => throw new NotSupportedException(),
               };
    }
}

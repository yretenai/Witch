using System.Buffers.Binary;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Serilog;

namespace Scarlet;

public record MessagePackElement(MessagePackTypeId Type, object? Value);

public enum MessagePackTypeId {
    PositiveFixIntStart = 0x0,
    PositiveFixIntEnd = 0x7F,
    FixMapStart = 0x80,
    FixMapEnd = 0x8F,
    FixArrayStart = 0x90,
    FixArrayEnd = 0x9F,
    FixStrStart = 0xA0,
    FixStrEnd = 0xBF,
    Nil = 0xC0,
    NeverUsed = 0xC1,
    False = 0xC2,
    True = 0xC3,
    Bin8 = 0xC4,
    Bin16 = 0xC5,
    Bin32 = 0xC6,
    Ext8 = 0xC7,
    Ext16 = 0xC8,
    Ext32 = 0xC9,
    Float32 = 0xCA,
    Float64 = 0xCB,
    UInt8 = 0xCC,
    UInt16 = 0xCD,
    UInt32 = 0xCE,
    UInt64 = 0xCF,
    Int8 = 0xD0,
    Int16 = 0xD1,
    Int32 = 0xD2,
    Int64 = 0xD3,
    FixExt1 = 0xD4,
    FixExt2 = 0xD5,
    FixExt4 = 0xD6,
    FixExt8 = 0xD7,
    FixExt16 = 0xD8,
    Str8 = 0xD9,
    Str16 = 0xDA,
    Str32 = 0xDB,
    Array16 = 0xDC,
    Array32 = 0xDD,
    Map16 = 0xDE,
    Map32 = 0xDF,
    NegativeFixIntStart = 0xE0,
    NegativeFixIntEnd = 0xFF,
}

public class MessagePackBuffer {
    public MessagePackBuffer(Memory<byte> buffer) => Buffer = buffer;

    public Memory<byte> Buffer { get; }
    public int Offset { get; set; }

    public T? Read<T>() {
        var offset = Offset;
        var value = MessagePackValue.Read<T>(Buffer, ref offset);
        Offset = offset;
        return value;
    }

    public T?[] ReadArray<T>(uint? count = null) {
        count ??= Read<uint>();

        if (count.Value == 0) {
            return Array.Empty<T>();
        }

        var array = new T?[count.Value];
        for (var i = 0; i < count.Value; ++i) {
            array[i] = Read<T>();
        }

        return array;
    }

    public Dictionary<TKey, TValue?> ReadDictionary<TKey, TValue>(uint? count = null) where TKey : notnull {
        count ??= Read<uint>();

        if (count == 0) {
            return new Dictionary<TKey, TValue?>();
        }

        var dictionary = new Dictionary<TKey, TValue?>((int) count.Value);
        for (var i = 0; i < count.Value; ++i) {
            var key = Read<TKey>()!;
            var value = Read<TValue>();
            dictionary[key] = value;
        }

        return dictionary;
    }

    public MessagePackElement Read() => MessagePackValue.Read(typeof(MessagePackElement), Buffer, ref Unsafe.AsRef(Offset));
}

public static class MessagePackValue {
    private static void TypePunt(Type target, ref object? element) {
        if (target == typeof(object)) {
            return;
        }

        var value = ((MessagePackElement) element!).Value;
        if (target == typeof(MessagePackElement)) {
            return;
        }

        if (value != default) {
            if (target.IsEnum && value.GetType().IsPrimitive) {
                value = Enum.ToObject(target, value);
            }

            var valueType = value.GetType();
            if (valueType != target) {
                if (target == typeof(byte[]) && valueType == typeof(Memory<byte>)) {
                    value = ((Memory<byte>) value).ToArray();
                } else if (target.IsConstructedGenericType && target.GetGenericTypeDefinition() == typeof(Memory<>) && target.GetElementType()!.MakeArrayType() == valueType) {
                    value = Activator.CreateInstance(target, valueType);
                } else if (target.IsPrimitive && value is string str) {
                    // this can be done via reflection, but reflection is slow so...
                    if (target == typeof(bool)) {
                        value = bool.Parse(str);
                    } else if (target == typeof(byte)) {
                        value = byte.Parse(str);
                    } else if (target == typeof(sbyte)) {
                        value = sbyte.Parse(str);
                    } else if (target == typeof(ushort)) {
                        value = ushort.Parse(str);
                    } else if (target == typeof(short)) {
                        value = short.Parse(str);
                    } else if (target == typeof(uint)) {
                        value = uint.Parse(str);
                    } else if (target == typeof(int)) {
                        value = int.Parse(str);
                    } else if (target == typeof(ulong)) {
                        value = ulong.Parse(str);
                    } else if (target == typeof(long)) {
                        value = long.Parse(str);
                    } else if (target == typeof(Half)) {
                        value = Half.Parse(str);
                    } else if (target == typeof(float)) {
                        value = float.Parse(str);
                    } else if (target == typeof(double)) {
                        value = double.Parse(str);
                    } else if (target == typeof(decimal)) {
                        value = decimal.Parse(str);
                    } else if (target == typeof(DateTime)) {
                        value = DateTime.Parse(str);
                    } else if (target == typeof(DateTimeOffset)) {
                        value = DateTimeOffset.Parse(str);
                    } else if (target == typeof(TimeSpan)) {
                        value = TimeSpan.Parse(str);
                    } else if (target == typeof(Guid)) {
                        value = Guid.Parse(str);
                    } else {
                        var method = target.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, new[] { typeof(string) });
                        if (method != null) {
                            value = method.Invoke(null, new object?[] { str });
                        } else {
                            throw new NotSupportedException($"I don't know how to do turn a string into a {target.FullName}");
                        }
                    }
                } else if (target == typeof(string)) {
                    value = target.ToString();
                } else {
                    var method = target.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, new[] { valueType });
                    if (method != null) {
                        value = method.Invoke(null, new[] { value });
                    } else {
                        var constructor = target.GetConstructor(BindingFlags.Public | BindingFlags.Instance, new[] { valueType });
                        if (constructor != null) {
                            value = Activator.CreateInstance(target, value);
                        } else {
                            try {
                                value = Convert.ChangeType(value, target);
                            } catch {
                                throw new NotSupportedException($"I don't know how to do turn a {valueType.FullName} into a {target.FullName}");
                            }
                        }
                    }
                }
            }
        }

        element = value;
    }

    public static T? Read<T>(Memory<byte> data, ref int offset) {
        var value = (object?) Read(typeof(T), data, ref offset);
        TypePunt(typeof(T), ref value);
        return value is T tValue ? tValue : default;
    }

    public static MessagePackElement Read(Type type, Memory<byte> data, ref int offset) {
        var opcode = data.Span[offset++];
        var typeID = (MessagePackTypeId) opcode;

        object? value = null;

        switch (typeID) {
            case >= MessagePackTypeId.PositiveFixIntStart and <= MessagePackTypeId.PositiveFixIntEnd:
                value = opcode & 0x7F;
                break;
            case >= MessagePackTypeId.NegativeFixIntStart and <= MessagePackTypeId.NegativeFixIntEnd:
                value = (sbyte) -(opcode & 0x1F);
                break;
            case >= MessagePackTypeId.FixMapStart and <= MessagePackTypeId.FixMapEnd:
                value = ReadDictionary(type, data, opcode & 0xF, ref offset);
                break;
            case >= MessagePackTypeId.FixArrayStart and <= MessagePackTypeId.FixArrayEnd:
                value = ReadArray(type, data, opcode & 0xF, ref offset);
                break;
            case >= MessagePackTypeId.FixStrStart and <= MessagePackTypeId.FixStrEnd:
                value = ReadString(data, opcode & 0x1F, ref offset);
                break;
            case MessagePackTypeId.Nil:
                value = null;
                break;
            case MessagePackTypeId.False:
                value = false;
                break;
            case MessagePackTypeId.True:
                value = true;
                break;
            case MessagePackTypeId.Bin8: {
                var count = data.Span[offset++];
                value = data.Slice(offset, count);
                break;
            }
            case MessagePackTypeId.Bin16: {
                var count = Primitive.ReadUInt16(data.Span, ref offset);
                value = data.Slice(offset, count);
                break;
            }
            case MessagePackTypeId.Bin32: {
                var count = Primitive.ReadInt32(data.Span, ref offset);
                value = data.Slice(offset, count);
                break;
            }
            case MessagePackTypeId.Ext8:
                value = ReadExtension(data, data.Span[offset++], (sbyte) data.Span[offset++], ref offset);
                break;
            case MessagePackTypeId.Ext16:
                value = ReadExtension(data, Primitive.ReadUInt16(data.Span, ref offset), (sbyte) data.Span[offset++], ref offset);
                break;
            case MessagePackTypeId.Ext32:
                value = ReadExtension(data, Primitive.ReadInt32(data.Span, ref offset), (sbyte) data.Span[offset++], ref offset);
                break;
            case MessagePackTypeId.Float32:
                value = Primitive.ReadFloat32(data.Span, ref offset);
                break;
            case MessagePackTypeId.Float64:
                value = Primitive.ReadFloat64(data.Span, ref offset);
                break;
            case MessagePackTypeId.UInt8:
                value = data.Span[offset++];
                break;
            case MessagePackTypeId.UInt16:
                value = Primitive.ReadUInt16(data.Span, ref offset);
                break;
            case MessagePackTypeId.UInt32:
                value = Primitive.ReadUInt32(data.Span, ref offset);
                break;
            case MessagePackTypeId.UInt64:
                value = Primitive.ReadUInt64(data.Span, ref offset);
                break;
            case MessagePackTypeId.Int8:
                value = (sbyte) data.Span[offset++];
                break;
            case MessagePackTypeId.Int16:
                value = Primitive.ReadInt16(data.Span, ref offset);
                break;
            case MessagePackTypeId.Int32:
                value = Primitive.ReadInt32(data.Span, ref offset);
                break;
            case MessagePackTypeId.Int64:
                value = Primitive.ReadInt64(data.Span, ref offset);
                break;
            case MessagePackTypeId.FixExt1: {
                var extType = (sbyte) data.Span[offset++];
                value = ReadExtension(data, 1, extType, ref offset);
                break;
            }
            case MessagePackTypeId.FixExt2: {
                var extType = (sbyte) data.Span[offset++];
                value = ReadExtension(data, 2, extType, ref offset);
                break;
            }
            case MessagePackTypeId.FixExt4: {
                var extType = (sbyte) data.Span[offset++];
                value = ReadExtension(data, 4, extType, ref offset);
                break;
            }
            case MessagePackTypeId.FixExt8: {
                var extType = (sbyte) data.Span[offset++];
                value = ReadExtension(data, 8, extType, ref offset);
                break;
            }
            case MessagePackTypeId.FixExt16: {
                var extType = (sbyte) data.Span[offset++];
                value = ReadExtension(data, 16, extType, ref offset);
                break;
            }
            case MessagePackTypeId.Str8: {
                value = ReadString(data, data.Span[offset++], ref offset);
                break;
            }
            case MessagePackTypeId.Str16: {
                value = ReadString(data, Primitive.ReadUInt16(data.Span, ref offset), ref offset);
                break;
            }
            case MessagePackTypeId.Str32: {
                value = ReadString(data, Primitive.ReadInt32(data.Span, ref offset), ref offset);
                break;
            }
            case MessagePackTypeId.Array16: {
                value = ReadArray(type, data, Primitive.ReadUInt16(data.Span, ref offset), ref offset);
                break;
            }
            case MessagePackTypeId.Array32: {
                value = ReadArray(type, data, Primitive.ReadInt32(data.Span, ref offset), ref offset);
                break;
            }
            case MessagePackTypeId.Map16: {
                value = ReadDictionary(type, data, Primitive.ReadUInt16(data.Span, ref offset), ref offset);
                break;
            }
            case MessagePackTypeId.Map32: {
                value = ReadDictionary(type, data, Primitive.ReadInt32(data.Span, ref offset), ref offset);
                break;
            }
        }

        return new MessagePackElement(typeID, value);
    }

    private static object? ReadExtension(Memory<byte> data, int length, sbyte typeId, ref int offset) {
        var _ = data.Slice(offset, length); // block
        offset += length;

        Log.Warning("Unimplemented EXT Type {Id}", typeId);

        return null;
    }

    private static object ReadString(Memory<byte> data, int length, ref int offset) {
        var block = data.Slice(offset, length);
        offset += length;

        if (block.Span[^1] == 0) {
            block = block[..^1];
        }

        return Encoding.UTF8.GetString(block.Span);
    }

    private static object ReadArray(Type type, Memory<byte> data, int length, ref int offset) {
        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Memory<>)) {
            // TypePunt will convert this array back into a Memory<>
            type = type.GetGenericArguments()[0].MakeArrayType();
        }

        if (!type.IsAssignableTo(typeof(IList))) {
            throw new InvalidOperationException("Trying to load an array into a non-array type!");
        }

        var instance = Activator.CreateInstance(type, length)!;
        var array = (IList) instance;
        var arrayType = type.HasElementType ? type.GetElementType()! : type.IsConstructedGenericType ? type.GetGenericArguments()[0] : typeof(object);

        for (var i = 0; i < length; ++i) {
            var value = (object?) Read(arrayType, data, ref offset);

            TypePunt(arrayType, ref value);

            array[i] = value;
        }

        return instance;
    }

    private static object ReadDictionary(Type type, Memory<byte> data, int length, ref int offset) {
        if (!type.IsAssignableTo(typeof(IDictionary))) {
            throw new InvalidOperationException("Trying to load an array into a non-array type!");
        }

        var instance = Activator.CreateInstance(type, length)!;
        var dictionary = (IDictionary) instance;
        var keyType = type.IsConstructedGenericType ? type.GetGenericArguments()[0] : typeof(object);
        var valueType = type.IsConstructedGenericType ? type.GetGenericArguments()[1] : typeof(object);

        for (var i = 0; i < length; ++i) {
            var key = (object?) Read(keyType, data, ref offset);
            var value = (object?) Read(valueType, data, ref offset);

            TypePunt(keyType, ref key);
            TypePunt(valueType, ref value);

            dictionary[key!] = value;
        }

        return instance;
    }

    private static class Primitive {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ushort ReadUInt16(Span<byte> data, ref int offset) {
            var value = BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
            offset += sizeof(ushort);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static short ReadInt16(Span<byte> data, ref int offset) {
            var value = BinaryPrimitives.ReadInt16LittleEndian(data[offset..]);
            offset += sizeof(short);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint ReadUInt32(Span<byte> data, ref int offset) {
            var value = BinaryPrimitives.ReadUInt32LittleEndian(data[offset..]);
            offset += sizeof(uint);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ReadInt32(Span<byte> data, ref int offset) {
            var value = BinaryPrimitives.ReadInt32LittleEndian(data[offset..]);
            offset += sizeof(int);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ulong ReadUInt64(Span<byte> data, ref int offset) {
            var value = BinaryPrimitives.ReadUInt64LittleEndian(data[offset..]);
            offset += sizeof(ulong);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long ReadInt64(Span<byte> data, ref int offset) {
            var value = BinaryPrimitives.ReadInt64LittleEndian(data[offset..]);
            offset += sizeof(long);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float ReadFloat32(Span<byte> data, ref int offset) {
            var value = BinaryPrimitives.ReadSingleLittleEndian(data[offset..]);
            offset += sizeof(float);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double ReadFloat64(Span<byte> data, ref int offset) {
            var value = BinaryPrimitives.ReadDoubleLittleEndian(data[offset..]);
            offset += sizeof(double);
            return value;
        }
    }
}

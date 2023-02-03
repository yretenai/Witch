using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace LittleMessagePack;

public static class MessagePackDeserializer {
    private record struct CacheEntry(Type Type, FieldInfo Field);

    private static Dictionary<Type, CacheEntry[]> Cache { get; } = new();

    public static T? Read<T>(Memory<byte> data, in MessagePackOptions options) where T : new() {
        var value = Read(typeof(T), data, options);
        if (value is T tValue) {
            return tValue;
        }

        return default;
    }

    public static object? Read(Type type, Memory<byte> data, in MessagePackOptions options) => Read(type, data, options, ref Unsafe.AsRef(0));

    private static object? Read(Type type, Memory<byte> data, in MessagePackOptions options, ref int offset) {
        var typeList = GetCachedEntry(type).AsSpan();

        if (type.IsEnum || type.IsPrimitive || type == typeof(string) || type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Memory<>) || type.IsAssignableTo(typeof(IDictionary)) || type.IsAssignableTo(typeof(IList)) || options.Extensions.Any(x => x.HandlesType(type))) {
            var value = ReadValue(type, data, options, ref offset);
            TypePunt(options, type, ref value);
            return value;
        } else {
            var instance = Activator.CreateInstance(type);
            var typeOffset = 0;
            while (offset < data.Length) {
                if (typeOffset >= typeList.Length) {
                    break;
                }

                var (targetType, field) = typeList[typeOffset++];

                object? value;
                if (targetType.IsClass || targetType is { IsValueType: true, IsEnum: false }) {
                    value = Read(targetType, data, options, ref offset);
                } else {
                    value = ReadValue(targetType, data, options, ref offset);
                }

                if (value == default && !options.WriteDefault) {
                    continue;
                }

                TypePunt(options, targetType, ref value);

                field.SetValue(instance, value);
            }

            return instance;
        }
    }

    private static void TypePunt(MessagePackOptions options, Type target, ref object? value) {
        if (value != default) {
            var targetType = value.GetType();
            foreach (var converter in options.Converters) {
                if (converter.CanConvert(targetType, target)) {
                    converter.Convert(targetType, value, options);
                    break;
                }
            }

            if (target.IsEnum && value.GetType().IsPrimitive) {
                value = Enum.ToObject(target, value);
            }

            var valueType = value.GetType();
            if (valueType != target) {
                if (target == typeof(byte[]) && valueType == typeof(Memory<byte>)) {
                    value = ((Memory<byte>) value).ToArray();
                } else if (target.IsConstructedGenericType && target.GetElementType()!.MakeArrayType() == valueType && target.GetGenericTypeDefinition() == typeof(Memory<>)) {
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
                            throw new InvalidOperationException("Trying to convert a string to a primitive type, implement Parse(string)!");
                        }
                    }
                } else if (target == typeof(string)) {
                    value = target.ToString();
                } else {
                    value = Convert.ChangeType(value, target);
                }
            }
        }
    }


    private static object? ReadValue(Type type, Memory<byte> data, in MessagePackOptions options, ref int offset) {
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
                value = ReadDictionary(type, data, opcode & 0xF, options, ref offset);
                break;
            case MessagePackTypeId.FixArrayStart:
                value = ReadArray(type, data, opcode & 0xF, options, ref offset);
                break;
            case MessagePackTypeId.FixStrStart:
                value = ReadString(data, opcode & 0x1F, options, ref offset);
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
                value = data[..count];
                break;
            }
            case MessagePackTypeId.Bin16: {
                var count = PrimitiveHelper.ReadUInt16(options.LittleEndianLength, data.Span, ref offset);
                value = data[..count];
                break;
            }
            case MessagePackTypeId.Bin32: {
                var count = PrimitiveHelper.ReadInt32(options.LittleEndianLength, data.Span, ref offset);
                value = data[..count];
                break;
            }
            case MessagePackTypeId.Ext8:
                value = ReadExtension(data, data.Span[offset++], (sbyte) data.Span[offset++], options, ref offset);
                break;
            case MessagePackTypeId.Ext16:
                value = ReadExtension(data, PrimitiveHelper.ReadUInt16(options.LittleEndianLength, data.Span, ref offset), (sbyte) data.Span[offset++], options, ref offset);
                break;
            case MessagePackTypeId.Ext32:
                value = ReadExtension(data, PrimitiveHelper.ReadInt32(options.LittleEndianLength, data.Span, ref offset), (sbyte) data.Span[offset++], options, ref offset);
                break;
            case MessagePackTypeId.Float32:
                value = PrimitiveHelper.ReadFloat32(options.LittleEndian, data.Span, ref offset);
                break;
            case MessagePackTypeId.Float64:
                value = PrimitiveHelper.ReadFloat64(options.LittleEndian, data.Span, ref offset);
                break;
            case MessagePackTypeId.UInt8:
                value = data.Span[offset++];
                break;
            case MessagePackTypeId.UInt16:
                value = PrimitiveHelper.ReadUInt16(options.LittleEndian, data.Span, ref offset);
                break;
            case MessagePackTypeId.UInt32:
                value = PrimitiveHelper.ReadUInt32(options.LittleEndian, data.Span, ref offset);
                break;
            case MessagePackTypeId.UInt64:
                value = PrimitiveHelper.ReadUInt64(options.LittleEndian, data.Span, ref offset);
                break;
            case MessagePackTypeId.Int8:
                value = (sbyte) data.Span[offset++];
                break;
            case MessagePackTypeId.Int16:
                value = PrimitiveHelper.ReadInt16(options.LittleEndian, data.Span, ref offset);
                break;
            case MessagePackTypeId.Int32:
                value = PrimitiveHelper.ReadInt32(options.LittleEndian, data.Span, ref offset);
                break;
            case MessagePackTypeId.Int64:
                value = PrimitiveHelper.ReadInt64(options.LittleEndian, data.Span, ref offset);
                break;
            case MessagePackTypeId.FixExt1: {
                var extType = (sbyte) data.Span[offset++];
                value = ReadExtension(data, 1, extType, options, ref offset);
                break;
            }
            case MessagePackTypeId.FixExt2: {
                var extType = (sbyte) data.Span[offset++];
                value = ReadExtension(data, 2, extType, options, ref offset);
                break;
            }
            case MessagePackTypeId.FixExt4: {
                var extType = (sbyte) data.Span[offset++];
                value = ReadExtension(data, 4, extType, options, ref offset);
                break;
            }
            case MessagePackTypeId.FixExt8: {
                var extType = (sbyte) data.Span[offset++];
                value = ReadExtension(data, 8, extType, options, ref offset);
                break;
            }
            case MessagePackTypeId.FixExt16: {
                var extType = (sbyte) data.Span[offset++];
                value = ReadExtension(data, 16, extType, options, ref offset);
                break;
            }
            case MessagePackTypeId.Str8: {
                value = ReadString(data, data.Span[offset++], options, ref offset);
                break;
            }
            case MessagePackTypeId.Str16: {
                value = ReadString(data, PrimitiveHelper.ReadUInt16(options.LittleEndianLength, data.Span, ref offset), options, ref offset);
                break;
            }
            case MessagePackTypeId.Str32: {
                value = ReadString(data, PrimitiveHelper.ReadInt32(options.LittleEndianLength, data.Span, ref offset), options, ref offset);
                break;
            }
            case MessagePackTypeId.Array16: {
                value = ReadArray(type, data, PrimitiveHelper.ReadUInt16(options.LittleEndianLength, data.Span, ref offset), options, ref offset);
                break;
            }
            case MessagePackTypeId.Array32: {
                value = ReadArray(type, data, PrimitiveHelper.ReadInt32(options.LittleEndianLength, data.Span, ref offset), options, ref offset);
                break;
            }
            case MessagePackTypeId.Map16: {
                value = ReadDictionary(type, data, PrimitiveHelper.ReadUInt16(options.LittleEndianLength, data.Span, ref offset), options, ref offset);
                break;
            }
            case MessagePackTypeId.Map32: {
                value = ReadDictionary(type, data, PrimitiveHelper.ReadInt32(options.LittleEndianLength, data.Span, ref offset), options, ref offset);
                break;
            }
        }

        return value;
    }

    private static object? ReadExtension(Memory<byte> data, int length, sbyte typeId, in MessagePackOptions options, ref int offset) {
        var block = data[..length];
        offset += length;

        var processor = options.Extensions.FirstOrDefault(x => x.CanRead(typeId));

        if (processor == null) {
            throw new NotSupportedException($"Extension ID {typeId} is not implemented!");
        }

        return processor.Read(block.Span, options);
    }

    private static object ReadString(Memory<byte> data, int length, in MessagePackOptions options, ref int offset) {
        var block = data[..length];
        offset += length;
        return options.Encoding.GetString(block.Span);
    }

    private static object ReadArray(Type type, Memory<byte> data, int length, in MessagePackOptions options, ref int offset) {
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
            var value = Read(arrayType, data, options, ref offset);

            TypePunt(options, arrayType, ref value);

            array[i] = value;
        }

        return instance;
    }

    private static object ReadDictionary(Type type, Memory<byte> data, int length, in MessagePackOptions options, ref int offset) {
        if (!type.IsAssignableTo(typeof(IDictionary))) {
            throw new InvalidOperationException("Trying to load an array into a non-array type!");
        }

        var instance = Activator.CreateInstance(type, length)!;
        var dictionary = (IDictionary) instance;
        var keyType = type.IsConstructedGenericType ? type.GetGenericArguments()[0] : typeof(object);
        var valueType = type.IsConstructedGenericType ? type.GetGenericArguments()[1] : typeof(object);

        for (var i = 0; i < length; ++i) {
            var key = Read(keyType, data, options, ref offset);
            var value = Read(valueType, data, options, ref offset);

            TypePunt(options, keyType, ref key);
            TypePunt(options, valueType, ref value);

            dictionary[key!] = value;
        }

        return instance;
    }

    private static CacheEntry[] GetCachedEntry(Type type) {
        if (!Cache.TryGetValue(type, out var fields)) {
            fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetCustomAttribute<IgnoreDataMemberAttribute>() == null).Select(x => new CacheEntry(x.FieldType, x)).ToArray();
            Cache[type] = fields;
        }

        return fields;
    }
}

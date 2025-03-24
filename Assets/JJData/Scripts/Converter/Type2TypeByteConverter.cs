using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public static class Type2TypeByteConverter
{
    public static Dictionary<string, int> TypeByteLength = new();

    static Type2TypeByteConverter()
    {
        InitTypeByteLengths();
    }

    private static void InitTypeByteLengths()
    {
        Dictionary<string, int> defaultTypeByteLength = new()
        {
            {"string", -1},
            {"int", 4},
            {"float", 4},
            {"double", 8},
            {"long", 8},
            {"short", 2},
            {"bool", 1},
            {"char", 1},
            {"Vector2", Marshal.SizeOf<Vector2>()},
            {"Vector3", Marshal.SizeOf<Vector3>()},
            {"Quaternion", Marshal.SizeOf<Quaternion>()},
            {"Color", Marshal.SizeOf<Color>()},
        };

        foreach (var typeByte in defaultTypeByteLength)
        {
            if (!TypeByteLength.ContainsKey(typeByte.Key))
                TypeByteLength[typeByte.Key] = typeByte.Value;
        }
    }

    public static byte[] Convert<T>(T value)
    {
        if (value == null)
            return Array.Empty<byte>();

        Type type = typeof(T);

        if (type == typeof(string))
            return Convertstring2Byte(value as string);

        if (type.IsArray)
            return ConvertArray2Byte((Array)(object)value);

        if (type.IsGenericType)
        {
            Type genericTypeDef = type.GetGenericTypeDefinition();

            if (genericTypeDef == typeof(List<>))
                return ConvertList2Byte((IList)(object)value, type.GetGenericArguments()[0]);

            if (genericTypeDef == typeof(Dictionary<,>))
                return ConvertDictionary2Byte((IDictionary)(object)value, type.GetGenericArguments()[0], type.GetGenericArguments()[1]);

            if (genericTypeDef == typeof(HashSet<>))
                return ConvertHashSet2Byte((IEnumerable)(object)value, type.GetGenericArguments()[0]);
        }

        if (type.IsPrimitive || type.IsEnum)
            return ConvertPrimitive2Byte(value);

        if (type.IsValueType)
            return ConvertStruct2Byte(value);

        if (type.IsClass)
            return ConvertClass2Byte(value);

        throw new NotSupportedException($"[{type.FullName}] 타입은 직렬화 지원되지 않습니다.");
    }

    public static byte[] Convertstring2Byte(string value)
    {
        if (value == null) value = "";
        Span<byte> stringByte = Encoding.UTF8.GetBytes(value);
        int length = stringByte.Length;
        Span<byte> lengthByte = stackalloc byte[4];
        MemoryMarshal.Write(lengthByte, ref length);

        Span<byte> result = stackalloc byte[4 + length];
        lengthByte.CopyTo(result);
        stringByte.CopyTo(result.Slice(4));

        return result.ToArray();
    }

    public static byte[] ConvertPrimitive2Byte(object value)
    {
        Type type = value.GetType();
        int size = Marshal.SizeOf(type);
        byte[] buffer = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(value, ptr, false);
            Marshal.Copy(ptr, buffer, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return buffer;
    }

    public static byte[] ConvertStruct2Byte(object value)
    {
        Type type = value.GetType();
        int size = Marshal.SizeOf(type);
        byte[] buffer = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(value, ptr, false);
            Marshal.Copy(ptr, buffer, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return buffer;
    }

    public static byte[] ConvertArray2Byte(Array array)
    {
        List<byte> result = new();
        int length = array.Length;
        result.AddRange(ConvertPrimitive2Byte(length));

        Type elementType = array.GetType().GetElementType();
        foreach (var item in array)
        {
            byte[] itemBytes = (byte[])(typeof(Type2TypeByteConverter)
                .GetMethod(nameof(Convert))
                .MakeGenericMethod(elementType)
                .Invoke(null, new object[] { item }));
            result.AddRange(itemBytes);
        }
        return result.ToArray();
    }

    public static byte[] ConvertList2Byte(IList list, Type elementType)
    {
        List<byte> result = new();
        int count = list.Count;
        result.AddRange(ConvertPrimitive2Byte(count));

        foreach (var item in list)
        {
            byte[] itemBytes = (byte[])(typeof(Type2TypeByteConverter)
                .GetMethod(nameof(Convert))
                .MakeGenericMethod(elementType)
                .Invoke(null, new object[] { item }));
            result.AddRange(itemBytes);
        }
        return result.ToArray();
    }

    public static byte[] ConvertDictionary2Byte(IDictionary dict, Type keyType, Type valueType)
    {
        List<byte> result = new();
        int count = dict.Count;
        result.AddRange(ConvertPrimitive2Byte(count));

        foreach (DictionaryEntry entry in dict)
        {
            byte[] keyBytes = (byte[])(typeof(Type2TypeByteConverter)
                .GetMethod(nameof(Convert))
                .MakeGenericMethod(keyType)
                .Invoke(null, new object[] { entry.Key }));

            byte[] valueBytes = (byte[])(typeof(Type2TypeByteConverter)
                .GetMethod(nameof(Convert))
                .MakeGenericMethod(valueType)
                .Invoke(null, new object[] { entry.Value }));

            result.AddRange(keyBytes);
            result.AddRange(valueBytes);
        }
        return result.ToArray();
    }

    public static byte[] ConvertHashSet2Byte(IEnumerable set, Type elementType)
    {
        List<byte> result = new();
        int count = set.Cast<object>().Count();
        result.AddRange(ConvertPrimitive2Byte(count));

        foreach (var item in set)
        {
            byte[] itemBytes = typeof(Type2TypeByteConverter)
                .GetMethod(nameof(Convert))
                .MakeGenericMethod(elementType)
                .Invoke(null, new object[] { item }) as byte[];
            result.AddRange(itemBytes);
        }
        return result.ToArray();
    }

    public static byte[] ConvertClass2Byte<T>(T obj)
    {
        List<byte> result = new();
        Type type = typeof(T);

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var fields = type.GetFields(flags)
            .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null);

        foreach (var field in fields)
        {
            object fieldValue = field.GetValue(obj);
            if (fieldValue == null)
                continue;

            Type fieldType = field.FieldType;
            byte[] fieldBytes = typeof(Type2TypeByteConverter)
                .GetMethod(nameof(Convert))
                .MakeGenericMethod(fieldType)
                .Invoke(null, new object[] { fieldValue }) as byte[];
            result.AddRange(fieldBytes);
        }
        return result.ToArray();
    }
}

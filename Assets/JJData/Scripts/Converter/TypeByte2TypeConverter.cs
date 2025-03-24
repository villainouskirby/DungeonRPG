using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public partial class TypeByte2TypeConverter
{
    public static string ConvertTypeByte2string(ReadOnlySpan<byte> bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }
    public static int ConvertTypeByte2int(ReadOnlySpan<byte> bytes)
    {
        return MemoryMarshal.Read<int>(bytes);
    }

    public static float ConvertTypeByte2float(ReadOnlySpan<byte> bytes)
    {
        return MemoryMarshal.Read<float>(bytes);
    }

    public static double ConvertTypeByte2double(ReadOnlySpan<byte> bytes)
    {
        return MemoryMarshal.Read<double>(bytes);
    }

    public static long ConvertTypeByte2long(ReadOnlySpan<byte> bytes)
    {
        return MemoryMarshal.Read<long>(bytes);
    }

    public static short ConvertTypeByte2short(ReadOnlySpan<byte> bytes)
    {
        return MemoryMarshal.Read<short>(bytes);
    }

    public static bool ConvertTypeByte2bool(ReadOnlySpan<byte> bytes)
    {
        return bytes[0] == 1;
    }

    public static char ConvertTypeByte2char(ReadOnlySpan<byte> bytes)
    {
        return (char)bytes[0];
    }

    public static Vector2 ConvertTypeByte2Vector2(ReadOnlySpan<byte> bytes)
    {
        return MemoryMarshal.Read<Vector2>(bytes);
    }

    public static Vector3 ConvertTypeByte2Vector3(ReadOnlySpan<byte> bytes)
    {
        return MemoryMarshal.Read<Vector3>(bytes);
    }

    public static Quaternion ConvertTypeByte2Quaternion(ReadOnlySpan<byte> bytes)
    {
        return MemoryMarshal.Read<Quaternion>(bytes);
    }

    public static Color ConvertTypeByte2Color(ReadOnlySpan<byte> bytes)
    {
        return MemoryMarshal.Read<Color>(bytes);
    }


    public static T Convert<T>(byte[] data)
    {
        Type type = typeof(T);
        int offset = 0;

        if (type == typeof(string))
            return (T)(object)ConvertTypeByteToLenString(data, ref offset);

        if (type.IsArray)
            return (T)(object)ConvertTypeByteToArray(data, ref offset, type.GetElementType());

        if (type.IsGenericType)
        {
            var genericTypeDef = type.GetGenericTypeDefinition();

            if (genericTypeDef == typeof(List<>))
                return (T)ConvertTypeByteToList(data, ref offset, type.GetGenericArguments()[0]);

            if (genericTypeDef == typeof(Dictionary<,>))
                return (T)ConvertTypeByteToDictionary(data, ref offset, type.GetGenericArguments()[0], type.GetGenericArguments()[1]);

            if (genericTypeDef == typeof(HashSet<>))
                return (T)ConvertTypeByteToHashSet(data, ref offset, type.GetGenericArguments()[0]);
        }

        if (type.IsPrimitive || type.IsEnum)
            return (T)ConvertTypeByteToPrimitive(data, ref offset, type);

        if (type.IsValueType)
            return (T)ConvertTypeByteToStruct(data, ref offset, type);

        if (type.IsClass)
            return (T)ConvertTypeByteToClass(data, ref offset, type);

        throw new NotSupportedException($"[{type.FullName}] 타입은 역직렬화 지원되지 않습니다.");
    }

    public static T Convert<T>(byte[] data, ref int offset)
    {
        Type type = typeof(T);

        if (type == typeof(string))
            return (T)(object)ConvertTypeByteToLenString(data, ref offset);

        if (type.IsArray)
            return (T)(object)ConvertTypeByteToArray(data, ref offset, type.GetElementType());

        if (type.IsGenericType)
        {
            var genericTypeDef = type.GetGenericTypeDefinition();

            if (genericTypeDef == typeof(List<>))
                return (T)ConvertTypeByteToList(data, ref offset, type.GetGenericArguments()[0]);

            if (genericTypeDef == typeof(Dictionary<,>))
                return (T)ConvertTypeByteToDictionary(data, ref offset, type.GetGenericArguments()[0], type.GetGenericArguments()[1]);

            if (genericTypeDef == typeof(HashSet<>))
                return (T)ConvertTypeByteToHashSet(data, ref offset, type.GetGenericArguments()[0]);
        }

        if (type.IsPrimitive || type.IsEnum)
            return (T)ConvertTypeByteToPrimitive(data, ref offset, type);

        if (type.IsValueType)
            return (T)ConvertTypeByteToStruct(data, ref offset, type);

        if (type.IsClass)
            return (T)ConvertTypeByteToClass(data, ref offset, type);

        throw new NotSupportedException($"[{type.FullName}] 타입은 역직렬화 지원되지 않습니다.");
    }

    public static string ConvertTypeByteToLenString(byte[] data, ref int offset)
    {
        int length = BitConverter.ToInt32(data, offset);
        offset += 4;
        string result = Encoding.UTF8.GetString(data, offset, length);
        offset += length;
        return result;
    }

    public static object ConvertTypeByteToPrimitive(byte[] data, ref int offset, Type type)
    {
        int size = Marshal.SizeOf(type);
        byte[] buffer = new byte[size];
        Array.Copy(data, offset, buffer, 0, size);
        offset += size;

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(buffer, 0, ptr, size);
        object result = Marshal.PtrToStructure(ptr, type);
        Marshal.FreeHGlobal(ptr);
        return result;
    }

    public static object ConvertTypeByteToStruct(byte[] data, ref int offset, Type type)
    {
        return ConvertTypeByteToPrimitive(data, ref offset, type);
    }

    public static Array ConvertTypeByteToArray(byte[] data, ref int offset, Type elementType)
    {
        int length = (int)ConvertTypeByteToPrimitive(data, ref offset, typeof(int));
        Array array = Array.CreateInstance(elementType, length);

        MethodInfo convertMethod = typeof(TypeByte2TypeConverter)
            .GetMethod(nameof(Convert), new[] { typeof(byte[]), typeof(int).MakeByRefType() })
            .MakeGenericMethod(elementType);

        for (int i = 0; i < length; i++)
        {
            object[] parameters = new object[] { data, offset };
            object item = convertMethod.Invoke(null, parameters);
            offset = (int)parameters[1];
            array.SetValue(item, i);
        }

        return array;
    }


    public static object ConvertTypeByteToList(byte[] data, ref int offset, Type elementType)
    {
        int count = (int)ConvertTypeByteToPrimitive(data, ref offset, typeof(int));
        Type listType = typeof(List<>).MakeGenericType(elementType);
        IList list = (IList)Activator.CreateInstance(listType);

        MethodInfo convertMethod = typeof(TypeByte2TypeConverter)
            .GetMethod(nameof(Convert), new[] { typeof(byte[]), typeof(int).MakeByRefType() })
            .MakeGenericMethod(elementType);

        for (int i = 0; i < count; i++)
        {
            object[] parameters = new object[] { data, offset };
            object item = convertMethod.Invoke(null, parameters);
            offset = (int)parameters[1];
            list.Add(item);
        }
        return list;
    }


    public static object ConvertTypeByteToDictionary(byte[] data, ref int offset, Type keyType, Type valueType)
    {
        int count = (int)ConvertTypeByteToPrimitive(data, ref offset, typeof(int));

        Type dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        IDictionary dict = (IDictionary)Activator.CreateInstance(dictType);

        // Convert<T>(byte[], ref int) 메서드 가져오기
        MethodInfo convertKeyMethod = typeof(TypeByte2TypeConverter)
            .GetMethod(nameof(Convert), new[] { typeof(byte[]), typeof(int).MakeByRefType() })
            .MakeGenericMethod(keyType);

        MethodInfo convertValueMethod = typeof(TypeByte2TypeConverter)
            .GetMethod(nameof(Convert), new[] { typeof(byte[]), typeof(int).MakeByRefType() })
            .MakeGenericMethod(valueType);

        for (int i = 0; i < count; i++)
        {
            object[] keyParams = new object[] { data, offset };
            object key = convertKeyMethod.Invoke(null, keyParams);
            offset = (int)keyParams[1];

            object[] valueParams = new object[] { data, offset };
            object value = convertValueMethod.Invoke(null, valueParams);
            offset = (int)valueParams[1];

            dict.Add(key, value);
        }

        return dict;
    }


    public static object ConvertTypeByteToHashSet(byte[] data, ref int offset, Type elementType)
    {
        int count = (int)ConvertTypeByteToPrimitive(data, ref offset, typeof(int));
        Type setType = typeof(HashSet<>).MakeGenericType(elementType);
        object set = Activator.CreateInstance(setType);
        MethodInfo addMethod = setType.GetMethod("Add");

        MethodInfo convertMethod = typeof(TypeByte2TypeConverter)
            .GetMethod(nameof(Convert), new[] { typeof(byte[]), typeof(int).MakeByRefType() })
            .MakeGenericMethod(elementType);

        for (int i = 0; i < count; i++)
        {
            object[] parameters = new object[] { data, offset };
            object item = convertMethod.Invoke(null, parameters);
            offset = (int)parameters[1];
            addMethod.Invoke(set, new object[] { item });
        }

        return set;
    }


    public static object ConvertTypeByteToClass(byte[] data, ref int offset, Type type)
    {
        object instance = Activator.CreateInstance(type);
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var fields = type.GetFields(flags)
            .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null);

        foreach (var field in fields)
        {
            Type fieldType = field.FieldType;

            MethodInfo convertMethod = typeof(TypeByte2TypeConverter)
                .GetMethod(nameof(Convert), new[] { typeof(byte[]), typeof(int).MakeByRefType() })
                .MakeGenericMethod(fieldType);

            object[] parameters = new object[] { data, offset };
            object fieldValue = convertMethod.Invoke(null, parameters);
            offset = (int)parameters[1];

            field.SetValue(instance, fieldValue);
        }

        return instance;
    }

}
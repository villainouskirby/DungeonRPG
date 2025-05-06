using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.XR;

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


    // 캐시: 제네릭 Convert 메서드, 필드 정보, 필드 설정 델리게이트 등을 캐싱합니다.
    private static readonly Dictionary<Type, MethodInfo> _convertMethodCache = new Dictionary<Type, MethodInfo>();
    private static readonly Dictionary<Type, FieldInfo[]> _fieldCache = new Dictionary<Type, FieldInfo[]>();
    private static readonly Dictionary<(Type, string), Action<object, object>> _fieldSetterCache = new Dictionary<(Type, string), Action<object, object>>();

    public static T Convert<T>(byte[] data)
    {
        int offset = 0;
        return Convert<T>(data, ref offset);
    }

    public static T Convert<T>(byte[] data, ref int offset)
    {
        Type type = typeof(T);

        if (type == typeof(string))
            return (T)(object)ConvertTypeByte2LenString(data, ref offset);

        if (type == typeof(Texture2D))
            return (T)(object)ConvertTypeByte2Texture2D(data, ref offset);

        if (type.IsArray)
            return (T)(object)ConvertTypeByte2Array(data, ref offset, type.GetElementType());

        if (type.IsEnum)
            return (T)ConvertTypeByte2Enum(data, ref offset, type);

        if (type.IsGenericType)
        {
            var genericTypeDef = type.GetGenericTypeDefinition();

            if (genericTypeDef == typeof(List<>))
                return (T)ConvertTypeByte2List(data, ref offset, type.GetGenericArguments()[0]);

            if (genericTypeDef == typeof(Dictionary<,>))
                return (T)ConvertTypeByte2Dictionary(data, ref offset, type.GetGenericArguments()[0], type.GetGenericArguments()[1]);

            if (genericTypeDef == typeof(HashSet<>))
                return (T)ConvertTypeByte2HashSet(data, ref offset, type.GetGenericArguments()[0]);
        }

        if (type.IsPrimitive)
            return (T)ConvertTypeByte2Primitive(data, ref offset, type);

        if (type.IsEnum)
            return (T)ConvertTypeByte2Enum(data, ref offset, type);

        if (type.IsValueType)
            return (T)ConvertTypeByte2Struct(data, ref offset, type);

        if (type.IsClass)
            return (T)ConvertTypeByte2Class(data, ref offset, type);

        throw new NotSupportedException($"[{type.FullName}] 타입은 역직렬화 지원되지 않습니다.");
    }

    public static string ConvertTypeByte2LenString(byte[] data, ref int offset)
    {
        int length = (int)ConvertTypeByte2Primitive(data, ref offset, typeof(int));
        string result = Encoding.UTF8.GetString(data, offset, length);
        offset += length;
        return result;
    }

    public static Texture2D ConvertTypeByte2Texture2D(byte[] data, ref int offset)
    {
        int width = (int)ConvertTypeByte2Primitive(data, ref offset, typeof(int));
        int height = (int)ConvertTypeByte2Primitive(data, ref offset, typeof(int));
        string format = ConvertTypeByte2LenString(data, ref offset);
        int mipmapCount = (int)ConvertTypeByte2Primitive(data, ref offset, typeof(int));
        int rawDataLength = (int)ConvertTypeByte2Primitive(data, ref offset, typeof(int));

        // Buffer.BlockCopy를 사용하여 빠르게 복사
        byte[] rawData = new byte[rawDataLength];
        Buffer.BlockCopy(data, offset, rawData, 0, rawDataLength);
        offset += rawDataLength;

        TextureFormat texFormat = (TextureFormat)Enum.Parse(typeof(TextureFormat), format);
        Texture2D tex = new Texture2D(width, height, texFormat, mipmapCount > 1);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.LoadRawTextureData(rawData);
        tex.Apply();

        return tex;
    }

    public static object ConvertTypeByte2Enum(byte[] data, ref int offset, Type type)
    {
        int enumValue = (int)ConvertTypeByte2Primitive(data, ref offset, typeof(int));
        return Enum.ToObject(type, enumValue);
    }

    public static object ConvertTypeByte2Primitive(byte[] data, ref int offset, Type type)
    {
        ReadOnlySpan<byte> span = data.AsSpan(offset);
        object result = null;

        if (type == typeof(short))
        {
            result = BinaryPrimitives.ReadInt16LittleEndian(span);
            offset += sizeof(short);
        }
        else if (type == typeof(ushort))
        {
            result = BinaryPrimitives.ReadUInt16LittleEndian(span);
            offset += sizeof(ushort);
        }
        else if (type == typeof(int))
        {
            result = BinaryPrimitives.ReadInt32LittleEndian(span);
            offset += sizeof(int);
        }
        else if (type == typeof(uint))
        {
            result = BinaryPrimitives.ReadUInt32LittleEndian(span);
            offset += sizeof(uint);
        }
        else if (type == typeof(long))
        {
            result = BinaryPrimitives.ReadInt64LittleEndian(span);
            offset += sizeof(long);
        }
        else if (type == typeof(ulong))
        {
            result = BinaryPrimitives.ReadUInt64LittleEndian(span);
            offset += sizeof(ulong);
        }
        else if (type == typeof(float))
        {
            result = BitConverter.ToSingle(data, offset);
            offset += sizeof(float);
        }
        else if (type == typeof(double))
        {
            result = BitConverter.ToDouble(data, offset);
            offset += sizeof(double);
        }
        else if (type == typeof(bool))
        {
            // bool은 1바이트이므로 그대로 읽어도 괜찮습니다.
            result = data[offset] != 0;
            offset += sizeof(bool);
        }

        else
        {
            // 안전한 fallback: 기존의 Marshal 방식을 사용
            int size = Marshal.SizeOf(type);
            byte[] buffer = new byte[size];
            Buffer.BlockCopy(data, offset, buffer, 0, size);
            offset += size;
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(buffer, 0, ptr, size);
                result = Marshal.PtrToStructure(ptr, type);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        return result;
    }

    public static object ConvertTypeByte2Struct(byte[] data, ref int offset, Type type)
    {
        return ConvertTypeByte2Primitive(data, ref offset, type);
    }

    // 캐싱된 제네릭 Convert 메서드 호출 (컬렉션 역직렬화 시 반복되는 Reflection 호출 최소화)
    private static MethodInfo GetConvertMethod(Type elementType)
    {
        if (!_convertMethodCache.TryGetValue(elementType, out var method))
        {
            method = typeof(TypeByte2TypeConverter)
                .GetMethod(nameof(Convert), new[] { typeof(byte[]), typeof(int).MakeByRefType() })
                .MakeGenericMethod(elementType);
            _convertMethodCache[elementType] = method;
        }
        return method;
    }

    public static Array ConvertTypeByte2Array(byte[] data, ref int offset, Type elementType)
    {
        int length = (int)ConvertTypeByte2Primitive(data, ref offset, typeof(int));
        Array array = Array.CreateInstance(elementType, length);
        MethodInfo convertMethod = GetConvertMethod(elementType);

        for (int i = 0; i < length; i++)
        {
            object[] parameters = new object[] { data, offset };
            object item = convertMethod.Invoke(null, parameters);
            offset = (int)parameters[1];
            array.SetValue(item, i);
        }
        return array;
    }

    public static object ConvertTypeByte2List(byte[] data, ref int offset, Type elementType)
    {
        int count = (int)ConvertTypeByte2Primitive(data, ref offset, typeof(int));

        Type listType = typeof(List<>).MakeGenericType(elementType);
        IList list = (IList)Activator.CreateInstance(listType, count);
        MethodInfo convertMethod = GetConvertMethod(elementType);

        for (int i = 0; i < count; i++)
        {
            object[] parameters = new object[] { data, offset };
            object item = convertMethod.Invoke(null, parameters);
            offset = (int)parameters[1];
            list.Add(item);
        }
        return list;
    }

    public static object ConvertTypeByte2Dictionary(byte[] data, ref int offset, Type keyType, Type valueType)
    {
        int count = (int)ConvertTypeByte2Primitive(data, ref offset, typeof(int));
        Type dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

        IDictionary dict = (IDictionary)Activator.CreateInstance(dictType, count);
        MethodInfo convertKeyMethod = GetConvertMethod(keyType);
        MethodInfo convertValueMethod = GetConvertMethod(valueType);

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

    public static object ConvertTypeByte2HashSet(byte[] data, ref int offset, Type elementType)
    {
        int count = (int)ConvertTypeByte2Primitive(data, ref offset, typeof(int));
        Type setType = typeof(HashSet<>).MakeGenericType(elementType);
        object set = Activator.CreateInstance(setType);
        MethodInfo addMethod = setType.GetMethod("Add");
        MethodInfo convertMethod = GetConvertMethod(elementType);

        for (int i = 0; i < count; i++)
        {
            object[] parameters = new object[] { data, offset };
            object item = convertMethod.Invoke(null, parameters);
            offset = (int)parameters[1];
            addMethod.Invoke(set, new object[] { item });
        }
        return set;
    }

    private static FieldInfo[] GetSerializableFields(Type type)
    {
        if (!_fieldCache.TryGetValue(type, out var fields))
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            fields = type.GetFields(flags)
                .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null)
                .ToArray();
            _fieldCache[type] = fields;
        }
        return fields;
    }

    public static object ConvertTypeByte2Class(byte[] data, ref int offset, Type type)
    {
        object instance = Activator.CreateInstance(type);
        FieldInfo[] fields = GetSerializableFields(type);

        foreach (var field in fields)
        {
            bool isSerializeReference = field.GetCustomAttribute<SerializeReference>() != null;
            bool isNull = !(bool)ConvertTypeByte2Primitive(data, ref offset, typeof(bool));
            if (isNull)
            {
                SetFieldValue(instance, field, null);
                continue;
            }

            if (isSerializeReference)
            {
                string typeName = ConvertTypeByte2LenString(data, ref offset);
                Type realType = Type.GetType(typeName);
                MethodInfo convertMethod = GetConvertMethod(realType);
                object[] parameters = new object[] { data, offset };
                object fieldValue = convertMethod.Invoke(null, parameters);
                offset = (int)parameters[1];
                SetFieldValue(instance, field, fieldValue);
            }
            else
            {
                Type fieldType = field.FieldType;
                MethodInfo convertMethod = GetConvertMethod(fieldType);
                object[] parameters = new object[] { data, offset };
                object fieldValue = convertMethod.Invoke(null, parameters);
                offset = (int)parameters[1];
                SetFieldValue(instance, field, fieldValue);
            }
        }
        return instance;
    }

    private static void SetFieldValue(object instance, FieldInfo field, object value)
    {
        var key = (field.DeclaringType, field.Name);
        if (!_fieldSetterCache.TryGetValue(key, out var setter))
        {
            
            // Expression을 이용해 필드 setter 델리게이트를 생성 및 캐싱
            var targetParam = Expression.Parameter(typeof(object));
            var valueParam = Expression.Parameter(typeof(object));
            var fieldExp = Expression.Field(Expression.Convert(targetParam, field.DeclaringType), field);
            var assignExp = Expression.Assign(fieldExp, Expression.Convert(valueParam, field.FieldType));
            var lambda = Expression.Lambda<Action<object, object>>(assignExp, targetParam, valueParam);
            setter = lambda.Compile();
            _fieldSetterCache[key] = setter;
        }
        setter(instance, value);
    }

}
using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public static partial class TypeByte2TypeConverter
{
    public static string ConvertTypeByte2string(ReadOnlySpan<byte> bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }
    public static int ConvertTypeByte2int(ReadOnlySpan<byte> bytes)
    {
        return ConvertFromBytes<int>(bytes);
    }

    public static float ConvertTypeByte2float(ReadOnlySpan<byte> bytes)
    {
        return ConvertFromBytes<float>(bytes);
    }

    public static double ConvertTypeByte2double(ReadOnlySpan<byte> bytes)
    {
        return ConvertFromBytes<double>(bytes);
    }

    public static long ConvertTypeByte2long(ReadOnlySpan<byte> bytes)
    {
        return ConvertFromBytes<long>(bytes);
    }

    public static short ConvertTypeByte2short(ReadOnlySpan<byte> bytes)
    {
        return ConvertFromBytes<short>(bytes);
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
        return ConvertFromBytes<Vector2>(bytes);
    }

    public static Vector3 ConvertTypeByte2Vector3(ReadOnlySpan<byte> bytes)
    {
        return ConvertFromBytes<Vector3>(bytes);
    }

    public static Quaternion ConvertTypeByte2Quaternion(ReadOnlySpan<byte> bytes)
    {
        return ConvertFromBytes<Quaternion>(bytes);
    }

    public static Color ConvertTypeByte2Color(ReadOnlySpan<byte> bytes)
    {
        return ConvertFromBytes<Color>(bytes);
    }

    private static T ConvertFromBytes<T>(ReadOnlySpan<byte> bytes) where T : struct
    {
        return MemoryMarshal.Read<T>(bytes);
    }
}
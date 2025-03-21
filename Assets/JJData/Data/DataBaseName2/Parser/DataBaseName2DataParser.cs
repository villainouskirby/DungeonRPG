using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;




public static class DataBaseName2DataParser
{
    readonly private static string _dataFilePath = "C:/Users/user/Documents/GitHub/2_UNITIYGAME/DungeonRPG/Assets/StreamingAssets/JJData/";
    private static Dictionary<string, int> _typeByteLength = new();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void SetXlsxData()
    {
        ConvertRowData2TestClass();
    }
    public static void ConvertRowData2TestClass()
    {
        int bufferSize = 2048;
        int stringLength;
        FileStream fileStream = new(Path.Combine(_dataFilePath, "DataBaseName2","TestClass.bin"), FileMode.Open, FileAccess.Read);
        Span<byte> buffer = stackalloc byte[bufferSize];
        int offset = 0;
        fileStream.Read(buffer);
        long rows = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
        offset += 8;
        DataBaseName2.TestClass = new DataBaseName2_TestClass[rows];
        for(int row = 0; row < rows; row++)
        {
            DataBaseName2_TestClass sheetRowData = new();
            if (offset + 4 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Test12 = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            if (offset + 4 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            if (offset + stringLength >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Test23 = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            if (offset + 4 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Test34 = TypeByte2TypeConverter.ConvertTypeByte2float(buffer.Slice(offset, 4));
            offset += 4;
            DataBaseName2.TestClass[row] = sheetRowData;
        }
    }

}

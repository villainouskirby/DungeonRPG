using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;




public static class SmithyTreeDataParser
{
    readonly private static string _dataFilePath = "C:/Users/villainouskirby/Documents/GitHub/DungeonRPG/Assets/StreamingAssets/JJData/";
    private static Dictionary<string, int> _typeByteLength = new();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void SetXlsxData()
    {
        ConvertRowData2SmithyTree_Tool();
        ConvertRowData2SmithyTree_Tool_Setting();
        ConvertRowData2SmithyTree_Weapon();
        ConvertRowData2SmithyTree_Weapon_Setting();
        ConvertRowData2SmithyTree_Armor();
        ConvertRowData2SmithyTree_Armor_Setting();
    }
    public static void ConvertRowData2SmithyTree_Weapon_Setting()
    {
        int bufferSize = 2048;
        int stringLength;
        FileStream fileStream = new(Path.Combine(_dataFilePath, "SmithyTree","SmithyTree_Weapon_Setting.bin"), FileMode.Open, FileAccess.Read);
        Span<byte> buffer = stackalloc byte[bufferSize];
        int offset = 0;
        fileStream.Read(buffer);
        long rows = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
        offset += 8;
        SmithyTree.SmithyTree_Weapon_Setting = new SmithyTree_SmithyTree_Weapon_Setting[rows];
        for(int row = 0; row < rows; row++)
        {
            SmithyTree_SmithyTree_Weapon_Setting sheetRowData = new();
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
            sheetRowData.Sector_setting = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
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
            sheetRowData.Next_sector_setting = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            SmithyTree.SmithyTree_Weapon_Setting[row] = sheetRowData;
        }
    }

    public static void ConvertRowData2SmithyTree_Tool_Setting()
    {
        int bufferSize = 2048;
        int stringLength;
        FileStream fileStream = new(Path.Combine(_dataFilePath, "SmithyTree","SmithyTree_Tool_Setting.bin"), FileMode.Open, FileAccess.Read);
        Span<byte> buffer = stackalloc byte[bufferSize];
        int offset = 0;
        fileStream.Read(buffer);
        long rows = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
        offset += 8;
        SmithyTree.SmithyTree_Tool_Setting = new SmithyTree_SmithyTree_Tool_Setting[rows];
        for(int row = 0; row < rows; row++)
        {
            SmithyTree_SmithyTree_Tool_Setting sheetRowData = new();
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
            sheetRowData.Sector_setting = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
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
            sheetRowData.Next_sector_setting = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            SmithyTree.SmithyTree_Tool_Setting[row] = sheetRowData;
        }
    }

    public static void ConvertRowData2SmithyTree_Armor()
    {
        int bufferSize = 2048;
        int stringLength;
        FileStream fileStream = new(Path.Combine(_dataFilePath, "SmithyTree","SmithyTree_Armor.bin"), FileMode.Open, FileAccess.Read);
        Span<byte> buffer = stackalloc byte[bufferSize];
        int offset = 0;
        fileStream.Read(buffer);
        long rows = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
        offset += 8;
        SmithyTree.SmithyTree_Armor = new SmithyTree_SmithyTree_Armor[rows];
        for(int row = 0; row < rows; row++)
        {
            SmithyTree_SmithyTree_Armor sheetRowData = new();
            if (offset + 8 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Tree_cell_id = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
            offset += 8;
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
            sheetRowData.Sector_setting = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            if (offset + 8 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Tool_id = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
            offset += 8;
            if (offset + 8 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource1_id = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
            offset += 8;
            if (offset + 4 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource1_number = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            if (offset + 8 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource2_id = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
            offset += 8;
            if (offset + 4 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource2_number = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            if (offset + 8 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource3_id = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
            offset += 8;
            if (offset + 4 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resourcet3_number = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            if (offset + 8 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource4_id = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
            offset += 8;
            if (offset + 4 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource4_number = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            if (offset + 1 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Cell_unlock = TypeByte2TypeConverter.ConvertTypeByte2bool(buffer.Slice(offset, 1));
            offset += 1;
            SmithyTree.SmithyTree_Armor[row] = sheetRowData;
        }
    }

    public static void ConvertRowData2SmithyTree_Tool()
    {
        int bufferSize = 2048;
        int stringLength;
        FileStream fileStream = new(Path.Combine(_dataFilePath, "SmithyTree","SmithyTree_Tool.bin"), FileMode.Open, FileAccess.Read);
        Span<byte> buffer = stackalloc byte[bufferSize];
        int offset = 0;
        fileStream.Read(buffer);
        long rows = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
        offset += 8;
        SmithyTree.SmithyTree_Tool = new SmithyTree_SmithyTree_Tool[rows];
        for(int row = 0; row < rows; row++)
        {
            SmithyTree_SmithyTree_Tool sheetRowData = new();
            if (offset + 8 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Tree_cell_id = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
            offset += 8;
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
            sheetRowData.Sector_setting = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            if (offset + 8 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Tool_id = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
            offset += 8;
            if (offset + 8 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource1_id = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
            offset += 8;
            if (offset + 4 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource1_number = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            if (offset + 8 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource2_id = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
            offset += 8;
            if (offset + 4 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource2_number = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            if (offset + 8 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource3_id = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
            offset += 8;
            if (offset + 4 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resourcet3_number = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            if (offset + 8 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource4_id = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
            offset += 8;
            if (offset + 4 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource4_number = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            if (offset + 1 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Cell_unlock = TypeByte2TypeConverter.ConvertTypeByte2bool(buffer.Slice(offset, 1));
            offset += 1;
            SmithyTree.SmithyTree_Tool[row] = sheetRowData;
        }
    }

    public static void ConvertRowData2SmithyTree_Armor_Setting()
    {
        int bufferSize = 2048;
        int stringLength;
        FileStream fileStream = new(Path.Combine(_dataFilePath, "SmithyTree","SmithyTree_Armor_Setting.bin"), FileMode.Open, FileAccess.Read);
        Span<byte> buffer = stackalloc byte[bufferSize];
        int offset = 0;
        fileStream.Read(buffer);
        long rows = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
        offset += 8;
        SmithyTree.SmithyTree_Armor_Setting = new SmithyTree_SmithyTree_Armor_Setting[rows];
        for(int row = 0; row < rows; row++)
        {
            SmithyTree_SmithyTree_Armor_Setting sheetRowData = new();
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
            sheetRowData.Sector_setting = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
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
            sheetRowData.Next_sector_setting = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            SmithyTree.SmithyTree_Armor_Setting[row] = sheetRowData;
        }
    }

    public static void ConvertRowData2SmithyTree_Weapon()
    {
        int bufferSize = 2048;
        int stringLength;
        FileStream fileStream = new(Path.Combine(_dataFilePath, "SmithyTree","SmithyTree_Weapon.bin"), FileMode.Open, FileAccess.Read);
        Span<byte> buffer = stackalloc byte[bufferSize];
        int offset = 0;
        fileStream.Read(buffer);
        long rows = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
        offset += 8;
        SmithyTree.SmithyTree_Weapon = new SmithyTree_SmithyTree_Weapon[rows];
        for(int row = 0; row < rows; row++)
        {
            SmithyTree_SmithyTree_Weapon sheetRowData = new();
            if (offset + 8 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Tree_cell_id = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
            offset += 8;
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
            sheetRowData.Sector_setting = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            if (offset + 8 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Weapon_id = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
            offset += 8;
            if (offset + 8 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource1_id = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
            offset += 8;
            if (offset + 4 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource1_number = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            if (offset + 8 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource2_id = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
            offset += 8;
            if (offset + 4 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource2_number = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            if (offset + 8 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource3_id = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
            offset += 8;
            if (offset + 4 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resourcet3_number = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            if (offset + 8 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource4_id = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
            offset += 8;
            if (offset + 4 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Resource4_number = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            if (offset + 1 >= buffer.Length)
            {
                byte[] leftData = buffer.Slice(offset).ToArray();
                buffer.Clear();
                leftData.CopyTo(buffer);
                fileStream.Read(buffer.Slice(leftData.Length));
                offset = 0;
            }
            sheetRowData.Cell_unlock = TypeByte2TypeConverter.ConvertTypeByte2bool(buffer.Slice(offset, 1));
            offset += 1;
            SmithyTree.SmithyTree_Weapon[row] = sheetRowData;
        }
    }

}

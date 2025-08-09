using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;




public static class Item_InfoDataParser
{
    readonly private static string _dataFilePath = "JJData/";
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void SetXlsxData()
    {
        ConvertRowData2Item();
        ConvertRowData2UseItem();
        ConvertRowData2DropItem();
        ConvertRowData2Weapon();
        ConvertRowData2Armor();
        ConvertRowData2Backpack();
        ConvertRowData2Pouch();
        ConvertRowData2Potion();
        ConvertRowData2Condition();
    }
    public static void ConvertRowData2Backpack()
    {
        int stringLength = 0;
        TextAsset binAsset = Resources.Load<TextAsset>(Path.Combine(_dataFilePath, "Item_Info/Backpack"));
        Span<byte> buffer = binAsset.bytes;
        int offset = 0;
        long rows = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
        offset += 8;
        Item_Info.Backpack = new Item_Info_Backpack[rows];
        for(int row = 0; row < rows; row++)
        {
            Item_Info_Backpack sheetRowData = new();
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.id = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            sheetRowData.max_weight = TypeByte2TypeConverter.ConvertTypeByte2float(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.speed = TypeByte2TypeConverter.ConvertTypeByte2float(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.judge = TypeByte2TypeConverter.ConvertTypeByte2bool(buffer.Slice(offset, 1));
            offset += 1;
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.Explanation = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            Item_Info.Backpack[row] = sheetRowData;
        }
        Resources.UnloadAsset(binAsset);
    }

    public static void ConvertRowData2Item()
    {
        int stringLength = 0;
        TextAsset binAsset = Resources.Load<TextAsset>(Path.Combine(_dataFilePath, "Item_Info/Item"));
        Span<byte> buffer = binAsset.bytes;
        int offset = 0;
        long rows = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
        offset += 8;
        Item_Info.Item = new Item_Info_Item[rows];
        for(int row = 0; row < rows; row++)
        {
            Item_Info_Item sheetRowData = new();
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.id = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.name = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.sprite = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            sheetRowData.max_amount = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.rank = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.weight = TypeByte2TypeConverter.ConvertTypeByte2float(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.sell_price = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.purchase_price = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.usable = TypeByte2TypeConverter.ConvertTypeByte2bool(buffer.Slice(offset, 1));
            offset += 1;
            sheetRowData.wearable = TypeByte2TypeConverter.ConvertTypeByte2bool(buffer.Slice(offset, 1));
            offset += 1;
            sheetRowData.pouchable = TypeByte2TypeConverter.ConvertTypeByte2bool(buffer.Slice(offset, 1));
            offset += 1;
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.PAR_DT = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            Item_Info.Item[row] = sheetRowData;
        }
        Resources.UnloadAsset(binAsset);
    }

    public static void ConvertRowData2Armor()
    {
        int stringLength = 0;
        TextAsset binAsset = Resources.Load<TextAsset>(Path.Combine(_dataFilePath, "Item_Info/Armor"));
        Span<byte> buffer = binAsset.bytes;
        int offset = 0;
        long rows = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
        offset += 8;
        Item_Info.Armor = new Item_Info_Armor[rows];
        for(int row = 0; row < rows; row++)
        {
            Item_Info_Armor sheetRowData = new();
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.id = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            sheetRowData.hp = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.stamina = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.speed = TypeByte2TypeConverter.ConvertTypeByte2float(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.hiding = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.Explanation = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            Item_Info.Armor[row] = sheetRowData;
        }
        Resources.UnloadAsset(binAsset);
    }

    public static void ConvertRowData2Weapon()
    {
        int stringLength = 0;
        TextAsset binAsset = Resources.Load<TextAsset>(Path.Combine(_dataFilePath, "Item_Info/Weapon"));
        Span<byte> buffer = binAsset.bytes;
        int offset = 0;
        long rows = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
        offset += 8;
        Item_Info.Weapon = new Item_Info_Weapon[rows];
        for(int row = 0; row < rows; row++)
        {
            Item_Info_Weapon sheetRowData = new();
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.id = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            sheetRowData.atk = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.ability = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.ratio1 = TypeByte2TypeConverter.ConvertTypeByte2float(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.ratio2 = TypeByte2TypeConverter.ConvertTypeByte2float(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.strong_Ratio = TypeByte2TypeConverter.ConvertTypeByte2float(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.max_charge = TypeByte2TypeConverter.ConvertTypeByte2float(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.strong_speed = TypeByte2TypeConverter.ConvertTypeByte2float(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.guard_ratio = TypeByte2TypeConverter.ConvertTypeByte2float(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.just_guard = TypeByte2TypeConverter.ConvertTypeByte2float(buffer.Slice(offset, 4));
            offset += 4;
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.Explanation = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            Item_Info.Weapon[row] = sheetRowData;
        }
        Resources.UnloadAsset(binAsset);
    }

    public static void ConvertRowData2DropItem()
    {
        int stringLength = 0;
        TextAsset binAsset = Resources.Load<TextAsset>(Path.Combine(_dataFilePath, "Item_Info/DropItem"));
        Span<byte> buffer = binAsset.bytes;
        int offset = 0;
        long rows = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
        offset += 8;
        Item_Info.DropItem = new Item_Info_DropItem[rows];
        for(int row = 0; row < rows; row++)
        {
            Item_Info_DropItem sheetRowData = new();
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.id = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.Explanation = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            Item_Info.DropItem[row] = sheetRowData;
        }
        Resources.UnloadAsset(binAsset);
    }

    public static void ConvertRowData2Condition()
    {
        int stringLength = 0;
        TextAsset binAsset = Resources.Load<TextAsset>(Path.Combine(_dataFilePath, "Item_Info/Condition"));
        Span<byte> buffer = binAsset.bytes;
        int offset = 0;
        long rows = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
        offset += 8;
        Item_Info.Condition = new Item_Info_Condition[rows];
        for(int row = 0; row < rows; row++)
        {
            Item_Info_Condition sheetRowData = new();
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.id = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.name = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.explanation = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            Item_Info.Condition[row] = sheetRowData;
        }
        Resources.UnloadAsset(binAsset);
    }

    public static void ConvertRowData2Pouch()
    {
        int stringLength = 0;
        TextAsset binAsset = Resources.Load<TextAsset>(Path.Combine(_dataFilePath, "Item_Info/Pouch"));
        Span<byte> buffer = binAsset.bytes;
        int offset = 0;
        long rows = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
        offset += 8;
        Item_Info.Pouch = new Item_Info_Pouch[rows];
        for(int row = 0; row < rows; row++)
        {
            Item_Info_Pouch sheetRowData = new();
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.id = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            sheetRowData.count = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.heal_buff = TypeByte2TypeConverter.ConvertTypeByte2float(buffer.Slice(offset, 4));
            offset += 4;
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.Explanation = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            Item_Info.Pouch[row] = sheetRowData;
        }
        Resources.UnloadAsset(binAsset);
    }

    public static void ConvertRowData2UseItem()
    {
        int stringLength = 0;
        TextAsset binAsset = Resources.Load<TextAsset>(Path.Combine(_dataFilePath, "Item_Info/UseItem"));
        Span<byte> buffer = binAsset.bytes;
        int offset = 0;
        long rows = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
        offset += 8;
        Item_Info.UseItem = new Item_Info_UseItem[rows];
        for(int row = 0; row < rows; row++)
        {
            Item_Info_UseItem sheetRowData = new();
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.id = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            sheetRowData.max_register_count = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.damage = TypeByte2TypeConverter.ConvertTypeByte2float(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.use_distance = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.sound_range = TypeByte2TypeConverter.ConvertTypeByte2float(buffer.Slice(offset, 4));
            offset += 4;
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.Explanation = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            Item_Info.UseItem[row] = sheetRowData;
        }
        Resources.UnloadAsset(binAsset);
    }

    public static void ConvertRowData2Potion()
    {
        int stringLength = 0;
        TextAsset binAsset = Resources.Load<TextAsset>(Path.Combine(_dataFilePath, "Item_Info/Potion"));
        Span<byte> buffer = binAsset.bytes;
        int offset = 0;
        long rows = TypeByte2TypeConverter.ConvertTypeByte2long(buffer.Slice(offset, 8));
        offset += 8;
        Item_Info.Potion = new Item_Info_Potion[rows];
        for(int row = 0; row < rows; row++)
        {
            Item_Info_Potion sheetRowData = new();
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.id = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            sheetRowData.effect = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.type = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.buff = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            stringLength = TypeByte2TypeConverter.ConvertTypeByte2int(buffer.Slice(offset, 4));
            offset += 4;
            sheetRowData.Explanation = TypeByte2TypeConverter.ConvertTypeByte2string(buffer.Slice(offset, stringLength));
            offset += stringLength;
            Item_Info.Potion[row] = sheetRowData;
        }
        Resources.UnloadAsset(binAsset);
    }

}

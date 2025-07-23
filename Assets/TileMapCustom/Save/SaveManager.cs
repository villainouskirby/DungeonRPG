using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TM = TileMapMaster;

public static class SaveManager
{
    public static SaveFileData SaveFileData { get { if (_saveFileData == null) LoadSaveFileData(); return _saveFileData; } }
    public static SaveFileData _saveFileData;
    public static SaveSlotIndex SaveSlotIndex = global::SaveSlotIndex.None;
    public static string SlotName = "";

    public static void SaveSlot() => SaveSlot(SaveSlotIndex, SlotName);
    public static void Load() => Load(LoadSlot(SaveSlotIndex));
    public static void NewSlot() => NewSlot(SaveSlotIndex, SlotName);

    public static void SaveSlot(SaveSlotIndex saveSlotIndex, string slotName)
    {
        SaveData saveData = new();
        saveData.mapType = TileMapMaster.Instance.MapType;
        saveData.PlayerPos = TileMapMaster.Instance.Player.transform.position;

        ISave[] saves = GameObject
            .FindObjectsOfType<MonoBehaviour>(true)
            .OfType<ISave>()
            .ToArray();

        for (int i = 0; i < saves.Length; i++)
            saves[i].Save(saveData);

        JJSave.LSave(saveData, $"SaveData", $"SaveFile/{saveSlotIndex}/");
    }

    public static SaveData LoadSlot(SaveSlotIndex saveSlotIndex)
    {
        SaveData saveData;
        JJSave.LLoad(out saveData, $"SaveData", $"SaveFile/{saveSlotIndex}/");
        return saveData;
    }

    public static void Load(SaveData saveData)
    {
        TileMapMaster.Instance.Player.transform.position = saveData.PlayerPos;
        TileMapMaster.Instance.MapType = saveData.mapType;

        ISave[] saves = GameObject
            .FindObjectsOfType<MonoBehaviour>(true)
            .OfType<ISave>()
            .ToArray();

        for (int i = 0; i < saves.Length; i++)
            saves[i].Load(saveData);
    }

    public static void LoadBase(SaveData saveData)
    {
        TileMapMaster.Instance.Player.transform.position = saveData.PlayerPos;
        TileMapMaster.Instance.MapType = saveData.mapType;

        ISave[] saves = GameObject
            .FindObjectsOfType<MonoBehaviour>(true)
            .OfType<ISave>()
            .ToArray();

        for (int i = 0; i < saves.Length; i++)
            if (saves[i] is ITileMapBase)
            saves[i].Load(saveData);
    }

    public static void LoadOption(SaveData saveData)
    {
        TileMapMaster.Instance.Player.transform.position = saveData.PlayerPos;
        TileMapMaster.Instance.MapType = saveData.mapType;

        ISave[] saves = GameObject
            .FindObjectsOfType<MonoBehaviour>(true)
            .OfType<ISave>()
            .ToArray();

        for (int i = 0; i < saves.Length; i++)
            if (saves[i] is ITileMapOption)
                saves[i].Load(saveData);
    }

    public static void LoadEtc(SaveData saveData)
    {
        TileMapMaster.Instance.Player.transform.position = saveData.PlayerPos;
        TileMapMaster.Instance.MapType = saveData.mapType;

        ISave[] saves = GameObject
            .FindObjectsOfType<MonoBehaviour>(true)
            .OfType<ISave>()
            .ToArray();

        for (int i = 0; i < saves.Length; i++)
            if (saves[i] is not ITileMapOption && saves[i] is not ITileMapBase)
                saves[i].Load(saveData);
    }

    public static void NewSlot(SaveSlotIndex saveSlotIndex, string slotName)
    {
        for (int i = 0; i < (int)MapEnum.Map2 + 1; i++)
        {
            TileMapData oriData;
            SaveData saveData = new();
            Debug.Log($"{((MapEnum)i).ToString()}_MapData");
            JJSave.RLoad(out oriData, $"{((MapEnum)i).ToString()}_MapData", ExtractorMaster.DataFileDirectory);
            JJSave.LSave(oriData.All, $"{((MapEnum)i).ToString()}_All", $"SaveFile/{saveSlotIndex}/{((MapEnum)i).ToString()}/");
            JJSave.LSave(saveData, $"SaveData", $"SaveFile/{saveSlotIndex}/");

            List<int> mapData = new();
            for (int j = 0; j < oriData.LayerData.Length; j++)
            {
                mapData.AddRange(oriData.LayerData[j].Tile);
            }

            JJSave.LSave(mapData.ToArray(), $"{((MapEnum)i).ToString()}_Stream", $"SaveFile/{saveSlotIndex}/{((MapEnum)i).ToString()}/", false);
        }

        LoadSaveFileData();
        SaveFileData.ManualSaveSlot[(int)saveSlotIndex] = MakeSlotData(slotName);
        JJSave.LSave(SaveFileData, "SaveSlotData", "SaveFile/");
    }

    public static void LoadSaveFileData()
    {
        JJSave.LLoad(out _saveFileData, "SaveSlotData", "SaveFile/");
        _saveFileData ??= new();
    }

    public static SaveSlotData MakeSlotData(string slotName)
    {
        SaveSlotData slotData = new(slotName, Vector3.zero, true);
        return slotData;
    }
}

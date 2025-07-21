using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TM = TileMapMaster;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get {  return _instance; } }
    private static SaveManager _instance;

    public void Awake()
    {
        _instance = this;
    }

    public string GenerateSlotName;
    [ContextMenu("Generate Slot")]
    public void GenerateSlot()
    {
        NewSlot(GenerateSlotName);
    }

    [ContextMenu("Save Slot")]
    public void SaveSlot()
    {
        SaveSlot(GenerateSlotName);
    }

    [ContextMenu("Load Slot")]
    public void LoadSlot()
    {
        TileMapMaster.Instance.LoadTilemap(LoadSlot(GenerateSlotName));
    }

    public void SaveSlot(string slotName)
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

        JJSave.LSave(saveData, $"{slotName}_SaveData", $"SaveFile/{slotName}/");
    }

    public SaveData LoadSlot(string slotName)
    {
        SaveData saveData;
        JJSave.LLoad(out saveData, $"{slotName}_SaveData", $"SaveFile/{slotName}/");
        return saveData;
    }

    public void Load(SaveData saveData)
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

    public void LoadBase(SaveData saveData)
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

    public void LoadOption(SaveData saveData)
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

    public void LoadEtc(SaveData saveData)
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

    public void NewSlot(string slotName)
    {
        for (int i = 0; i < (int)MapEnum.Map2 + 1; i++)
        {
            TileMapData oriData;
            SaveData saveData = new();
            saveData.PlayerPos = Vector2.zero;
            saveData.Items = new();
            saveData.ResourceNodeSpawner = new();
            Debug.Log($"{((MapEnum)i).ToString()}_MapData");
            JJSave.RLoad(out oriData, $"{((MapEnum)i).ToString()}_MapData", ExtractorMaster.DataFileDirectory);
            JJSave.LSave(oriData.All, $"{((MapEnum)i).ToString()}_All", $"SaveFile/{slotName}/{((MapEnum)i).ToString()}/");
            JJSave.LSave(saveData, $"{slotName}_SaveData", $"SaveFile/{slotName}/");

            List<int> mapData = new();
            for (int j = 0; j < oriData.LayerData.Length; j++)
            {
                mapData.AddRange(oriData.LayerData[j].Tile);
            }

            JJSave.LSave(mapData.ToArray(), $"{((MapEnum)i).ToString()}_Stream", $"SaveFile/{slotName}/{((MapEnum)i).ToString()}/", false);
        }
    }
}

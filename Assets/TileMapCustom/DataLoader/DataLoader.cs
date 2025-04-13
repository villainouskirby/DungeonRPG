using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataLoader : MonoBehaviour, ITileMapBase
{
    public static DataLoader Instance {get {return _instance;}}
    public static DataLoader _instance;

    [ContextMenu("Make New Slot")]
    public void NewSlot()
    {
        for (int i = 0; i < (int)MapEnum.Map1 + 1; i++)
        {
            TileMapData oriData;
            JJSave.RLoad(out oriData, $"{((MapEnum)i).ToString()}_MapData", ExtractorMaster.DataFileDirectory);

            JJSave.LSave(oriData.All, $"{((MapEnum)i).ToString()}_All", $"{((MapEnum)i).ToString()}/");

            List<int> mapData = new();
            for (int j = 0; j < oriData.LayerData.Length; j++)
            {
                mapData.AddRange(oriData.LayerData[j].Tile);
            }

            JJSave.LSave(mapData.ToArray(), $"{((MapEnum)i).ToString()}_Stream", $"{((MapEnum)i).ToString()}/", false);
        }
    }

    public TileMapDataAll All;

    public void Init()
    {
        _instance = this;
    }

    public void InitMap(MapEnum mapType)
    {
        JJSave.LLoad(out All, $"{mapType.ToString()}_All", $"{mapType.ToString()}/");
    }

    public void StartMap(MapEnum mapType)
    {
        InitMap(mapType);
    }

    public void SaveMapData()
    {
        
    }

    public int Prime { get {return (int)TileMapBasePrimeEnum.DataLoader;}}
}

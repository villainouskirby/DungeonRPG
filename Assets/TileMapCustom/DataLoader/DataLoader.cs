using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataLoader : MonoBehaviour, ITileMapBase
{
    public static DataLoader Instance {get {return _instance;}}
    private static DataLoader _instance;

    public TileMapDataAll All;

    public void Init()
    {
        _instance = this;
    }

    public void InitMap(MapEnum mapType)
    {
        JJSave.LLoad(out All, $"{mapType.ToString()}_All", $"SaveFile/{TileMapMaster.Instance.SlotName}/{mapType.ToString()}/");
    }

    public void StartMap(MapEnum mapType)
    {
    }

    public void SaveMapData()
    {
        
    }

    public int Prime { get {return (int)TileMapBasePrimeEnum.DataLoader;}}
}

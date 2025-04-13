using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Windows;

public class ExtractorMaster : MonoBehaviour
{
    public static ExtractorMaster Instance { get { return _instance; } }
    private static ExtractorMaster _instance;

    static public string        DataFileDirectory = "TileMapData/";
    static public MapEnum       MapType = MapEnum.Map1;
    static public int           ChunkSize = 16;

    public GameObject           LayerRoot;

    [Header("Layer Wall Settings")]
    public List<Sprite>         WallType;
    public bool                 IndividualWall = false;
    public List<List<Sprite>>   WallSettings;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        Extract();
    }

    public void Extract()
    {
        IExtractor[] extractor = gameObject.GetComponentsInChildren<IExtractor>();
        TileMapData mapData = new();
        mapData.All = new();

        for (int i = 0; i < extractor.Length; i++)
        {
            extractor[i].Extract(MapType, ref mapData);
        }

        mapData.All.Setting = new();
        if (mapData.All.InteractionObjData == null)
            mapData.All.Setting.OptionsActive[(int)TileMapOptionEnum.InteractionObjManager].Active = false;
        if (mapData.All.SpawnerData == null)
            mapData.All.Setting.OptionsActive[(int)TileMapOptionEnum.SpawnerManager].Active = false;
        mapData.All.Setting.Init();

        JJSave.RSave(mapData, $"{MapType}_MapData", DataFileDirectory);
        AssetDatabase.Refresh();
    }
}

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
    public MapEnum              Map;

    [Header("Layer Wall Settings")]
    public MapSpriteList        WallType;
    public bool                 IndividualWall = false;
    public List<MapSpriteList>  WallSettings;

    [Header("Shadow Settings")]
    public MapSpriteList ShadowType;
    public bool IndividualShadow = false;
    public List<MapSpriteList> ShadowSettings;

    [HideInInspector]
    public int[] ShadowSpriteIndex;
    [HideInInspector]
    public Vector2Int StartPos;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        MapType = Map;
        Extract();
    }

    public Vector2Int CorrectPos(Vector2Int pos)
    {
        return pos - StartPos;
    }

    public Vector2 CorrectPos(Vector2 pos)
    {
        return new(pos.x - StartPos.x, pos.y - StartPos.y);
    }

    public void Extract()
    {
        IExtractorFirst[] extractorFirst = gameObject.GetComponentsInChildren<IExtractorFirst>();
        IExtractor[] extractor = gameObject.GetComponentsInChildren<IExtractor>();
        IExtractorLate[] extractorLate = gameObject.GetComponentsInChildren<IExtractorLate>();
        TileMapData mapData = new();
        mapData.All = new();

        for (int i = 0; i < extractorFirst.Length; i++)
        {
            extractorFirst[i].Extract(MapType, mapData);
        }
        for (int i = 0; i < extractor.Length; i++)
        {
            extractor[i].Extract(MapType, mapData);
        }
        for (int i = 0; i < extractorLate.Length; i++)
        {
            extractorLate[i].Extract(MapType, mapData);
        }

        mapData.All.Setting = new();
        if (mapData.All.InteractionObjData == null)
            mapData.All.Setting.OptionsActive[(int)TileMapOptionEnum.InteractionObjManager].Active = false;
        if (mapData.All.SpawnerData == null || mapData.All.SpawnerData.Count == 0)
            mapData.All.Setting.OptionsActive[(int)TileMapOptionEnum.SpawnerManager].Active = false;
        if (mapData.All.CameraAreaData == null || mapData.All.CameraAreaData.Length == 0)
            mapData.All.Setting.OptionsActive[(int)TileMapOptionEnum.CameraAreaManager].Active = false;

        mapData.All.Setting.Init();

        JJSave.RSave(mapData, $"{MapType}_MapData", DataFileDirectory);
        AssetDatabase.Refresh();
    }
}

[System.Serializable]
public class MapSpriteList
{
    public List<Sprite> Sprites = new();
}
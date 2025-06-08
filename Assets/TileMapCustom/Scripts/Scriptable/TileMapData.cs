using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[System.Serializable]
public class TileMapData
{
    public TileMapDataAll All;

    // StreamData
    // 각 Layer 타일 정보
    public TileMapLayerData[] LayerData;
}

[System.Serializable]
public class TileMapDataAll
{
    // FixedData
    // 청크가 가로 세로 몇개 있는지
    public int Width;
    public int Height;
    // 한 청크에서 가로 타일 길이 (가로 = 세로)
    public int ChunkSize;
    // Layer가 총 몇개인지
    public int LayerCount;
    // 전체 맵이 공유하는 Texture Mapping
    public Texture2D[] MapTexture;
    // MapSetting 데이터
    public MapSettings Setting;
    // Deco 데이터
    public Dictionary<Vector2Int, List<DecoObjData>> decoObjData;
    // CameraArea 데이터
    public CameraAreaData[] CameraAreaData;

    // Wall, LayerIndex 데이터
    public TileMapLayerInfo[] TileMapLayerInfo;


    // SaveData
    // Spawner 데이터
    public SpawnerInfoData SpawnerInfoData;
    public Dictionary<string, SpawnerData> SpawnerData;
    // Interaction 데이터
    public InteractionObjData InteractionObjData;
    // 플레이어가 시작하는 위치
    public Vector2Int PlayerSpawnTilePos;
}
[System.Serializable]
public class TileMapDataStream
{
    public int[] MapStreamData;

    public TileMapDataStream(int[] mapStreamData)
    {
        MapStreamData = mapStreamData;
    }
}

[System.Serializable]
public class TileMapLayerData
{
    public int[] Tile;
}

[System.Serializable]
public class TileMapLayerInfo
{
    public int LayerIndex;
    public int[] WallTileIndex;
    public float Z;
}
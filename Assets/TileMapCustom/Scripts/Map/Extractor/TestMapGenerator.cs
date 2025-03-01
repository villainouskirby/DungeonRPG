using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TestMapGenerator : MonoBehaviour
{
    static public string DataFilePath = "Assets/Resources/";
    static public string DataFileDirectory = "TileMapData/";

    [Header("TestMap Settings")]
    public int width = 1000;
    public int height = 1000;
    public float scale = 1.0f;
    public MapEnum MapType;

    public int[,] GenerateMap()
    {
        int[,] map = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = (float)x / width * scale;
                float yCoord = (float)y / height * scale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);

                // 노이즈 값에 따라 땅(0) 또는 빈 공간(-1) 결정
                map[x, y] = sample > 0.5f ? 0 : 1;
            }
        }

        return map;
    }

    [ContextMenu("MakeMap")]
    public void Generate()
    {
        int[,] tileArray = GenerateMap(); // Tilemap을 배열로 변환
        // ScriptableObject 생성
        TileMapData tileMapData = ScriptableObject.CreateInstance<TileMapData>();
        tileMapData.SetTileData(tileArray);

        TileMapData visitedMapData = ScriptableObject.CreateInstance<TileMapData>();
        int[,] visitedTileArray = new int[tileMapData.Width, tileMapData.Height];
        visitedMapData.SetTileData(visitedTileArray);

        string assetName = MapType.ToString();
        // 저장할 폴더 경로
        string directoryPath = $"{DataFilePath}{DataFileDirectory}{assetName}/";
        
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        // ScriptableObject를 에셋으로 저장
        AssetDatabase.DeleteAsset($"{directoryPath}{assetName}.asset");
        AssetDatabase.CreateAsset(tileMapData, $"{directoryPath}{assetName}.asset");
        AssetDatabase.CreateAsset(visitedMapData, $"{directoryPath}{assetName}Visited.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"TilemapData asset created at: {directoryPath}");
    }
}

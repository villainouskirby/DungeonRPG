using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

public class TileMapExtractor : MonoBehaviour
{
    static public string DataFilePath = "Assets/Resources/";
    static public string DataFileDirectory = "TileMapData/";

    public Tilemap Tilemap;

    public MapEnum MapType;

    public Texture2D[] Texture;

    // 특정 스프라이트를 원하는 ID에 매핑
    public Dictionary<Texture2D, int> SpriteToId = new();

    public void TextureMapping()
    {
        for(int i = 0; i < Texture.Length; i++)
        {
            SpriteToId[Texture[i]] = i;
        }
    }

    public int[,] ExtractTilemapToArray()
    {
        Tilemap.CompressBounds();
        BoundsInt bounds = Tilemap.cellBounds;
        Vector2Int size = new(bounds.size.x, bounds.size.y);

        int[,] tileArray = new int[size.x, size.y];
        TileBase[] tiles = Tilemap.GetTilesBlock(bounds);

        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                int index = x + y * size.x;
                TileBase tileBase = tiles[index];
                if (tileBase == null)
                {
                    tileArray[x, y] = -1;
                    continue;
                }
                Tile tile = (Tile)tileBase;
                if (SpriteToId.TryGetValue(tile.sprite.texture, out int id))
                {
                    tileArray[x, y] = id; // 스프라이트에 매핑된 ID 저장
                }
                else
                {
                    tileArray[x, y] = -1;
                }
            }
        }
        return tileArray;
    }

    public Vector2 GetPlayerSpawnPos()
    {
        var childs = Tilemap.GetComponentsInChildren<Transform>();

        foreach(var child in childs)
        {
            if(child.transform.name == "PlayerSpawnPoint")
                return child.transform.position;
        }

        return Vector2.zero;
    }

    void Start()
    {
        TextureMapping(); // 텍스처 매핑 실행
        int[,] tileArray = ExtractTilemapToArray(); // Tilemap을 배열로 변환
        Vector2 playerSpawnPos = GetPlayerSpawnPos();
        playerSpawnPos = new(playerSpawnPos.x + 9, playerSpawnPos.y + 5);
        // ScriptableObject 생성
        TileMapData tileMapData = ScriptableObject.CreateInstance<TileMapData>();
        tileMapData.SetTileData(tileArray);
        tileMapData.PlayerSpawnPos = playerSpawnPos;

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

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.Windows;
using UnityEditor;

public class TileMapExtractor : MonoBehaviour
{
    public Tilemap tilemap; // Tilemap 참조

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
        BoundsInt bounds = tilemap.cellBounds;
        Vector2Int size = new(bounds.size.x, bounds.size.y);

        int[,] tileArray = new int[size.x, size.y];
        TileBase[] tiles = tilemap.GetTilesBlock(bounds);

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

    void Start()
    {
        TextureMapping(); // 텍스처 매핑 실행
        int[,] tileArray = ExtractTilemapToArray(); // Tilemap을 배열로 변환

        // ScriptableObject 생성
        TilemapDataTest tilemapData = ScriptableObject.CreateInstance<TilemapDataTest>();
        tilemapData.SetTileData(tileArray);

        // 저장할 폴더 경로
        string directoryPath = "Assets/Test/Map";
        string assetPath = $"{directoryPath}/TilemapData.asset";

        // 에디터 환경에서만 실행
#if UNITY_EDITOR
        // 폴더가 없으면 생성
        if (!AssetDatabase.IsValidFolder("Assets/Test"))
        {
            AssetDatabase.CreateFolder("Assets", "Test");
        }
        if (!AssetDatabase.IsValidFolder(directoryPath))
        {
            AssetDatabase.CreateFolder("Assets/Test", "Map");
        }

        // ScriptableObject를 에셋으로 저장
        AssetDatabase.CreateAsset(tilemapData, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"TilemapData asset created at: {assetPath}");
#endif
    }
}

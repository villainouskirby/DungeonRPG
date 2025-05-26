using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using EM = ExtractorMaster;
using System.Linq;
using System;
using UnityEngine.UIElements;

public class TileMapExtractor : MonoBehaviour, IExtractor
{
    public List<Tilemap> Tilemap;

    private List<Sprite> _sprites;


    public void Extract(MapEnum mapType, TileMapData mapData)
    {
        Tilemap = EM.Instance.LayerRoot.GetComponentsInChildren<Tilemap>().ToList();
        _sprites = new();

        Vector2Int size = new();

        List<int> emptyList = new();
        for(int i = 0; i < Tilemap.Count; i++)
        {
            Tilemap[i].CompressBounds();
            BoundsInt bounds = Tilemap[i].cellBounds;
            if (bounds.size.x == 0 && bounds.size.y == 0)
            {
                emptyList.Add(i);
                continue;
            }

            Vector3Int leftBottom = bounds.position;
            Vector2Int rightTop = new(leftBottom.x + bounds.size.x, leftBottom.y + bounds.size.y);

            size.x = Mathf.Max(size.x, rightTop.x);
            size.y = Mathf.Max(size.y, rightTop.y);
        }

        for (int i = emptyList.Count - 1; i >= 0; i--)
        {
            Tilemap.RemoveAt(emptyList[i]);
        }

        // Chunk 단위로 업스케일링
        Vector2Int chunkSize = new(Mathf.CeilToInt(((float)size.x / (float)EM.ChunkSize)), Mathf.CeilToInt(((float)size.y / (float)EM.ChunkSize)));
        size.x = chunkSize.x * EM.ChunkSize;
        size.y = chunkSize.y * EM.ChunkSize;

        mapData.All.Width = chunkSize.x;
        mapData.All.Height = chunkSize.y;
        mapData.All.ChunkSize = EM.ChunkSize;
        mapData.All.LayerCount = Tilemap.Count;

        Debug.Log($"맵 정보 - 가로 {mapData.All.Width} 세로 {mapData.All.Height}");

        Vector2 spawnPos = GameObject.FindWithTag("SpawnPoint").transform.position;
        mapData.All.PlayerSpawnTilePos = new(Mathf.FloorToInt(spawnPos.x), Mathf.FloorToInt(spawnPos.y));

        mapData.LayerData = new TileMapLayerData[Tilemap.Count];
        mapData.All.TileMapLayerInfo = new TileMapLayerInfo[Tilemap.Count];

        for (int i = 0; i < Tilemap.Count; i++)
        {
            TileMapLayerData layerData = ExtractLayer2TileMapData(Tilemap[i], size, chunkSize);
            mapData.All.TileMapLayerInfo[i] = new();
            mapData.All.TileMapLayerInfo[i].LayerIndex = Tilemap[i].gameObject.GetComponent<TilemapRenderer>().sortingOrder;
            mapData.All.TileMapLayerInfo[i].Z = Tilemap[i].transform.position.z;
            mapData.LayerData[i] = layerData;
        }

        mapData.All.MapTexture = ConvertSprite2Texture2D(_sprites);

        for (int i = 0; i < Tilemap.Count; i++)
        {
            if (EM.Instance.IndividualWall)
            {
                mapData.All.TileMapLayerInfo[i].WallTileIndex = ConvertSprite2Int(EM.Instance.WallSettings[i].Sprites);
            }
            else
            {
                mapData.All.TileMapLayerInfo[i].WallTileIndex = ConvertSprite2Int(EM.Instance.WallType.Sprites);
            }
        }

        // 그림자 타일 세팅
        // 일단 여기서 진행
        EM.Instance.ShadowSpriteIndex = ConvertSprite2Int(EM.Instance.ShadowType.Sprites);
    }

    public TileMapLayerData ExtractLayer2TileMapData(Tilemap targetLayer, Vector2Int size, Vector2Int chunkSize)
    {
        targetLayer.CompressBounds();
        BoundsInt bounds = targetLayer.cellBounds;
        Vector3Int startPos = bounds.position;

        // Data Info
        // Header
        int[] tileData = new int[size.x * size.y];
        Array.Fill(tileData, 0);

        TileBase[] tiles = targetLayer.GetTilesBlock(bounds);

        for (int y = 0; y < bounds.size.y; y++)
        {
            for (int x = 0; x < bounds.size.x; x++)
            {
                int correctX = x + startPos.x;
                int correctY = y + startPos.y;

                Vector2Int chunkIndex = new(correctX / EM.ChunkSize, correctY / EM.ChunkSize);
                Vector2Int localIndex = new(correctX % EM.ChunkSize, correctY % EM.ChunkSize);

                int chunkStartIndex = chunkIndex.x + chunkIndex.y * chunkSize.x;
                int localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;
                int index = localIndex.x + localIndex.y * EM.ChunkSize + localStartIndex;

                TileBase tileBase = tiles[x + y * bounds.size.x];

                Sprite sprite = null;

                if (tileBase == null)
                {
                    tileData[index] = 0;
                    continue;
                }
                else if (tileBase is Tile tile)
                {
                    sprite = tile.sprite;
                }
                else if (tileBase is RuleTile ruleTile)
                {
                    Vector3Int tilePos = new(correctX, correctY, 0);
                    TileData tileDataOut = new();
                    ruleTile.GetTileData(tilePos, targetLayer, ref tileDataOut);
                    sprite = tileDataOut.sprite;
                }
                
                tileData[index] = AddorGetSpriteIndex(sprite);
            }
        }

        TileMapLayerData result = new();
        result.Tile = tileData;

        return result;
    }

    public int AddorGetSpriteIndex(Sprite sprite)
    {
        if (_sprites.Contains(sprite))
            return _sprites.IndexOf(sprite) + 1; // 0은 기본 이미지가 들어가야함으로 1 추가하고 계산
        else
        {
            _sprites.Add(sprite);
            return _sprites.Count;
        }
    }

    public int[] ConvertSprite2Int(List<Sprite> sprite)
    {
        List<int> spriteType = new();
        for(int i = 0; i < sprite.Count; i++)
            if (_sprites.Contains(sprite[i]))
                spriteType.Add(_sprites.IndexOf(sprite[i]) + 1);

        return spriteType.ToArray();
    }

    public Texture2D[] ConvertSprite2Texture2D(List<Sprite> sprite)
    {
        Texture2D[] result = new Texture2D[sprite.Count + 1];

        Rect defaultRect = sprite[0].rect;
        Texture2D defaultTex = new((int)defaultRect.width, (int)defaultRect.height, TextureFormat.RGBA32, false);
        defaultTex.filterMode = FilterMode.Point;
        Color[] clearColors = new Color[defaultTex.width * defaultTex.height];
        for (int i = 0; i < clearColors.Length; i++)
        {
            clearColors[i] = Color.clear;
        }
        defaultTex.SetPixels(clearColors);
        defaultTex.Apply();
        result[0] = defaultTex;

        for (int i = 0; i < sprite.Count; i++)
        {
            Rect rect = sprite[i].rect;
            Texture2D sourceTex = sprite[i].texture;

            Texture2D newTex = new((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
            newTex.filterMode = FilterMode.Point;
            Color[] newTexColor = sourceTex.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
            newTex.SetPixels(newTexColor);
            newTex.Apply();

            result[i + 1] = newTex;
        }

        return result;
    }
}

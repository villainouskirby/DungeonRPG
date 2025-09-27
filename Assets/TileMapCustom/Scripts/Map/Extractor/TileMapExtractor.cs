using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using EM = ExtractorMaster;
using System;

public class TileMapExtractor : MonoBehaviour, IExtractorFirst
{
    public List<Tilemap> Tilemap;

    private List<Sprite> _sprites;
    private Dictionary<(Sprite a, Sprite b), Sprite> _mergeSprite;


    public void Extract(MapEnum mapType, TileMapData mapData)
    {
        Tilemap = new();
        _mergeSprite = new();
        int childCount = EM.Instance.LayerRoot.transform.childCount;
        for (int i = 0; i < childCount; i++)
            if (EM.Instance.LayerRoot.transform.GetChild(i).TryGetComponent(out Tilemap layerMap))
                Tilemap.Add(layerMap);
        _sprites = new();

        Vector2Int size = new();
        Vector2Int startPos = new();

        List<int> emptyList = new();
        for (int i = 0; i < Tilemap.Count; i++)
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
            startPos.x = Mathf.Min(startPos.x, leftBottom.x);
            startPos.y = Mathf.Min(startPos.y, leftBottom.y);
        }
        EM.Instance.StartPos = startPos;
        size = EM.Instance.CorrectPos(size);

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
        mapData.All.PlayerSpawnTilePos = EM.Instance.CorrectPos(mapData.All.PlayerSpawnTilePos);

        mapData.LayerData = new TileMapLayerData[Tilemap.Count];
        mapData.All.TileMapLayerInfo = new TileMapLayerInfo[Tilemap.Count];
        mapData.HeightData = new int[size.x * size.y];
        mapData.Wall03Data = new int[size.x * size.y];
        mapData.Wall47Data = new int[size.x * size.y];
        mapData.All.WallDataPool = new();

        for (int i = 0; i < Tilemap.Count; i++)
        {
            Tilemap decoMap = null;
            Tilemap heightMap = null;
            for (int j = 0; j < Tilemap[i].transform.childCount; j++)
            {
                if (!Tilemap[i].transform.GetChild(j).gameObject.activeSelf)
                    continue;
                string type = Tilemap[i].transform.GetChild(j).name.Split("_")[1];

                switch (type)
                {
                    case "Height":
                        heightMap = Tilemap[i].transform.GetChild(j).GetComponent<Tilemap>();
                        break;
                    case "Deco":
                        decoMap = Tilemap[i].transform.GetChild(j).GetComponent<Tilemap>();
                        break;
                }
            }

            TileMapLayerData layerData = ExtractLayer2TileMapData(Tilemap[i], size, chunkSize, decoMap);
            mapData.All.TileMapLayerInfo[i] = new();
            mapData.All.TileMapLayerInfo[i].LayerIndex = Tilemap[i].gameObject.GetComponent<TilemapRenderer>().sortingOrder;
            mapData.All.TileMapLayerInfo[i].Z = Tilemap[i].transform.position.z;
            mapData.LayerData[i] = layerData;

            ExtractLayerHeightData(mapData, heightMap, i, size, chunkSize.x);
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
        EM.Instance.WallSpriteIndex = ConvertSprite2Int(EM.Instance.WallType.Sprites);
        EM.Instance.ShadowSpriteIndex = ConvertSprite2Int(EM.Instance.ShadowType.Sprites);
    }

    /// <summary>
    /// int32를 4bit 단위로 쪼갰을 때, 특정 index(0~7) 위치에 값을 넣는다.
    /// </summary>
    public int SetIntSliceValue4Bit(int source, int index, int value)
    {
        int shift = index * 4;
        int mask = 0b1111 << shift;
        source &= ~mask;
        source |= (value << shift);
        return source;
    }

    public int GetIntSliceValue4Bit(int source, int index)
    {
        int shift = index * 4;
        return (source >> shift) & 0xF;
    }

    /// <summary>
    /// 32bit 정수(int)에서 8bit 단위 슬롯(index)에 value를 저장.
    /// </summary>
    public int SetIntSliceValue8Bit(int source, int index, int value)
    {
        int shift = index * 8;
        int mask = 0xFF << shift;
        source &= ~mask;
        source |= (value & 0xFF) << shift;
        return source;
    }

    public void ExtractLayerHeightData(TileMapData mapData, Tilemap heightMap, int layer, Vector2Int size, int mapChunkWidth)
    {
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
                mapData.HeightData[x + y * size.x] = SetIntSliceValue4Bit(mapData.HeightData[x + y * size.x], layer, EM.Instance.LayerDefaultHeight[layer]);
        }

        if (heightMap == null)
            return;
        heightMap.CompressBounds();
        BoundsInt bounds = heightMap.cellBounds;
        Vector3Int startPos = bounds.position;
        TileBase[] tiles = heightMap.GetTilesBlock(bounds);

        for (int y = 0; y < bounds.size.y; y++)
        {
            for (int x = 0; x < bounds.size.x; x++)
            {
                int correctX = x + startPos.x - EM.Instance.StartPos.x;
                int correctY = y + startPos.y - EM.Instance.StartPos.y;

                Vector2Int chunkIndex = new(correctX / EM.ChunkSize, correctY / EM.ChunkSize);
                Vector2Int localIndex = new(correctX % EM.ChunkSize, correctY % EM.ChunkSize);

                int chunkStartIndex = chunkIndex.x + chunkIndex.y * mapChunkWidth;
                int localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;
                int index = localIndex.x + localIndex.y * EM.ChunkSize + localStartIndex;

                TileBase tileBase = tiles[x + y * bounds.size.x];
                Sprite height = null;
                if (tileBase is Tile tile)
                    height = tile.sprite;

                if (height != null)
                {
                    if (height.name == "W")
                    {
                        mapData.HeightData[index] = SetIntSliceValue4Bit(mapData.HeightData[index], layer, 15);
                    }
                    else
                        mapData.HeightData[index] = SetIntSliceValue4Bit(mapData.HeightData[index], layer, int.Parse(height.name));
                }
            }
        }

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int chunkIndex = new(x / EM.ChunkSize, y / EM.ChunkSize);
                Vector2Int localIndex = new(x % EM.ChunkSize, y % EM.ChunkSize);

                int chunkStartIndex = chunkIndex.x + chunkIndex.y * mapChunkWidth;
                int localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;
                int index = localIndex.x + localIndex.y * EM.ChunkSize + localStartIndex;
                int correctIndex;

                if (GetIntSliceValue4Bit(mapData.HeightData[index], layer) == 15)
                {
                    WallData wallData = new();

                    int endY = y;
                    int startY = y;
                    while (endY < size.y - 1)
                    {
                        endY++;
                        chunkIndex = new(x / EM.ChunkSize, endY / EM.ChunkSize);
                        localIndex = new(x % EM.ChunkSize, endY % EM.ChunkSize);
                        chunkStartIndex = chunkIndex.x + chunkIndex.y * mapChunkWidth;
                        localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;
                        correctIndex = localIndex.x + localIndex.y * EM.ChunkSize + localStartIndex;

                        if (GetIntSliceValue4Bit(mapData.HeightData[correctIndex], layer) != 15)
                        { 
                            wallData.EndHeight = GetIntSliceValue4Bit(mapData.HeightData[correctIndex], layer);
                            break;
                        }

                        if (endY == size.y - 1)
                        {
                            wallData.EndHeight = EM.Instance.LayerDefaultHeight[layer];
                            break;
                        }
                    }
                    while (startY > 0)
                    {
                        startY--;
                        chunkIndex = new(x / EM.ChunkSize, startY / EM.ChunkSize);
                        localIndex = new(x % EM.ChunkSize, startY % EM.ChunkSize);
                        chunkStartIndex = chunkIndex.x + chunkIndex.y * mapChunkWidth;
                        localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;
                        correctIndex = localIndex.x + localIndex.y * EM.ChunkSize + localStartIndex;

                        if (GetIntSliceValue4Bit(mapData.HeightData[correctIndex], layer) != 15)
                        {
                            wallData.StartHeight = GetIntSliceValue4Bit(mapData.HeightData[correctIndex], layer);
                            break;
                        }

                        if (startY == 0)
                        {
                            wallData.StartHeight = EM.Instance.LayerDefaultHeight[layer];
                            break;
                        }
                    }
                    wallData.Length = endY - startY - 1;
                    wallData.StartY = startY + 1;
                    
                    bool isExist = false;
                    int wallIndex = 0;
                    for (int i = 0; i < mapData.All.WallDataPool.Count; i++)
                    {
                        if (mapData.All.WallDataPool[i].Length == wallData.Length &&
                            mapData.All.WallDataPool[i].StartY == wallData.StartY &&
                            mapData.All.WallDataPool[i].EndHeight == wallData.EndHeight &&
                            mapData.All.WallDataPool[i].StartHeight == wallData.StartHeight
                            ) { isExist = true; wallIndex = i; break; }
                    }

                    if (isExist)
                    {
                        if (layer < 4)
                            mapData.Wall03Data[index] = SetIntSliceValue8Bit(mapData.Wall03Data[index], layer, wallIndex);
                        else
                            mapData.Wall47Data[index] = SetIntSliceValue8Bit(mapData.Wall47Data[index], layer - 4, wallIndex);
                    }
                    else
                    {
                        wallIndex = mapData.All.WallDataPool.Count;
                        mapData.All.WallDataPool.Add(wallData);
                        if (layer < 4)
                            mapData.Wall03Data[index] = SetIntSliceValue8Bit(mapData.Wall03Data[index], layer, wallIndex);
                        else
                            mapData.Wall47Data[index] = SetIntSliceValue8Bit(mapData.Wall47Data[index], layer - 4, wallIndex);
                    }
                }
            }
        }
    }

    public TileMapLayerData ExtractLayer2TileMapData(Tilemap targetLayer, Vector2Int size, Vector2Int chunkSize, Tilemap decoMap)
    {
        targetLayer.CompressBounds();
        decoMap?.CompressBounds();

        BoundsInt bounds = targetLayer.cellBounds;
        Vector3Int startPos = bounds.position;

        // Data Info
        // Header
        int[] tileData = new int[size.x * size.y];
        Array.Fill(tileData, 0);

        TileBase[] tiles = targetLayer.GetTilesBlock(bounds);

        BoundsInt decoBounds = new();
        Vector3Int decoStartPos = Vector3Int.zero;
        TileBase[] decoTiles = null;
        if (decoMap != null)
        {
            decoBounds = decoMap.cellBounds;
            decoStartPos = decoBounds.position;
            decoTiles = decoMap.GetTilesBlock(decoBounds);
        }

        Sprite[,] oriSprites = new Sprite[size.x, size.y];
        for (int y = 0; y < bounds.size.y; y++)
        {
            for (int x = 0; x < bounds.size.x; x++)
            {
                int correctX = x + startPos.x - EM.Instance.StartPos.x;
                int correctY = y + startPos.y - EM.Instance.StartPos.y;
                Sprite oriSprite = GetTileSpriteByCorrectPos(targetLayer, x, y, startPos, chunkSize, bounds, tiles);
                oriSprites[correctX, correctY] = oriSprite;
            }
        }

        Sprite[,] decoSprites = new Sprite[size.x, size.y];
        if (decoMap != null)
        {
            for (int y = 0; y < decoBounds.size.y; y++)
            {
                for (int x = 0; x < decoBounds.size.x; x++)
                {
                    int correctX = x + decoStartPos.x - EM.Instance.StartPos.x;
                    int correctY = y + decoStartPos.y - EM.Instance.StartPos.y;
                    Sprite decoSprite = GetTileSpriteByCorrectPos(decoMap, x, y, decoStartPos, chunkSize, decoBounds, decoTiles);
                    decoSprites[correctX, correctY] = decoSprite;
                }
            }
        }

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int chunkIndex = new(x / EM.ChunkSize, y / EM.ChunkSize);
                Vector2Int localIndex = new(x % EM.ChunkSize, y % EM.ChunkSize);

                int chunkStartIndex = chunkIndex.x + chunkIndex.y * chunkSize.x;
                int localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;
                int index = localIndex.x + localIndex.y * EM.ChunkSize + localStartIndex;

                if (oriSprites[x, y] == null && decoSprites[x, y] == null)
                    tileData[index] = 0;
                else if (oriSprites[x, y] != null && decoSprites[x, y] == null)
                    tileData[index] = AddorGetSpriteIndex(oriSprites[x, y]);
                else if (oriSprites[x, y] == null && decoSprites[x, y] != null)
                    tileData[index] = AddorGetSpriteIndex(decoSprites[x, y]);
                else
                    tileData[index] = AddorGetSpriteIndex(Merge(oriSprites[x, y], decoSprites[x, y]));
            }
        }

        TileMapLayerData result = new();
        result.Tile = tileData;

        return result;
    }

    private Sprite GetTileSpriteByCorrectPos(Tilemap targetLayer, int x, int y, Vector3Int startPos, Vector2Int chunkSize, BoundsInt bounds, TileBase[] tiles)
    {
        int correctX = x + startPos.x - EM.Instance.StartPos.x;
        int correctY = y + startPos.y - EM.Instance.StartPos.y;

        TileBase tileBase = tiles[x + y * bounds.size.x];

        Sprite sprite = null;

        if (tileBase == null)
        {
            sprite = null;
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

        return sprite;
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

    public Sprite Merge(Sprite baseSprite, Sprite overlaySprite)
    {
        if (_mergeSprite.ContainsKey((baseSprite, overlaySprite)))
            return _mergeSprite[(baseSprite, overlaySprite)];

        Texture2D bt = ExtractTexture(baseSprite);
        Texture2D ot = ExtractTexture(overlaySprite);

        int w = bt.width;
        int h = bt.height;

        Color32[] basePx = bt.GetPixels32();
        Color32[] overPx = ot.GetPixels32();

        Color32[] outPx = new Color32[basePx.Length];
        Array.Copy(basePx, outPx, outPx.Length);

        for (int i = 0; i < outPx.Length; i++)
        {
            Color32 s = overPx[i];
            if (s.a == 0) continue;
            if (s.a == 255) { outPx[i] = s; continue; }

            Color32 d = outPx[i];
            float a = s.a / 255f, ia = 1f - a;
            outPx[i] = new Color32(
                (byte)Mathf.Clamp(Mathf.RoundToInt(s.r * a + d.r * ia), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(s.g * a + d.g * ia), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(s.b * a + d.b * ia), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(255 - (255 - d.a) * ia), 0, 255)
            );
        }

        Texture2D tex = new(w, h, TextureFormat.RGBA32, false)
        {
            filterMode = bt.filterMode,
            wrapMode = TextureWrapMode.Clamp
        };
        tex.SetPixels32(outPx);
        tex.Apply(false, false);

        float ppu = baseSprite.pixelsPerUnit > 0 ? baseSprite.pixelsPerUnit : 100f;
        Sprite result = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect);

        return result;
    }

    public Texture2D ExtractTexture(Sprite sprite, FilterMode filter = FilterMode.Point)
    {
        if (sprite == null) throw new ArgumentNullException(nameof(sprite));

        // 스프라이트가 차지하는 실제 사각형(픽셀 단위, 텍스처 공간)
        Rect r = sprite.textureRect;
        int x = Mathf.FloorToInt(r.x);
        int y = Mathf.FloorToInt(r.y);
        int w = Mathf.RoundToInt(r.width);
        int h = Mathf.RoundToInt(r.height);

        // 원본 텍스처에서 해당 영역 픽셀 읽기 (부분 추출)
        Color[] pixels;
        pixels = sprite.texture.GetPixels(x, y, w, h);

        // 새 텍스처 생성 후 복사
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = filter;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(pixels);
        tex.Apply(false);

        return tex;
    }
}

using UnityEngine;
using System.Collections.Generic;
using TreeEditor;

public class TileMapController : MonoBehaviour
{
    [Header("TileMap Settings")]
    private Material _tileMaterial; // 쉐이더가 적용된 머티리얼

    [Header("Texture Settings")]
    public Texture2D DefaultTexture; // 기본 Texture
    public Texture2D[] TileTexture; // 인스펙터에서 받을 Texture2D 배열
    private Texture2DArray _tileTextureArray; // 변환된 Texture2DArray

    [Header("Tile Settings")]
    public TilemapDataTest mapData;
    public float TileSize = 1;
    public int TileSizeX = 100;
    public int TileSizeY = 100;

    [Header("ViewMode")]
    public bool ViewMode = false;
    public float TargetTileSize = 1;

    private GraphicsBuffer _tileMapBuffer; // StructuredBuffer<int>
    private int[] _tileMapData; // 타일 데이터를 저장할 배열

    void Start()
    {
        SetMaterialData();
    }

    public void SetMaterialData()
    {
        CreateTexture2DArray();
        InitializeTileMap();
        _tileMaterial.SetFloat("_TileSize", TileSize);
        if(ViewMode)
        {
            _tileMaterial.SetFloat("_ViewTargetMode", 1.0f);
            _tileMaterial.SetFloat("_ViewTargetTileSize", TargetTileSize);
        }
        else
        {
            _tileMaterial.SetFloat("_ViewTargetMode", 0.0f);
        }
    }

    private void CheckTileMaterial()
    {
        if (_tileMaterial == null)
        {
            SpriteRenderer rend = GetComponent<SpriteRenderer>();
            if (rend != null) _tileMaterial = rend.material;
        }
    }

    /// <summary>
    /// Texture2D 배열을 Texture2DArray로 변환
    /// </summary>
    public void CreateTexture2DArray()
    {
        CheckTileMaterial();

        if (TileTexture.Length == 0)
        {
            Debug.LogError("[TileMapController] tileTextures 배열이 비어 있습니다.");
            return;
        }

        int width = TileTexture[0].width;
        int height = TileTexture[0].height;
        TextureFormat format = TileTexture[0].format;
        bool mipChain = false;

        _tileTextureArray = new(width, height, TileTexture.Length, format, mipChain);
        _tileTextureArray.anisoLevel = 1;

        for (int i = 0; i < TileTexture.Length; i++)
        {
            if (TileTexture[i] == null)
            {
                Debug.LogWarning($"[TileMapController] tileTextures[{i}]가 비어 있습니다. 기본 텍스처로 대체됩니다.");
                Graphics.CopyTexture(DefaultTexture, 0, 0, _tileTextureArray, i, 0);
                continue;
            }

            Graphics.CopyTexture(TileTexture[i], 0, 0, _tileTextureArray, i, 0);
        }

        _tileMaterial.SetTexture("_TileTexture", _tileTextureArray);
        _tileMaterial.SetInt("_TextureSize", _tileTextureArray.depth);
    }

    /// <summary>
    /// 타일맵 데이터 초기화
    /// </summary>
    public void InitializeTileMap()
    {
        CheckTileMaterial();

        if(mapData != null)
        {
            int[,] mapDataArray = mapData.GetTileData();
            int sizex = mapData.width;
            int sizey = mapData.height;
            int bufferSize2 = sizex * sizey + 3; // 헤더 3개 + 타일 데이터
            _tileMapData = new int[bufferSize2];

            // 헤더 정보 (gridSizeX, gridSizeY, 전체 버퍼 크기)
            _tileMapData[0] = sizex;
            _tileMapData[1] = sizey;
            _tileMapData[2] = bufferSize2 - 3; // 실제 데이터 크기

            for(int j = 0; j < sizey; j++)
            {
                for(int i = 0;  i < sizex; i++)
                {
                    _tileMapData[3 + (j * sizex) + i] = mapData.tiles[(j * sizex) + i];
                }
            }

            _tileMapBuffer = new(GraphicsBuffer.Target.Structured, bufferSize2, sizeof(int));
            _tileMapBuffer.SetData(_tileMapData);
            _tileMaterial.SetBuffer("_TileMapBuffer", _tileMapBuffer);

            return;
        }

        int bufferSize = TileSizeX * TileSizeY + 3; // 헤더 3개 + 타일 데이터
        _tileMapData = new int[bufferSize];

        // 헤더 정보 (gridSizeX, gridSizeY, 전체 버퍼 크기)
        _tileMapData[0] = TileSizeX;
        _tileMapData[1] = TileSizeY;
        _tileMapData[2] = bufferSize - 3; // 실제 데이터 크기

        // 랜덤한 타일맵 데이터 생성 (예제용)
        for (int i = 3; i < bufferSize; i++)
        {
            _tileMapData[i] = Random.Range(0, _tileTextureArray.depth); // 타일 인덱스 랜덤 할당
        }

        _tileMapBuffer = new(GraphicsBuffer.Target.Structured, bufferSize, sizeof(int));
        _tileMapBuffer.SetData(_tileMapData);
        _tileMaterial.SetBuffer("_TileMapBuffer", _tileMapBuffer);
    }
}

using UnityEngine;
using System.Collections.Generic;
using TreeEditor;
using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.UI;

public class GuideTileMapController : MonoBehaviour
{
    // TileMapData;
    private TileMapData _mapData;
    private TileMapData _fovMapData;
    private Material _tileMaterial;

    [Header("Texture Settings")]
    public Texture2D DefaultTexture;
    public Texture2D[] TileTexture;
    private Texture2DArray _tileTextureArray;


    private Image _image;
    private RectTransform _rectTransform;

    private float _lastScaleX;
    private float _lastScaleY;

    void Start()
    {
        Init();
    }

    void Init()
    {
        _mapData = MapManager.Instance.MapData;
        _fovMapData = MapManager.Instance.FOVMapData;
        _lastScaleX = 0;
        _lastScaleY = 0;
        SetMaterialData();
    }

    public void SetMaterialData()
    {
        SetCompoent();
        CheckTileMaterial();
        CreateTexture2DArray();
        InitializeTileMap();
        _tileMaterial.SetFloat("_TileSize", MapManager.Instance.TileSize);
        _tileMaterial.SetFloat("_GuideTileSize", MapManager.Instance.GuideTileSize);
        _image.material = _tileMaterial;
    }

    public void SetCompoent()
    {
        _image = GetComponent<Image>();
        _rectTransform = GetComponent<RectTransform>();
    }

    private void CheckTileMaterial()
    {
        if (_tileMaterial == null)
        {
            _tileMaterial = _image.material;
        }
    }

    public void CreateTexture2DArray()
    {
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

    public void InitializeTileMap()
    {
        _tileMaterial.SetBuffer("_MapDataBuffer", MapManager.Instance.MapDataBuffer);
        _tileMaterial.SetBuffer("_BlurMapDataBufferRow", MapManager.Instance.VisitedMapDataBufferRow);
        _tileMaterial.SetBuffer("_BlurMapDataBufferColumn", MapManager.Instance.VisitedMapDataBufferColumn);
    }

    public void FixedUpdate()
    {
        float scaleX = _rectTransform.localScale.x;
        float scaleY = _rectTransform.localScale.y;
        float sizeX = _rectTransform.sizeDelta.x * 0.01f;
        float sizeY = _rectTransform.sizeDelta.y * 0.01f;

        float x = scaleX * sizeX;
        float y = scaleY * sizeY;

        if (x != _lastScaleX)
            _image.material.SetFloat("_ScaleX", x);
        if(y != _lastScaleY)
            _image.material.SetFloat("_ScaleY", y);

        _lastScaleX = x;
        _lastScaleY = y;
    }
}

using UnityEngine;
using System.Collections.Generic;
using TreeEditor;
using System;

public class TileMapController : MonoBehaviour
{
    private Material _tileMaterial;

    [Header("Texture Settings")]
    public Texture2D DefaultTexture;
    public Texture2D[] TileTexture;
    private Texture2DArray _tileTextureArray;


    void Start()
    {
        Init();
    }

    void Init()
    {
        SetMaterialData();
        MapManager.Instance.AddFOVDataChangeAction(SetBlurMap);
    }

    public void SetMaterialData()
    {
        CheckTileMaterial();
        CreateTexture2DArray();
        InitializeTileMap();
        _tileMaterial.SetFloat("_TileSize", MapManager.Instance.TileSize);
    }

    private void CheckTileMaterial()
    {
        if (_tileMaterial == null)
        {
            SpriteRenderer rend = GetComponent<SpriteRenderer>();
            if (rend != null) _tileMaterial = rend.sharedMaterial;
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

        TileTexture = null;
    }

    public void InitializeTileMap()
    {
        _tileMaterial.SetBuffer("_MapDataBuffer", MapManager.Instance.MapDataBuffer);
        _tileMaterial.SetBuffer("_BlurMapDataBuffer", MapManager.Instance.FOVDataBuffer);
    }

    public void SetBlurMap()
    {
        _tileMaterial.SetBuffer("_BlurMapDataBuffer", MapManager.Instance.FOVDataBuffer);
    }
}

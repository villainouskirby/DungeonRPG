using UnityEngine;

public class TileMapController : MonoBehaviour
{
    private Material _tileMaterial;

    [Header("Texture Settings")]
    public Texture2D DefaultTexture;
    public Sprite[] TileSprites; // Texture2D 대신 Sprite 배열 사용
    private Texture2DArray _tileTextureArray;

    void Start()
    {
        Init();
    }

    void Init()
    {
        SetMaterialData();
        MapManager.Instance.FOVCaster.AddBufferChangeEndAction(SetBlurMap);
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
        if (TileSprites.Length == 0)
        {
            Debug.LogError("[TileMapController] TileSprites 배열이 비어 있습니다.");
            return;
        }

        int width = (int)TileSprites[0].rect.width;
        int height = (int)TileSprites[0].rect.height;
        TextureFormat format = TileSprites[0].texture.format;
        bool mipChain = false;

        _tileTextureArray = new(width, height, TileSprites.Length, format, mipChain);
        _tileTextureArray.anisoLevel = 1;

        for (int i = 0; i < TileSprites.Length; i++)
        {
            Texture2D tex = SpriteToTexture2D(TileSprites[i]);

            if (tex == null)
            {
                Debug.LogWarning($"[TileMapController] TileSprites[{i}]가 비어 있습니다. 기본 텍스처로 대체됩니다.");
                Graphics.CopyTexture(DefaultTexture, 0, 0, _tileTextureArray, i, 0);
                continue;
            }

            Graphics.CopyTexture(tex, 0, 0, _tileTextureArray, i, 0);
        }

        _tileMaterial.SetTexture("_TileTexture", _tileTextureArray);
        _tileMaterial.SetInt("_TextureSize", _tileTextureArray.depth);
    }

    /// <summary>
    /// Sprite를 Texture2D로 변환하는 함수
    /// </summary>
    private Texture2D SpriteToTexture2D(Sprite sprite)
    {
        if (sprite == null) return DefaultTexture;

        Rect rect = sprite.rect;
        Texture2D sourceTex = sprite.texture;

        Texture2D newTex = new Texture2D((int)rect.width, (int)rect.height, sourceTex.format, false);
        newTex.SetPixels(sourceTex.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height));
        newTex.Apply();

        return newTex;
    }

    public void InitializeTileMap()
    {
        _tileMaterial.SetBuffer("_MapDataBuffer", MapManager.Instance.MapDataBuffer);
        _tileMaterial.SetBuffer("_BlurMapDataBuffer", MapManager.Instance.FOVCaster.FOVDataBuffer);
    }

    public void SetBlurMap()
    {
        _tileMaterial.SetBuffer("_BlurMapDataBuffer", MapManager.Instance.FOVCaster.FOVDataBuffer);
    }
}

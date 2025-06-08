using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using System.IO;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using System.Net.NetworkInformation;

public class ResourceSpriteAtlasGenerator : MonoBehaviour
{
    public static string FolderPath = "Assets/TileMapAtals/Resource";
    public static string SpriteFileName = "Sprite";
    public static string AssetName = "ResourceNodeAtlas.spriteatlas";
    public static string LocalSpriteFilePath = "TileMapAtals/Resource/Sprite";

    void Start()
    {
        string assetPath = Path.Combine(FolderPath, AssetName);
        string spritePath = Path.Combine(FolderPath, SpriteFileName);
        Debug.Log(assetPath);
        Debug.Log(spritePath);
        CreateAtlas(ResourceSpriteAtlasManager.Instance.SpriteInfo, assetPath, spritePath);
        RegisterAddressable("Resource", "ResourceNodeAtlas", assetPath);
    }

    public void CreateAtlas(List<SpriteInfo> spriteInfo, string assetPath, string spritePath)
    {
        if (!AssetDatabase.IsValidFolder(FolderPath))
            AssetDatabase.CreateFolder("Assets/TileMapAtals", "Resource");
        if (!AssetDatabase.IsValidFolder(spritePath))
            AssetDatabase.CreateFolder("Assets/TileMapAtals/Resource", "Sprite");

        var atlas = new SpriteAtlas();
        var ti = atlas.GetTextureSettings();
        ti.generateMipMaps = false;
        ti.filterMode = FilterMode.Point;
        ti.readable = true;
        atlas.SetTextureSettings(ti);

        var pp = atlas.GetPackingSettings();
        pp.enableRotation = false;
        pp.enableTightPacking = false;
        atlas.SetPackingSettings(pp);

        SetSpriteForRead(spriteInfo);
        
        foreach (var info in spriteInfo)
        {
            SpriteAtlasExtensions.Add(atlas, new[] { CloneSprite(info.Sprite, info.Name, spritePath) });
        }

        // 5. 에셋으로 저장
        AssetDatabase.DeleteAsset(assetPath);
        AssetDatabase.CreateAsset(atlas, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"ResourceSpriteAtlasGenerator : SpriteAtlas 에셋 생성 완료: {assetPath}");
    }

    public void RegisterAddressable(string groupName, string keyName, string assetPath)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;

        var group = settings.FindGroup(groupName);

        if (group == null)
        {
            group = settings.CreateGroup(
            groupName,
            false,
            false,
            false,
            new List<AddressableAssetGroupSchema>(),
            new[] { typeof(BundledAssetGroupSchema) }
        );
        }

        string guid = AssetDatabase.AssetPathToGUID(assetPath);

        settings.RemoveAssetEntry(guid);
        var entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: true);
        entry.address = keyName;

        settings.SetDirty(
            AddressableAssetSettings.ModificationEvent.EntryMoved,
            entry,
            true
        );

        AssetDatabase.SaveAssets();
        Debug.Log($"ResourceSpriteGenerator : {groupName} 그룹에 Addressable로 등록 완료");
    }

    public void SetSpriteForRead(List<SpriteInfo> spriteInfo)
    {
        HashSet<Texture2D> texture2Ds = new();

        for (int i = 0; i < spriteInfo.Count; i++)
        {
            texture2Ds.Add(spriteInfo[i].Sprite.texture);
        }

        foreach(Texture2D texture2D in texture2Ds)
        {
            string path = AssetDatabase.GetAssetPath(texture2D);
            TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(path);
            if (ti != null)
            {
                ti.isReadable = true;
                ti.SaveAndReimport();
            }
        }
    }

    public Sprite CloneSprite(Sprite source, string newName, string spritePath)
    {
        string assetPath = $"{spritePath}/{newName}.png";

        Texture2D tex;
        int w;
        int h;
        try
        {
            Rect r = source.rect;
            Texture2D origTex = source.texture;
            w = (int)r.width;
            h = (int)r.height;

            tex = new(w, h, TextureFormat.RGBA32, false);
            Color[] pixels = origTex.GetPixels((int)r.x, (int)r.y, w, h);
            tex.SetPixels(pixels);
            tex.Apply();
        }
        catch
        {
            Rect texRect = source.textureRect;
            int x = Mathf.FloorToInt(texRect.x);
            int y = Mathf.FloorToInt(texRect.y);
            w = Mathf.FloorToInt(texRect.width);
            h = Mathf.FloorToInt(texRect.height);

            Texture2D atlasTex = source.texture;
            tex = new(w, h, TextureFormat.RGBA32, false);
            Color[] pixels = atlasTex.GetPixels(x, y, w, h);
            tex.SetPixels(pixels);
            tex.Apply();
        }

        tex.name = newName + "_Tex";
        Sprite newSprite = Sprite.Create(
            tex,
            new(0, 0, w, h),
            new(0.5f, 0.5f),
            source.pixelsPerUnit,
            0,
            SpriteMeshType.FullRect
        );
        newSprite.name = newName;

        File.WriteAllBytes(Path.Combine(Application.dataPath, LocalSpriteFilePath, $"{newName}.png"), tex.EncodeToPNG());
        AssetDatabase.ImportAsset(assetPath);

        var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        TextureImporterSettings settings = new();
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.filterMode = FilterMode.Point;
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteExtrude = 0;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }
}

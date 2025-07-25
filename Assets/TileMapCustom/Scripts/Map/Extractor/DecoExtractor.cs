using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor.U2D;
using UnityEngine.U2D;
using UnityEditor;
using System.Linq;
using System.IO;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Net;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

public class DecoExtractor : MonoBehaviour, IExtractor
{
    public string DataPath;

    private int _chunkSize;
    private HashSet<Sprite> _decoSprite;


    public void Extract(MapEnum mapType, TileMapData mapData)
    {
        DataPath = $"{Application.dataPath}/TileMapAtals/";
        _chunkSize = mapData.All.ChunkSize;
        _decoSprite = new();
        ExtractTilemap2Deco(mapData);

        string assetPath = $"Assets/TileMapAtals/{mapType.ToString()}/{mapType}DecoAtlas.spriteatlas";

        CreateAtlas(mapType, assetPath);
        RegisterAddressable("DecoAtlas", $"{mapType.ToString()}_DecoAtlas", assetPath);
    }

    public void ExtractTilemap2Deco(TileMapData mapData)
    {
        mapData.All.decoObjData = new();

        Transform[] childs = transform.GetComponentsInChildren<Transform>();

        for(int i = 0; i < childs.Length; i++)
        {
            if (childs[i].name == transform.name)
                continue;

            DecoObjData objData = Object2DecoObjData(childs[i].gameObject);

            Vector2Int chunkIndex = GetObjChunkIndex(objData.Position);

            if (!mapData.All.decoObjData.ContainsKey(chunkIndex))
                mapData.All.decoObjData[chunkIndex] = new();

            mapData.All.decoObjData[chunkIndex].Add(objData);
        }
    }

    public DecoObjData Object2DecoObjData(GameObject obj)
    {
        DecoObjData result = new();
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

        result.Name = obj.name;
        result.Position = obj.transform.position;
        result.Position = ExtractorMaster.Instance.CorrectPos(result.Position);
        result.Rotation = obj.transform.rotation;
        result.Scale = obj.transform.localScale;
        result.Color = sr.color;
        _decoSprite.Add(sr.sprite);
        result.SpriteName = sr.sprite.name;
        result.LayerName = sr.sortingLayerName;
        result.LayerIndex = sr.sortingOrder;
        result.ColliderData = GetColliderData(obj);
        
        return result;
    }

    public Vector2Int GetObjChunkIndex(Vector3 pos)
    {
        return new((int)(pos.x / _chunkSize), (int)(pos.y / _chunkSize));
    }

    public ColliderData GetColliderData(GameObject obj)
    {
        ColliderData result = new();
        if (!obj.TryGetComponent<Collider2D>(out var collider))
        {
            result.Type = ColliderType.None;
            return result;
        }

        result.Offset = collider.offset;
        result.IsTrigger = collider.isTrigger;

        switch (collider)
        {
            case BoxCollider2D box:
                result.Type = ColliderType.Box;
                result.Size = box.size;
                break;

            case CircleCollider2D circle:
                result.Type = ColliderType.Circle;
                result.Radius = circle.radius;
                break;

            case PolygonCollider2D poly:
                result.Type = ColliderType.Poly;
                result.Points = poly.points;
                break;

            default:
                Debug.LogWarning($"DecoExtractor : {collider.GetType()}은 구현되어있지 않습니다. 김도현한테 연락 바랍니다.");
                return null;
        }

        return result;
    }

    public void CreateAtlas(MapEnum mapType, string assetPath)
    {
        string folder = $"{DataPath}{mapType.ToString()}/";

        if (Directory.Exists(folder))
            Directory.Delete(folder, true);
        Directory.CreateDirectory(folder);


        SpriteAtlas atlas = new();

        SpriteAtlasTextureSettings textureSettings = new();
        textureSettings.filterMode = FilterMode.Point;

        atlas.SetTextureSettings(textureSettings);

        SerializedObject so = new(atlas);

        SpriteAtlasExtensions.Add(atlas, _decoSprite.ToArray());

        AssetDatabase.CreateAsset(atlas, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"DecoExtractor : SpriteAtlas 에셋 생성 완료: {assetPath}");
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
        Debug.Log($"DecoExtractor : {groupName} 그룹에 Addressable로 등록 완료");
    }
}

[System.Serializable]
public class DecoObjData
{
    public string Name;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public Color Color;
    public string SpriteName;
    public string LayerName;
    public int LayerIndex;
    public ColliderData ColliderData;

    public DecoObjData()
    {
    }
}

[System.Serializable]
public class ColliderData
{
    public ColliderType Type;

    public Vector2 Offset;      // Collider2D
    public Vector2 Size;        // BoxCollider2D
    public float Radius;        // CircleCollider2D
    public Vector2[] Points;    // PolygonCollider2D
    public bool IsTrigger;

    public ColliderData()
    {

    }
}

public enum ColliderType { None, Box, Circle, Poly }
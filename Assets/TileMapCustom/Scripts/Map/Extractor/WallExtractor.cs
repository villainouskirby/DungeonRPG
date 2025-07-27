using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;
using EM = ExtractorMaster;

public class Wallxtractor : MonoBehaviour, IExtractorLate
{
    public string DataPath;

    private CompositeCollider2D _composite;

    public void Extract(MapEnum mapType, TileMapData mapData)
    {
        _composite = GetComponent<CompositeCollider2D>();

        DataPath = $"{Application.dataPath}/MapMeshData/WallMesh/";
        string folder = $"{DataPath}{mapType.ToString()}/";
        if (Directory.Exists(folder))
            Directory.Delete(folder, true);
        Directory.CreateDirectory(folder);

        StartCoroutine(ExtractTilemap2WallMesh(mapType, mapData));
    }

    private IEnumerator ExtractTilemap2WallMesh(MapEnum mapType, TileMapData mapData)
    {
        bool[,] chunkData = new bool[16, 16];
        string groupName = $"{mapType.ToString()}_WallMesh";

        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var group = settings.FindGroup(groupName);
        if (group != null)
            settings.RemoveGroup(group);
        group = settings.CreateGroup(
            groupName,
            false,
            false,
            false,
            new List<AddressableAssetGroupSchema>(),
            new[] { typeof(BundledAssetGroupSchema) }
        );


        for (int w = 0; w < mapData.All.Width; w++)
        {
            for (int h = 0; h < mapData.All.Height; h++)
            {
                int chunkStartIndex = w + h * mapData.All.Width;
                int localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;

                for (int x = 0; x < EM.ChunkSize; x++)
                {
                    for (int y = 0; y < EM.ChunkSize; y++)
                    {
                        int index = x + y * EM.ChunkSize + localStartIndex;
                        bool isWall = false;

                        for (int i = 0; i < mapData.All.LayerCount; i++)
                        {
                            if (EM.Instance.WallSpriteIndex.Contains(mapData.LayerData[i].Tile[index]))
                            {
                                isWall = true;
                                break;
                            }
                        }

                        chunkData[x, y] = isWall;
                    }
                }

                GenerateFromGrid(chunkData);
                yield return StartCoroutine(MakeChunkWall(mapType, w, h));
                DeleteAllChild(transform);
            }
        }

        yield return null;

        for (int w = 0; w < mapData.All.Width; w++)
        {
            for (int h = 0; h < mapData.All.Height; h++)
            {
                string assetPath = $"Assets/MapMeshData/WallMesh/{mapType.ToString()}/WallMesh_{w}_{h}.bytes";
                RegisterAddressable(group, $"{mapType.ToString()}_WallMesh_{w}_{h}", assetPath);
            }
        }

        AssetDatabase.SaveAssets();
    }

    private IEnumerator MakeChunkWall(MapEnum mapType, int w, int h)
    {
        yield return null;
        _composite.GenerateGeometry();
        PolygonColliderData data = new(_composite);
        JJSave.ASave(data, $"WallMesh_{w}_{h}", $"MapMeshData/WallMesh/{mapType.ToString()}/", false);
        yield return null;
    }

    private void DeleteAllChild(Transform parent)
    {
        BoxCollider2D[] childs = parent.GetComponentsInChildren<BoxCollider2D>();

        for (int i = 0; i < childs.Length; i++)
            Destroy(childs[i].gameObject);
    }

    private void GenerateFromGrid(bool[,] grid)
    {
        for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
            {
                if (!grid[x, y]) continue;
                var go = new GameObject($"Tile");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(x + 0.5f, y + 0.5f, 0);
                var bc = go.AddComponent<BoxCollider2D>();
                bc.size = Vector2.one;
                bc.usedByComposite = true;
            }
    }

    private void RegisterAddressable(AddressableAssetGroup group, string keyName, string assetPath)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;

        string guid = AssetDatabase.AssetPathToGUID(assetPath);

        var entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: true);
        entry.address = keyName;

        settings.SetDirty(
            AddressableAssetSettings.ModificationEvent.EntryMoved,
            entry,
            true
        );

        AssetDatabase.SaveAssets();
    }
}

public class PolygonColliderData
{
    public int PathCount;
    public List<Vector2[]> Loops;

    public PolygonColliderData(CompositeCollider2D collider)
    {
        PathCount = collider.pathCount;
        Loops = new();
        for (int i = 0; i < collider.pathCount; i++)
        {
            List<Vector2> pts = new List<Vector2>();
            collider.GetPath(i, pts);

            if (pts.Count >= 3)
                Loops.Add(pts.ToArray());
        }
    }

    public PolygonColliderData()
    {
        PathCount = 0;
        Loops = new();
    }
}

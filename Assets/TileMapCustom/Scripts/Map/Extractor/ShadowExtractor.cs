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
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using static UnityEditor.Experimental.GraphView.GraphView;
using EM = ExtractorMaster;

public class ShadowExtractor : MonoBehaviour, IExtractorLate
{
    public string DataPath;

    private CompositeCollider2D _composite;

    private MethodInfo _genShadowMesh;
    public void Extract(MapEnum mapType, TileMapData mapData)
    {
        _composite = GetComponent<CompositeCollider2D>();

        DataPath = $"{Application.dataPath}/MapMeshData/ChunkShadowMesh/";
        string folder = $"{DataPath}{mapType.ToString()}/";
        if (Directory.Exists(folder))
            Directory.Delete(folder, true);
        Directory.CreateDirectory(folder);

        var utilType = typeof(ShadowCaster2D).Assembly
            .GetType("UnityEngine.Rendering.Universal.ShadowUtility");
        _genShadowMesh = utilType?
            .GetMethod("GenerateShadowMesh",
                       BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                       null, new[] { typeof(Mesh), typeof(Vector3[]) }, null);

        StartCoroutine(ExtractTilemap2ShadowMesh(mapType , mapData));
    }

    private IEnumerator ExtractTilemap2ShadowMesh(MapEnum mapType, TileMapData mapData)
    {
        bool[,] chunkData = new bool[16, 16];
        string groupName = $"{mapType.ToString()}_ChunkShadowMesh";

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

        int childCount = EM.Instance.LayerRoot.transform.childCount;
        List<Tilemap> tilemap = new();
        for (int i = 0; i < childCount; i++)
            if (EM.Instance.LayerRoot.transform.GetChild(i).gameObject.activeSelf && EM.Instance.LayerRoot.transform.GetChild(i).TryGetComponent(out Tilemap layerMap))
                tilemap.Add(layerMap);

        for (int i = 0; i < tilemap.Count; i++)
        {
            AssetDatabase.CreateFolder($"Assets/MapMeshData/ChunkShadowMesh/{mapType.ToString()}", $"layer{i}");
            yield return null;
            Tilemap wallMap = null;
            for (int j = 0; j < tilemap[i].transform.childCount; j++)
            {
                if (!tilemap[i].transform.GetChild(j).gameObject.activeSelf)
                    continue;
                string type = tilemap[i].transform.GetChild(j).name.Split("_")[1];

                switch (type)
                {
                    case "Shadow":
                        wallMap = tilemap[i].transform.GetChild(j).GetComponent<Tilemap>();
                        break;
                }
            }

            bool[] allMap = new bool[mapData.All.Width * mapData.All.Height * EM.ChunkSize * EM.ChunkSize];
            Array.Fill(allMap, false);

            if (wallMap != null)
            {
                BoundsInt bounds = wallMap.cellBounds;
                Vector3Int startPos = bounds.position;
                TileBase[] tiles = wallMap.GetTilesBlock(bounds);
                for (int y = 0; y < bounds.size.y; y++)
                {
                    for (int x = 0; x < bounds.size.x; x++)
                    {
                        int correctX = x + startPos.x - EM.Instance.StartPos.x;
                        int correctY = y + startPos.y - EM.Instance.StartPos.y;

                        Vector2Int chunkIndex = new(correctX / EM.ChunkSize, correctY / EM.ChunkSize);
                        Vector2Int localIndex = new(correctX % EM.ChunkSize, correctY % EM.ChunkSize);

                        int chunkStartIndex = chunkIndex.x + chunkIndex.y * mapData.All.Width;
                        int localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;
                        int index = localIndex.x + localIndex.y * EM.ChunkSize + localStartIndex;

                        TileBase tileBase = tiles[x + y * bounds.size.x];
                        Sprite shadow = null;
                        if (tileBase is Tile tile)
                            shadow = tile.sprite;

                        if (shadow != null)
                            allMap[index] = true;
                    }
                }
            }


            for (int w = 0; w < mapData.All.Width; w++)
            {
                for (int h = 0; h < mapData.All.Height; h++)
                {
                    if (wallMap == null)
                    {
                        for (int x = 0; x < EM.ChunkSize; x++)
                        {
                            for (int y = 0; y < EM.ChunkSize; y++)
                            {
                                chunkData[x, y] = false;
                            }
                        }
                    }
                    else
                    {
                        int chunkStartIndex = w + h * mapData.All.Width;
                        int localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;

                        for (int x = 0; x < EM.ChunkSize; x++)
                        {
                            for (int y = 0; y < EM.ChunkSize; y++)
                            {

                                int index = x + y * EM.ChunkSize + localStartIndex;
                                chunkData[x, y] = allMap[index];
                            }
                        }
                    }

                    GenerateFromGrid(chunkData);
                    yield return StartCoroutine(MakeChunkShadow(mapType, w, h, i));
                    DeleteAllChild(transform);
                }
            }

            yield return new WaitForSeconds(0.1f);

            for (int w = 0; w < mapData.All.Width; w++)
            {
                for (int h = 0; h < mapData.All.Height; h++)
                {
                    string assetPath = $"Assets/MapMeshData/ChunkShadowMesh/{mapType.ToString()}/layer{i}/layer{i}_ChunkShadowMesh_{w}_{h}.asset";
                    RegisterAddressable(group, $"{mapType.ToString()}_ChunkShadowMesh_{w}_{h}", assetPath);
                }
            }
        }

        AssetDatabase.SaveAssets();
    }

    private IEnumerator MakeChunkShadow(MapEnum mapType, int w, int h, int layer)
    {
        yield return null;
        _composite.GenerateGeometry();
        Mesh chunkShadowMesh = ApplyCompositeShadow();

        AssetDatabase.CreateAsset(chunkShadowMesh, $"Assets/MapMeshData/ChunkShadowMesh/{mapType.ToString()}/layer{layer}/layer{layer}_chunkShadowMesh_{w}_{h}.asset");
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
    /*
    private void TestGenerateFromGrid(bool[,] grid, int w, int h)
    {
        var b = GameObject.Find("ChunkRoot");
        if(b == null)
            b = new GameObject($"ChunkRoot");
        var a = new GameObject($"chunk_{w}_{h}");
        a.transform.SetParent(b.transform, false);
        a.transform.position = new Vector3(w * 16, h * 16, 0);

        for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
            {
                if (!grid[x, y]) continue;
                var go = new GameObject($"Tile");
                go.transform.SetParent(a.transform, false);
                go.transform.localPosition = new Vector3(x + 0.5f, y + 0.5f, 0);
                var bc = go.AddComponent<BoxCollider2D>();
                bc.size = Vector2.one;
                bc.usedByComposite = true;
            }
    }
    */

    private Mesh ApplyCompositeShadow()
    {
        List<Vector2[]> loops = new();
        for (int i = 0; i < _composite.pathCount; i++)
        {
            List<Vector2> pts = new();
            _composite.GetPath(i, pts);
            if (pts.Count >= 3)
                loops.Add(pts.ToArray());
        }
        if (loops.Count == 0) return new();

        List<Mesh> parts = new(loops.Count);

        foreach (Vector2[] shape in loops)
        {
            Mesh mesh = new();
            Vector3[] path3D = Array.ConvertAll(shape, v => (Vector3)v);
            _genShadowMesh.Invoke(null, new object[] { mesh, path3D });
            parts.Add(mesh);
        }

        CombineInstance[] combines = new CombineInstance[parts.Count];
        for (int i = 0; i < parts.Count; i++)
        {
            combines[i].mesh = parts[i];
            combines[i].transform = Matrix4x4.identity;
        }
        Mesh combined = new();
        combined.CombineMeshes(combines, mergeSubMeshes: true, useMatrices: false);
        combined.indexFormat = IndexFormat.UInt16;

        return combined;
    }


    private void RegisterAddressable(AddressableAssetGroup group, string keyName, string assetPath)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;

        string guid = AssetDatabase.AssetPathToGUID(assetPath);

        var entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: true);
        Debug.Log(keyName);
        entry.address = keyName;

        settings.SetDirty(
            AddressableAssetSettings.ModificationEvent.EntryMoved,
            entry,
            true
        );

        AssetDatabase.SaveAssets();
    }
}

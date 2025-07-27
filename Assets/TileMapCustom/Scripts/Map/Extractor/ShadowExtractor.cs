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


        for (int w = 0; w < mapData.All.Width; w++)
        {
            for(int h = 0; h < mapData.All.Height; h++)
            {
                int chunkStartIndex = w + h * mapData.All.Width;
                int localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;

                for(int x = 0; x < EM.ChunkSize; x++)
                {
                    for(int y = 0; y < EM.ChunkSize; y++)
                    {
                        int index = x + y * EM.ChunkSize + localStartIndex;
                        bool isShadow = false;

                        for (int i = 0; i < mapData.All.LayerCount; i++)
                        {
                            if(EM.Instance.ShadowSpriteIndex.Contains(mapData.LayerData[i].Tile[index]))
                            {
                                isShadow = true;
                                break;
                            }
                        }

                        chunkData[x, y] = isShadow;
                    }
                }

                GenerateFromGrid(chunkData);
                yield return StartCoroutine(MakeChunkShadow(mapType, w, h));
                DeleteAllChild(transform);
                string assetPath = $"Assets/MapMeshData/ChunkShadowMesh/{mapType.ToString()}/ChunkShadowMesh_{w}_{h}.asset";
                RegisterAddressable(group, $"{mapType.ToString()}_ChunkShadowMesh_{w}_{h}", assetPath);
            }
        }

        AssetDatabase.SaveAssets();
    }

    private IEnumerator MakeChunkShadow(MapEnum mapType, int w, int h)
    {
        yield return null;
        _composite.GenerateGeometry();
        Mesh chunkShadowMesh = ApplyCompositeShadow();

        AssetDatabase.CreateAsset(chunkShadowMesh, $"Assets/MapMeshData/ChunkShadowMesh/{mapType.ToString()}/chunkShadowMesh_{w}_{h}.asset");
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
        entry.address = keyName;

        settings.SetDirty(
            AddressableAssetSettings.ModificationEvent.EntryMoved,
            entry,
            true
        );

        AssetDatabase.SaveAssets();
    }
}

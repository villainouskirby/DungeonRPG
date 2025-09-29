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

        int childCount = EM.Instance.LayerRoot.transform.childCount;
        List<Tilemap> tilemap = new();
        for (int i = 0; i < childCount; i++)
            if (EM.Instance.LayerRoot.transform.GetChild(i).gameObject.activeSelf && EM.Instance.LayerRoot.transform.GetChild(i).TryGetComponent(out Tilemap layerMap))
                tilemap.Add(layerMap);

        for (int i = 0; i < tilemap.Count; i++)
        {
            Tilemap wallMap = null;
            for (int j = 0; j < tilemap[i].transform.childCount; j++)
            {
                if (!tilemap[i].transform.GetChild(j).gameObject.activeSelf)
                    continue;
                string type = tilemap[i].transform.GetChild(j).name.Split("_")[1];

                switch (type)
                {
                    case "Wall":
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
                        Sprite wall = null;
                        if (tileBase is Tile tile)
                            wall = tile.sprite;

                        if (wall != null)
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
                    yield return StartCoroutine(MakeChunkWall(mapType, w, h, i));
                    DeleteAllChild(transform);
                }
            }

            yield return new WaitForSeconds(0.1f);

            for (int w = 0; w < mapData.All.Width; w++)
            {
                for (int h = 0; h < mapData.All.Height; h++)
                {
                    string assetPath = $"Assets/MapMeshData/WallMesh/{mapType.ToString()}/layer{i}/layer{i}_WallMesh_{w}_{h}.bytes";
                    RegisterAddressable(group, $"{mapType.ToString()}_layer{i}_WallMesh_{w}_{h}", assetPath);
                }
            }
        }

        AssetDatabase.SaveAssets();
    }

    private IEnumerator MakeChunkWall(MapEnum mapType, int w, int h, int layer)
    {
        yield return null;
        _composite.GenerateGeometry();
        PolygonColliderData data = new(_composite);
        JJSave.ASave(data, $"layer{layer}_WallMesh_{w}_{h}", $"MapMeshData/WallMesh/{mapType.ToString()}/layer{layer}/", false);
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
        Debug.Log(keyName);
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

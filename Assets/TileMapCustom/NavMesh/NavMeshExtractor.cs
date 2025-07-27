using NavMeshPlus.Components;
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
using NavMeshSurface = NavMeshPlus.Components.NavMeshSurface;

public class NavMeshxtractor : MonoBehaviour, IExtractorLate
{
    public string DataPath;
    public GameObject WallRoot;
    public GameObject GroundRoot;

    private CompositeCollider2D _groundComposite;
    private CompositeCollider2D _wallComposite;

    public void Extract(MapEnum mapType, TileMapData mapData)
    {
        _groundComposite = GroundRoot.GetComponent<CompositeCollider2D>();
        _wallComposite = WallRoot.GetComponent<CompositeCollider2D>();

        DataPath = $"{Application.dataPath}/MapMeshData/ChunkNavMesh/";
        string folder = $"{DataPath}{mapType.ToString()}/";
        if (Directory.Exists(folder))
            Directory.Delete(folder, true);
        Directory.CreateDirectory(folder);

        StartCoroutine(ExtractTilemap2ChunkNavMesh(mapType, mapData));
    }

    private enum TileType
    {
        None = 0,
        Wall = 1,
        Ground = 2,
    }

    private IEnumerator ExtractTilemap2ChunkNavMesh(MapEnum mapType, TileMapData mapData)
    {
        TileType[,] chunkData = new TileType[16, 16];
        string groupName = $"{mapType.ToString()}_ChunkNavMesh";

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
                        bool isEmpty = true;

                        for (int i = 0; i < mapData.All.LayerCount; i++)
                        {
                            if (EM.Instance.WallSpriteIndex.Contains(mapData.LayerData[i].Tile[index]))
                            {
                                isWall = true;
                                break;
                            }
                            if (mapData.LayerData[i].Tile[index] != 0)
                                isEmpty = false;
                        }

                        if (isWall)
                            chunkData[x, y] = TileType.Wall;
                        else if (isEmpty)
                            chunkData[x, y] = TileType.None;
                        else
                            chunkData[x, y] = TileType.Ground;
                    }
                }

                GenerateFromGrid(chunkData);
                yield return StartCoroutine(MakeChunkNav(mapType, w, h));
                DeleteAllChild(transform);
            }
        }

        yield return null;

        for (int w = 0; w < mapData.All.Width; w++)
        {
            for (int h = 0; h < mapData.All.Height; h++)
            {
                string assetPath = $"Assets/MapMeshData/ChunkNavMesh/{mapType.ToString()}/ChunkNavMesh_{w}_{h}.asset";
                RegisterAddressable(group, $"{mapType.ToString()}_ChunkNavMesh_{w}_{h}", assetPath);
            }
        }

        AssetDatabase.SaveAssets();
    }

    public Mesh CreateMeshFromComposite(CompositeCollider2D composite)
    {
        Mesh mesh = new();
        List<Vector3> vertices = new();
        List<int> triangles = new();

        int pathCount = composite.pathCount;
        int vertexOffset = 0;

        for (int p = 0; p < pathCount; p++)
        {
            int pointCount = composite.GetPathPointCount(p);
            Vector2[] path2D = new Vector2[pointCount];
            composite.GetPath(p, path2D);

            int[] idx = Triangulate(path2D);

            for (int i = 0; i < path2D.Length; i++)
            {
                vertices.Add(new Vector3(path2D[i].x, path2D[i].y, 0f));
            }

            for (int i = 0; i < idx.Length; i += 3)
            {
                triangles.Add(idx[i + 0] + vertexOffset);
                triangles.Add(idx[i + 1] + vertexOffset);
                triangles.Add(idx[i + 2] + vertexOffset);
            }

            vertexOffset += path2D.Length;
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;

        int[] Triangulate(Vector2[] pts)
        {
            List<int> result = new();
            List<int> V = new();
            for (int i = 0; i < pts.Length; i++)
            {
                V.Add(i);
            }

            while (V.Count >= 3)
            {
                for (int i = 0; i < V.Count; i++)
                {
                    int i0 = V[(i + V.Count - 1) % V.Count];
                    int i1 = V[i];
                    int i2 = V[(i + 1) % V.Count];

                    Vector2 a = pts[i0];
                    Vector2 b = pts[i1];
                    Vector2 c = pts[i2];

                    if (IsConvex(a, b, c) && NoPointInTri(a, b, c))
                    {
                        result.Add(i0);
                        result.Add(i1);
                        result.Add(i2);
                        V.RemoveAt(i);
                        break;
                    }
                }
            }

            return result.ToArray();

            bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
            {
                return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) > 0f;
            }

            bool NoPointInTri(Vector2 a, Vector2 b, Vector2 c)
            {
                for (int j = 0; j < V.Count; j++)
                {
                    Vector2 p = pts[V[j]];
                    if (p != a && p != b && p != c && PointInTriangle(p, a, b, c))
                    {
                        return false;
                    }
                }
                return true;
            }

            bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
            {
                Vector2 v0 = c - a;
                Vector2 v1 = b - a;
                Vector2 v2 = p - a;

                float dot00 = Vector2.Dot(v0, v0);
                float dot01 = Vector2.Dot(v0, v1);
                float dot02 = Vector2.Dot(v0, v2);
                float dot11 = Vector2.Dot(v1, v1);
                float dot12 = Vector2.Dot(v1, v2);

                float invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
                float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
                float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

                return (u >= 0f) && (v >= 0f) && (u + v < 1f);
            }
        }
    }

    private IEnumerator MakeChunkNav(MapEnum mapType, int w, int h)
    {
        yield return null;
        _groundComposite.GenerateGeometry();
        _wallComposite.GenerateGeometry();
        Mesh groundMesh = CreateMeshFromComposite(_groundComposite);
        Mesh wallMesh = CreateMeshFromComposite(_wallComposite);

        List<NavMeshBuildSource> sources = new();

        NavMeshBuildSource srcGround = new();
        srcGround.shape = NavMeshBuildSourceShape.Mesh;
        srcGround.sourceObject = groundMesh;
        srcGround.area = 0;
        srcGround.transform = Matrix4x4.TRS(
        new Vector3(w * 16f, h * 16f, 0f),
        Quaternion.identity,
        Vector3.one);

        sources.Add(srcGround);

        NavMeshBuildSource srcWall = new();
        srcWall.shape = NavMeshBuildSourceShape.Mesh;
        srcWall.sourceObject = wallMesh;
        srcWall.area = 1;
        srcGround.transform = Matrix4x4.TRS(
        new Vector3(w * 16f, h * 16f, 0f),
        Quaternion.identity,
        Vector3.one);
        sources.Add(srcWall);

        NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0);

        Bounds bounds = new(
            new(w * 16f + 8f, h * 16f + 8f, 0f),
            new(16f, 16f, 2f)
        );
        Vector3 positionOffset = new(w * 16, h * 16, 0);
        Quaternion rotationOffset = Quaternion.identity;

        NavMeshData navData = NavMeshBuilder.BuildNavMeshData(
            buildSettings,
            sources,
            bounds,
            Vector3.zero,
            rotationOffset
        );

        GameObject visualObj = new("VisualObj");
        var surface = visualObj.AddComponent<NavMeshSurface>();
        surface.navMeshData = navData;
        surface.AddData();
        visualObj.transform.position = positionOffset;

        NavMesh.AddNavMeshData(navData);

        AssetDatabase.CreateAsset(navData, $"Assets/MapMeshData/ChunkNavMesh/{mapType.ToString()}/ChunkNavMesh_{w}_{h}.asset");
        yield return null;
    }

    private void DeleteAllChild(Transform parent)
    {
        BoxCollider2D[] childs = parent.GetComponentsInChildren<BoxCollider2D>();

        for (int i = 0; i < childs.Length; i++)
            Destroy(childs[i].gameObject);
    }

    private void GenerateFromGrid(TileType[,] grid)
    {
        for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
            {
                GameObject go;
                BoxCollider2D bc;

                switch (grid[x, y])
                {
                    case TileType.None:
                        continue;
                    case TileType.Wall:
                        go = new GameObject($"WallTile");
                        go.transform.SetParent(WallRoot.transform, false);
                        go.transform.localPosition = new Vector3(x + 0.5f, y + 0.5f, 0);
                        bc = go.AddComponent<BoxCollider2D>();
                        bc.size = Vector2.one;
                        bc.usedByComposite = true;
                        Debug.Log("벽생성");
                        break;
                    case TileType.Ground:
                        go = new GameObject($"GroundTile");
                        go.transform.SetParent(GroundRoot.transform, false);
                        go.transform.localPosition = new Vector3(x + 0.5f, y + 0.5f, 0);
                        bc = go.AddComponent<BoxCollider2D>();
                        bc.size = Vector2.one;
                        bc.usedByComposite = true;
                        break;
                }
            }

        for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
            {
                GameObject testRoot = new($"Test{x}{y}");
                GameObject wallTest = new("wall");
                wallTest.AddComponent<CompositeCollider2D>();
                wallTest.AddComponent<Rigidbody2D>();
                GameObject groundTest = new("ground");
                groundTest.AddComponent<CompositeCollider2D>();
                groundTest.AddComponent<Rigidbody2D>();
                GameObject go;
                BoxCollider2D bc;

                switch (grid[x, y])
                {
                    case TileType.None:
                        continue;
                    case TileType.Wall:
                        go = new GameObject($"WallTile");
                        go.transform.SetParent(wallTest.transform, false);
                        go.transform.localPosition = new Vector3(x + 0.5f, y + 0.5f, 0);
                        bc = go.AddComponent<BoxCollider2D>();
                        bc.size = Vector2.one;
                        bc.usedByComposite = true;
                        Debug.Log("벽생성");
                        break;
                    case TileType.Ground:
                        go = new GameObject($"GroundTile");
                        go.transform.SetParent(groundTest.transform, false);
                        go.transform.localPosition = new Vector3(x + 0.5f, y + 0.5f, 0);
                        bc = go.AddComponent<BoxCollider2D>();
                        bc.size = Vector2.one;
                        bc.usedByComposite = true;
                        break;
                }
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

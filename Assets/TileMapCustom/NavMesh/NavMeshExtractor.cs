using NavMeshPlus.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AI;
using EM = ExtractorMaster;

public class NavMeshExtractor : MonoBehaviour, IExtractorLate
{
    public string DataPath;
    public GameObject WallRoot;
    public GameObject GroundRoot;

    private CompositeCollider2D _groundComposite;
    private CompositeCollider2D _wallComposite;
    private Dictionary<(Vector2Int c1, Vector2Int c2), bool> _chunkConnect;
    private Dictionary<Vector2Int, List<Vector2Int>> ChunkLinkData;

    public void Extract(MapEnum mapType, TileMapData mapData)
    {
        _groundComposite = GroundRoot.GetComponent<CompositeCollider2D>();
        _wallComposite = WallRoot.GetComponent<CompositeCollider2D>();
        _chunkConnect = new();
        ChunkLinkData = new();

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
        Empty = 3,
    }

    private IEnumerator ExtractTilemap2ChunkNavMesh(MapEnum mapType, TileMapData mapData)
    {
        TileType[,] chunkData = new TileType[18, 18];
        List<Vector2Int> chunkLinkData = new();
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
                for (int i = 0; i < 18; i++)
                    for (int j = 0; j < 18; j++)
                        chunkData[i, j] = TileType.None;
                chunkLinkData.Clear();

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
                            chunkData[x + 1, y + 1] = TileType.Wall;
                        else if (isEmpty)
                            chunkData[x + 1, y + 1] = TileType.Empty;
                        else
                            chunkData[x + 1, y + 1] = TileType.Ground;
                    }
                }

                if (w != 0) // 좌측
                {
                    Vector2Int c1 = new(w, h);
                    Vector2Int c2 = new(w - 1, h);
                    if (!_chunkConnect.ContainsKey((c2, c1)) || !_chunkConnect[(c2, c1)])
                    {
                        _chunkConnect[(c2, c1)] = true;

                        chunkStartIndex = (w - 1) + h * mapData.All.Width;
                        localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;
                        int x = 15;
                        bool isWall;

                        for (int y = 0; y < EM.ChunkSize; y++)
                        {
                            if (chunkData[1, y + 1] == TileType.Ground)
                            {
                                isWall = false;
                                int index = x + y * EM.ChunkSize + localStartIndex;

                                for (int i = 0; i < mapData.All.LayerCount; i++)
                                {
                                    if (EM.Instance.WallSpriteIndex.Contains(mapData.LayerData[i].Tile[index]))
                                    {
                                        isWall = true;
                                        break;
                                    }
                                }

                                if (!isWall)
                                {
                                    chunkData[0, y + 1] = TileType.Ground;
                                    chunkLinkData.Add(new(-1, y));
                                }
                            }
                        }
                    }
                }
                if (h != 0) // 하단
                {
                    Vector2Int c1 = new(w, h);
                    Vector2Int c2 = new(w, h - 1);
                    if (!_chunkConnect.ContainsKey((c2, c1)) || !_chunkConnect[(c2, c1)])
                    {
                        _chunkConnect[(c2, c1)] = true;

                        chunkStartIndex = w + (h - 1) * mapData.All.Width;
                        localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;
                        int y = 15;
                        bool isWall;

                        for (int x = 0; x < EM.ChunkSize; x++)
                        {
                            if (chunkData[x + 1, 1] == TileType.Ground)
                            {
                                isWall = false;
                                int index = x + y * EM.ChunkSize + localStartIndex;

                                for (int i = 0; i < mapData.All.LayerCount; i++)
                                {
                                    if (EM.Instance.WallSpriteIndex.Contains(mapData.LayerData[i].Tile[index]))
                                    {
                                        isWall = true;
                                        break;
                                    }
                                }

                                if (!isWall)
                                {
                                    chunkData[x + 1, 0] = TileType.Ground;
                                    chunkLinkData.Add(new(x, -1));
                                }
                            }
                        }
                    }
                }
                if (w != mapData.All.Width - 1) // 우측
                {
                    Vector2Int c1 = new(w, h);
                    Vector2Int c2 = new(w + 1, h);
                    if (!_chunkConnect.ContainsKey((c1, c2)) || !_chunkConnect[(c1, c2)])
                    {
                        _chunkConnect[(c1, c2)] = true;

                        chunkStartIndex = (w + 1) + h * mapData.All.Width;
                        localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;
                        int x = 0;
                        bool isWall;

                        for (int y = 0; y < EM.ChunkSize; y++)
                        {
                            if (chunkData[16, y + 1] == TileType.Ground)
                            {
                                isWall = false;
                                int index = x + y * EM.ChunkSize + localStartIndex;

                                for (int i = 0; i < mapData.All.LayerCount; i++)
                                {
                                    if (EM.Instance.WallSpriteIndex.Contains(mapData.LayerData[i].Tile[index]))
                                    {
                                        isWall = true;
                                        break;
                                    }
                                }

                                if (!isWall)
                                {
                                    chunkData[17, y + 1] = TileType.Ground;
                                    chunkLinkData.Add(new(16, y));
                                }
                            }
                        }
                    }
                }
                if (h != mapData.All.Height - 1) // 상단
                {
                    Vector2Int c1 = new(w, h);
                    Vector2Int c2 = new(w, h + 1);
                    if (!_chunkConnect.ContainsKey((c1, c2)) || !_chunkConnect[(c1, c2)])
                    {
                        _chunkConnect[(c1, c2)] = true;

                        chunkStartIndex = w + (h + 1) * mapData.All.Width;
                        localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;
                        int y = 0;
                        bool isWall;

                        for (int x = 0; x < EM.ChunkSize; x++)
                        {
                            if (chunkData[x + 1, 16] == TileType.Ground)
                            {
                                isWall = false;
                                int index = x + y * EM.ChunkSize + localStartIndex;

                                for (int i = 0; i < mapData.All.LayerCount; i++)
                                {
                                    if (EM.Instance.WallSpriteIndex.Contains(mapData.LayerData[i].Tile[index]))
                                    {
                                        isWall = true;
                                        break;
                                    }
                                }

                                if (!isWall)
                                {
                                    chunkData[x + 1, 17] = TileType.Ground;
                                    chunkLinkData.Add(new(x, 16));
                                }
                            }
                        }
                    }
                }
                // 좌측 하단
                if (chunkData[0, 1] == TileType.Ground && chunkData[1, 0] == TileType.Ground)
                {
                    chunkStartIndex = (w - 1) + (h - 1) * mapData.All.Width;
                    localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;
                    bool isWall = false;
                    int index = 15 + 15 * EM.ChunkSize + localStartIndex;

                    for (int i = 0; i < mapData.All.LayerCount; i++)
                    {
                        if (EM.Instance.WallSpriteIndex.Contains(mapData.LayerData[i].Tile[index]))
                        {
                            isWall = true;
                            break;
                        }
                    }

                    if (!isWall)
                    {
                        chunkData[0, 0] = TileType.Ground;
                        chunkLinkData.Add(new(-1, -1));
                    }
                }
                // 좌측 상단
                if (chunkData[0, 16] == TileType.Ground && chunkData[1, 17] == TileType.Ground)
                {
                    chunkStartIndex = (w - 1) + (h + 1) * mapData.All.Width;
                    localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;
                    bool isWall = false;
                    int index = 15 + 0 * EM.ChunkSize + localStartIndex;

                    for (int i = 0; i < mapData.All.LayerCount; i++)
                    {
                        if (EM.Instance.WallSpriteIndex.Contains(mapData.LayerData[i].Tile[index]))
                        {
                            isWall = true;
                            break;
                        }
                    }

                    if (!isWall)
                    {
                        chunkData[0, 17] = TileType.Ground;
                        chunkLinkData.Add(new(-1, 16));
                    }
                }
                // 우측 하단
                if (chunkData[17, 1] == TileType.Ground && chunkData[16, 0] == TileType.Ground)
                {
                    chunkStartIndex = (w + 1) + (h - 1) * mapData.All.Width;
                    localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;
                    bool isWall = false;
                    int index = 0 + 15 * EM.ChunkSize + localStartIndex;

                    for (int i = 0; i < mapData.All.LayerCount; i++)
                    {
                        if (EM.Instance.WallSpriteIndex.Contains(mapData.LayerData[i].Tile[index]))
                        {
                            isWall = true;
                            break;
                        }
                    }

                    if (!isWall)
                    {
                        chunkData[17, 0] = TileType.Ground;
                        chunkLinkData.Add(new(16, -1));
                    }
                }
                // 우측 상단
                if (chunkData[17, 16] == TileType.Ground && chunkData[16, 17] == TileType.Ground)
                {
                    chunkStartIndex = (w + 1) + (h + 1) * mapData.All.Width;
                    localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;
                    bool isWall = false;
                    int index = 0 + 0 * EM.ChunkSize + localStartIndex;

                    for (int i = 0; i < mapData.All.LayerCount; i++)
                    {
                        if (EM.Instance.WallSpriteIndex.Contains(mapData.LayerData[i].Tile[index]))
                        {
                            isWall = true;
                            break;
                        }
                    }

                    if (!isWall)
                    {
                        chunkData[17, 17] = TileType.Ground;
                        chunkLinkData.Add(new(16, 16));
                    }
                }

                ChunkLinkData[new(w, h)] = chunkLinkData.ToList();

                GenerateFromGrid(chunkData, w , h);
                yield return StartCoroutine(MakeChunkNav(mapType, w, h));
                DeleteAllChild(transform);
            }
        }

        JJSave.ASave(ChunkLinkData, $"NavChunkLinkData", $"MapMeshData/ChunkNavMesh/{mapType.ToString()}/", false);

        yield return null;

        RegisterAddressable(group, $"{mapType}_NavChunkLinkData", $"Assets/MapMeshData/ChunkNavMesh/{mapType.ToString()}/NavChunkLinkData.bytes");

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

    public Mesh CreateMesh(CompositeCollider2D composite)
    {
        if (composite == null)
        {
            Debug.LogError("CompositeCollider2D is null!");
            return new Mesh();
        }

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        int vertexOffset = 0;

        int pathCount = composite.pathCount;
        for (int p = 0; p < pathCount; p++)
        {
            int pointCount = composite.GetPathPointCount(p);
            if (pointCount < 3) continue;

            Vector2[] pts = new Vector2[pointCount];
            composite.GetPath(p, pts);

            if (SignedArea(pts) < 0f)
                Array.Reverse(pts);

            pts = RemoveDuplicates(pts);
            if (pts.Length < 3) continue;

            int[] idx = Triangulate(pts);
            if (idx.Length < 3) continue;

            for (int i = 0; i < pts.Length; i++)
                vertices.Add(new Vector3(pts[i].x, pts[i].y, 0f));

            for (int i = 0; i < idx.Length; i += 3)
            {
                triangles.Add(vertexOffset + idx[i + 0]);
                triangles.Add(vertexOffset + idx[i + 1]);
                triangles.Add(vertexOffset + idx[i + 2]);
            }

            vertexOffset += pts.Length;
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;


        // ──────────── 지역 함수들 ────────────

        float SignedArea(Vector2[] poly)
        {
            float area = 0f;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                area += (poly[j].x * poly[i].y) - (poly[i].x * poly[j].y);
            }
            return area * 0.5f;
        }

        Vector2[] RemoveDuplicates(Vector2[] input)
        {
            List<Vector2> list = new List<Vector2>();
            for (int i = 0; i < input.Length; i++)
            {
                if (i == 0 || Vector2.Distance(list[list.Count - 1], input[i]) > Mathf.Epsilon)
                    list.Add(input[i]);
            }
            if (list.Count > 1 && Vector2.Distance(list[0], list[list.Count - 1]) < Mathf.Epsilon)
                list.RemoveAt(list.Count - 1);
            return list.ToArray();
        }

        int[] Triangulate(Vector2[] pts)
        {
            List<int> result = new List<int>();
            List<int> V = new List<int>();
            for (int i = 0; i < pts.Length; i++)
                V.Add(i);

            int safety = pts.Length * pts.Length;
            while (V.Count >= 3 && safety-- > 0)
            {
                bool earFound = false;
                for (int vi = 0; vi < V.Count; vi++)
                {
                    int i0 = V[(vi + V.Count - 1) % V.Count];
                    int i1 = V[vi];
                    int i2 = V[(vi + 1) % V.Count];

                    Vector2 a = pts[i0], b = pts[i1], c = pts[i2];
                    if (IsConvex(a, b, c) && NoPointInTri(a, b, c, pts, V))
                    {
                        result.Add(i2);
                        result.Add(i1);
                        result.Add(i0);
                        V.RemoveAt(vi);
                        earFound = true;
                        break;
                    }
                }
                if (!earFound) break;
            }

            return result.ToArray();
        }

        bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
            => ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) > 0f;

        bool NoPointInTri(Vector2 a, Vector2 b, Vector2 c, Vector2[] pts2, List<int> verts)
        {
            for (int j = 0; j < verts.Count; j++)
            {
                Vector2 p = pts2[verts[j]];
                if (!p.Equals(a) && !p.Equals(b) && !p.Equals(c) && PointInTriangle(p, a, b, c))
                    return false;
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

    private IEnumerator MakeChunkNav(MapEnum mapType, int w, int h)
    {
        yield return null;
        _groundComposite.GenerateGeometry();
        _wallComposite.GenerateGeometry();
        Mesh groundMesh = CreateMesh(_groundComposite);
        Mesh wallMesh = CreateMesh(_wallComposite);

        List<NavMeshBuildSource> sources = new();

        NavMeshBuildSource srcGround = new();
        srcGround.shape = NavMeshBuildSourceShape.Mesh;
        srcGround.sourceObject = groundMesh;
        srcGround.area = 0;
        srcGround.transform = Matrix4x4.TRS(
        new Vector3(w * 16f, h * 16f, 0f),
        Quaternion.Euler(0, 0f, 0f),
        Vector3.one);

        sources.Add(srcGround);

        NavMeshBuildSource srcWall = new();
        srcWall.shape = NavMeshBuildSourceShape.Mesh;
        srcWall.sourceObject = wallMesh;
        srcWall.area = 1;
        srcWall.transform = Matrix4x4.TRS(
        new Vector3(w * 16f, h * 16f, 0f),
        Quaternion.Euler(0, 0f, 0f),
        Vector3.one);
        sources.Add(srcWall);

        NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0);
        buildSettings.agentRadius = 0f;

        var bounds = new Bounds(
          new Vector3(8f + w * 16f, 0, 8f + h * 16f),
          new Vector3(18f, 1f, 18f)
        );

        Vector3 positionOffset = new(0, 0, 0);
        Quaternion rotationOffset = Quaternion.Euler(new(-90, 0, 0));

        NavMeshData navData = NavMeshBuilder.BuildNavMeshData(
            buildSettings,
            sources,
            bounds,
            positionOffset,
            rotationOffset
        );

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

    private void GenerateFromGrid(TileType[,] grid, int w, int h)
    {
        for (int x = -1; x <= 16; x++)
        {
            for (int y = -1; y <= 16; y++)
            {
                GameObject go;
                BoxCollider2D bc;

                int correctX = x + 1;
                int correctY = y + 1;

                switch (grid[correctX, correctY])
                {
                    case TileType.None:
                        break;
                    case TileType.Empty:
                        go = new GameObject($"WallTile");
                        go.transform.SetParent(WallRoot.transform, false);
                        go.transform.localPosition = new Vector3(x + 0.5f, y + 0.5f, 0);
                        bc = go.AddComponent<BoxCollider2D>();
                        bc.size = Vector2.one;
                        bc.usedByComposite = true;
                        break;
                    case TileType.Wall:
                        go = new GameObject($"WallTile");
                        go.transform.SetParent(WallRoot.transform, false);
                        go.transform.localPosition = new Vector3(x + 0.5f, y + 0.5f, 0);
                        bc = go.AddComponent<BoxCollider2D>();
                        bc.size = Vector2.one;
                        bc.usedByComposite = true;
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

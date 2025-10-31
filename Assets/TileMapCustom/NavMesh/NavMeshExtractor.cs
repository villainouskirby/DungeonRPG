#if UNITY_EDITOR
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
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
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

        StartCoroutine(ExtractTilemap2NavMesh(mapType, mapData));
        //StartCoroutine(ExtractTilemap2ChunkNavMesh(mapType, mapData));
    }

    private enum TileType
    {
        None = 0,
        Wall = 1,
        Ground = 2,
        Empty = 3,
    }

    private IEnumerator ExtractTilemap2NavMesh(MapEnum mapType, TileMapData mapData)
    {
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

        int childCount = EM.Instance.LayerRoot.transform.childCount;
        List<Tilemap> tilemap = new();
        for (int i = 0; i < childCount; i++)
            if (EM.Instance.LayerRoot.transform.GetChild(i).gameObject.activeSelf && EM.Instance.LayerRoot.transform.GetChild(i).TryGetComponent(out Tilemap layerMap))
                tilemap.Add(layerMap);

        Tilemap wallMap = null;
        bool[] allMap = new bool[mapData.All.Width * mapData.All.Height * EM.ChunkSize * EM.ChunkSize];
        Array.Fill(allMap, false);

        for (int i = 0; i < tilemap.Count; i++)
        {
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

                        if (allMap.Length <= index)
                            continue;

                        TileBase tileBase = tiles[x + y * bounds.size.x];
                        Sprite wall = null;
                        if (tileBase is Tile tile)
                            wall = tile.sprite;

                        if (wall != null)
                            allMap[index] = true;

                        int tileSpriteIndex = 0;
                        for (int j = 0; j < mapData.LayerData.Length; j++)
                        {
                            if (mapData.LayerData[j].Tile.Length < index)
                                tileSpriteIndex = Mathf.Max(mapData.LayerData[j].Tile[index], tileSpriteIndex);
                        }
                        if (tileSpriteIndex == 0)
                            allMap[index] = true;
                    }
                }
            }
        }

        Mesh groundMesh = BuildMergedMesh(allMap, mapData.All.Width, mapData.All.Height);

        yield return StartCoroutine(MakeNav(mapType, mapData.All.Width, mapData.All.Height, groundMesh));
        yield return null;

        string assetPath = $"Assets/MapMeshData/ChunkNavMesh/{mapType.ToString()}/NavMesh.asset";
        RegisterAddressable(group, $"{mapType.ToString()}_NavMesh", assetPath);

        AssetDatabase.SaveAssets();
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

        int childCount = EM.Instance.LayerRoot.transform.childCount;
        List<Tilemap> tilemap = new();
        for (int i = 0; i < childCount; i++)
            if (EM.Instance.LayerRoot.transform.GetChild(i).gameObject.activeSelf && EM.Instance.LayerRoot.transform.GetChild(i).TryGetComponent(out Tilemap layerMap))
                tilemap.Add(layerMap);

        Tilemap wallMap = null;
        bool[] allMap = new bool[mapData.All.Width * mapData.All.Height * EM.ChunkSize * EM.ChunkSize];
        Array.Fill(allMap, false);

        for (int i = 0; i < tilemap.Count; i++)
        {
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
        }

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
                            if (allMap[index])
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
                                    if (allMap[index])
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
                                    if (allMap[index])
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
                                    if (allMap[index])
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
                                    if (allMap[index])
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
                        if (allMap[index])
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
                        if (allMap[index])
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
                        if (allMap[index])
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
                        if (allMap[index])
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

    private IEnumerator MakeNav(MapEnum mapType, int h, int w, Mesh groundMesh)
    {
        yield return null;

        List<NavMeshBuildSource> sources = new();

        NavMeshBuildSource srcGround = new();
        srcGround.shape = NavMeshBuildSourceShape.Mesh;
        srcGround.sourceObject = groundMesh;
        srcGround.area = 0;
        srcGround.transform = Matrix4x4.TRS(
        new Vector3(0f, 0f, 0f),
        Quaternion.Euler(0, 0f, 0f),
        Vector3.one);

        sources.Add(srcGround);

        NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0);
        buildSettings.agentRadius = 0f;

        var bounds = new Bounds(
          new Vector3(8f + w / 2 * 16f, 0, 8f + h / 2 * 16f),
          new Vector3(16f * (w + 2), 1f, 16f * (h + 2))
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

        AssetDatabase.CreateAsset(navData, $"Assets/MapMeshData/ChunkNavMesh/{mapType.ToString()}/NavMesh.asset");
        yield return null;
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

    public Mesh BuildMergedMesh(bool[] grid, int mapW, int mapH)
    {
        int width = mapW * 16;   // (요청대로 하드코딩 유지)
        int height = mapH * 16;

        bool[,] visited = new bool[width, height];

        var verts = new List<Vector3>(width * height * 4);
        var uvs = new List<Vector2>(width * height * 4);
        var tris = new List<int>(width * height * 6);
        var norms = new List<Vector3>(width * height * 4);

        // (x,y) → 1D 인덱스 변환 (네가 쓰던 계산을 함수로 캡슐화)
        int ToIndex(int gx, int gy)
        {
            Vector2Int chunkIndex = new(gx / EM.ChunkSize, gy / EM.ChunkSize);
            Vector2Int localIndex = new(gx % EM.ChunkSize, gy % EM.ChunkSize);

            int chunkStartIndex = chunkIndex.x + chunkIndex.y * mapW;
            int localStartIndex = chunkStartIndex * EM.ChunkSize * EM.ChunkSize;
            return localIndex.x + localIndex.y * EM.ChunkSize + localStartIndex;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width;)
            {
                int index = ToIndex(x, y); // ★ 현재 칸 인덱스는 매 위치에서 재계산

                if (!grid[index] && !visited[x, y])
                {
                    // 1) 가로 최대 확장
                    int w = 1;
                    while (x + w < width &&
                           !grid[ToIndex(x + w, y)] &&          // ★ 옆 칸 값 검사
                           !visited[x + w, y])
                    {
                        w++;
                    }

                    // 2) 세로 최대 확장 (가로 폭 w 유지)
                    int h = 1;
                    bool canGrow = true;
                    while (y + h < height && canGrow)
                    {
                        for (int dx = 0; dx < w; dx++)
                        {
                            int gx = x + dx;
                            int gy = y + h;

                            if (grid[ToIndex(gx, gy)] ||      // ★ 아래줄 각 칸 값 검사
                                visited[gx, gy])
                            {
                                canGrow = false;
                                break;
                            }
                        }
                        if (canGrow) h++;
                    }

                    // 3) 직사각형 쿼드 추가 (좌하→우하→우상→좌상)
                    float x0 = x * 1f;            // (요청대로 스케일 하드코딩 유지)
                    float y0 = y * 1f;
                    float x1 = (x + w) * 1f;
                    float y1 = (y + h) * 1f;

                    int vi = verts.Count;

                    verts.Add(new Vector3(x0, y0, 0f));
                    verts.Add(new Vector3(x1, y0, 0f));
                    verts.Add(new Vector3(x1, y1, 0f));
                    verts.Add(new Vector3(x0, y1, 0f));

                    // UV는 타일링(0~w, 0~h). 텍스처 Wrap=Repeat이면 반복됨.
                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(w, 0));
                    uvs.Add(new Vector2(w, h));
                    uvs.Add(new Vector2(0, h));

                    norms.Add(Vector3.forward);
                    norms.Add(Vector3.forward);
                    norms.Add(Vector3.forward);
                    norms.Add(Vector3.forward);

                    tris.Add(vi + 0);
                    tris.Add(vi + 2);
                    tris.Add(vi + 1);

                    tris.Add(vi + 0);
                    tris.Add(vi + 3);
                    tris.Add(vi + 2);

                    // 4) 방문 처리
                    for (int dy = 0; dy < h; dy++)
                        for (int dx = 0; dx < w; dx++)
                            visited[x + dx, y + dy] = true;

                    x += w; // 다음 시작점으로 점프
                }
                else
                {
                    x++;
                }
            }
        }

        var mesh = new Mesh
        {
            indexFormat = (verts.Count > 65000)
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16
        };
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetNormals(norms);
        mesh.SetTriangles(tris, 0, true);
        mesh.RecalculateBounds();

        return mesh;
    }

    private void GenerateFromGrid(TileType[,] grid)
    {
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
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
#endif
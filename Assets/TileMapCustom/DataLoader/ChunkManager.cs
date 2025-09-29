using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Member;
using DL = DataLoader;
using TM = TileMapMaster;

public class ChunkManager : MonoBehaviour, ITileMapBase
{
    public static ChunkManager Instance { get { return _instance; } }
    private static ChunkManager _instance;


    public FileStream MapStream;
    public FileStream HeightStream;
    public FileStream Wall03Stream;
    public FileStream Wall47Stream;
    public int[] LoadedChunkMapData;
    public int[] LoadedChunkHeightData;
    public int[] LoadedChunkWall03Data;
    public int[] LoadedChunkWall47Data;
    public Dictionary<Vector2Int, int> LoadedChunkIndex;
    public Dictionary<Vector3Int, ChunkGen> CachedChunk;

    public Action<Vector2Int> ChunkLoadAction;
    public Action<Vector2Int> ChunkUnloadAction;
    public Action<Vector2Int> ChunkRefreshAction;

    public int ViewChunkSize { get { return _viewChunkSize; } }
    private int _viewChunkSize;

    [Header("ChunkBlank Settings")]
    public int ChunkBlank = 2;

    [Header("Cach Settings")]
    public int MaxGen = 5;

    private byte[] _streamBuffer;
    private byte[] _heightBuffer;

    public class ChunkGen
    {
        public int Gen;
        public int[] ChunkData;

        public ChunkGen(int[] chunkData)
        {
            Gen = 0;
            ChunkData = chunkData;
        }
    }

    private void OnDisable()
    {
        MapStream.Close();
        HeightStream.Close();
        Wall03Stream.Close();
        Wall47Stream.Close();
    }


    public void Init()
    {
        _instance = this;
        LoadedChunkIndex = new();
        CachedChunk = new();
    }

    public void InitMap(MapEnum mapType)
    {
        ChunkLoadAction = null;
        ChunkUnloadAction = null;
        ChunkRefreshAction = null;
        LastChunkPos = GetChunkPos(PlayerMoveChecker.Instance.LastTilePos);
        LoadedChunkIndex.Clear();
        SetViewChunkSize(TM.Instance.ViewBoxSize);
        LoadedChunkMapData = new int[DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize * _viewChunkSize * _viewChunkSize * DL.Instance.All.LayerCount];
        LoadedChunkHeightData = new int[DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize * _viewChunkSize * _viewChunkSize];
        _streamBuffer = new byte[DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize * 4 + 4];
        _heightBuffer = new byte[DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize * 4 + 4];
        CachedChunk.Clear();


        string mapPath = JJSave.GetSavePath($"{mapType.ToString()}_Stream", $"JJSave/SaveFile/{TM.Instance.SaveSlotIndex}/{mapType.ToString()}/");

        MapStream?.Close();
        MapStream = new FileStream(
            mapPath,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.ReadWrite
        );

        string heightPath = JJSave.GetSavePath($"{mapType.ToString()}_Height", $"JJSave/SaveFile/{TM.Instance.SaveSlotIndex}/{mapType.ToString()}/");

        HeightStream?.Close();
        HeightStream = new FileStream(
            heightPath,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.ReadWrite
        );

        string wall03Path = JJSave.GetSavePath($"{mapType.ToString()}_Wall03", $"JJSave/SaveFile/{TM.Instance.SaveSlotIndex}/{mapType.ToString()}/");

        Wall03Stream?.Close();
        Wall03Stream = new FileStream(
            wall03Path,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.ReadWrite
        );

        string wall47Path = JJSave.GetSavePath($"{mapType.ToString()}_Wall47", $"JJSave/SaveFile/{TM.Instance.SaveSlotIndex}/{mapType.ToString()}/");

        Wall47Stream?.Close();
        Wall47Stream = new FileStream(
            wall47Path,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.ReadWrite
        );


        PlayerMoveChecker.Instance.AddMoveAction(CheckChunkMove);
    }

    public void StartMap(MapEnum mapType)
    {
        // 맵 시작시 전체 청크 로딩
        UpdateAllChunk(LastChunkPos);
    }

    public void RefreshLayer(int layer)
    {
        foreach (Vector2Int pos in GetAllChunkIndexInViewBox(_viewChunkSize))
        {
            ChunkRefreshAction?.Invoke(pos);
        }
    }

    public void SaveMapData()
    {

    }


    public int GetViewBoxSize()
    {
        return DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize * _viewChunkSize * _viewChunkSize;
    }

    public int[] GetMappingArray()
    {
        int[] mapping = new int[_viewChunkSize * _viewChunkSize];

        for (int i = 0; i < _viewChunkSize; i++)
        {   
            for (int j = 0; j < _viewChunkSize; j++)
            {
                mapping[Pos2ArrayIndex(new(j, i), _viewChunkSize)] = LoadedChunkIndex[ConvertLocalChunkPos2WorldChunkPos(new(j, i), LastChunkPos)];
            }
        }
        return mapping;
    }


    public void UpdateAllChunk(Vector2Int playerChunkPos)
    {
        LoadedChunkIndex.Clear();
        CachedChunk.Clear();
        SetViewChunkSize(TM.Instance.ViewBoxSize);
        LoadedChunkMapData = new int[DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize * _viewChunkSize * _viewChunkSize * DL.Instance.All.LayerCount];
        LoadedChunkHeightData = new int[DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize * _viewChunkSize * _viewChunkSize];
        LoadedChunkWall03Data = new int[DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize * _viewChunkSize * _viewChunkSize];
        LoadedChunkWall47Data = new int[DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize * _viewChunkSize * _viewChunkSize];
        Vector2Int[] allChunk = GetAllChunkIndexInViewBox(_viewChunkSize);

        for (int i = 0; i < allChunk.Length; i++)
        {
            LoadedChunkIndex.Add(allChunk[i], i);

            for (int j = 0; j < DL.Instance.All.LayerCount; j++)
            {
                int offset = GetLayerChunkStartIndex(j, DL.Instance.All.ChunkSize, _viewChunkSize, _viewChunkSize);
                offset += Pos2ArrayIndex(GetLocalChunkPos(allChunk[i], playerChunkPos), _viewChunkSize) * DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize;
                LoadChunkFromStream(allChunk[i], j).CopyTo(LoadedChunkMapData.AsSpan(offset));
            }
            LoadChunkFromHeight(allChunk[i]).CopyTo(LoadedChunkHeightData.AsSpan(LoadedChunkIndex[allChunk[i]] * DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize));
            LoadChunkFromWall03(allChunk[i]).CopyTo(LoadedChunkWall03Data.AsSpan(LoadedChunkIndex[allChunk[i]] * DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize));
            LoadChunkFromWall47(allChunk[i]).CopyTo(LoadedChunkWall47Data.AsSpan(LoadedChunkIndex[allChunk[i]] * DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize));
        }
    }

    public void UpdateChangeChunk(Vector2Int playerChunkPos)
    {
        GetNewAndRemovedChunk(LastChunkPos, playerChunkPos, _viewChunkSize, out var added, out var removed);

        if (added.Count != removed.Count)
        {
            UpdateAllChunk(playerChunkPos);
            return;
        }

        for (int i = 0; i < added.Count; i++)
        {
            // 사라질 청크들의 유산을 이어 받는다..
            LoadedChunkIndex.Add(added[i], LoadedChunkIndex[removed[i]]);
            ChunkLoadAction?.Invoke(added[i]);

            for (int j = 0; j < DL.Instance.All.LayerCount; j++)
            {
                int offset = GetLayerChunkStartIndex(j, DL.Instance.All.ChunkSize, _viewChunkSize, _viewChunkSize);
                offset += LoadedChunkIndex[removed[i]] * DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize;
                LoadChunkFromStream(added[i], j).CopyTo(LoadedChunkMapData.AsSpan(offset));
            }
            LoadChunkFromHeight(added[i]).CopyTo(LoadedChunkHeightData.AsSpan(LoadedChunkIndex[removed[i]] * DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize));
            LoadChunkFromWall03(added[i]).CopyTo(LoadedChunkWall03Data.AsSpan(LoadedChunkIndex[removed[i]] * DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize));
            LoadChunkFromWall47(added[i]).CopyTo(LoadedChunkWall47Data.AsSpan(LoadedChunkIndex[removed[i]] * DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize));

            LoadedChunkIndex.Remove(removed[i]);
            ChunkUnloadAction?.Invoke(removed[i]);
        }

        SetMapBuffer();
        MapManager.Instance.ChangeHeightData();
    }

    private void SetMapBuffer()
    {
        for(int i = 0; i < DL.Instance.All.LayerCount; i++)
        {
            MapManager.Instance.ChangeMapData(i);
        }
    }

    private void GetNewAndRemovedChunk(Vector2Int oldViewBoxPos, Vector2Int newViewBoxPos, int viewBoxSize,
    out List<Vector2Int> added, out List<Vector2Int> removed)
    {
        int sideViewBoxSize = (viewBoxSize - 1) / 2;

        HashSet<Vector2Int> oldSet = new();
        HashSet<Vector2Int> newSet = new();
        Vector2Int oldPos = new(oldViewBoxPos.x - sideViewBoxSize, oldViewBoxPos.y - sideViewBoxSize);
        Vector2Int newPos = new(newViewBoxPos.x - sideViewBoxSize, newViewBoxPos.y - sideViewBoxSize);

        for (int x = 0; x < viewBoxSize; x++)
        {
            for (int y = 0; y < viewBoxSize; y++)
            {
                oldSet.Add(new Vector2Int(oldPos.x + x, oldPos.y + y));
                newSet.Add(new Vector2Int(newPos.x + x, newPos.y + y));
            }
        }

        added = newSet.Except(oldSet).ToList();
        removed = oldSet.Except(newSet).ToList();
    }


    private Vector2Int[] GetAllChunkIndexInViewBox(int viewChunkSize)
    {
        Vector2Int[] chunk = new Vector2Int[viewChunkSize * viewChunkSize];
        for (int i = 0; i < viewChunkSize; i++)
        {
            for (int j = 0; j < viewChunkSize; j++)
            {
                chunk[Pos2ArrayIndex(new(i, j), viewChunkSize)] = ConvertLocalChunkPos2WorldChunkPos(new(i, j), LastChunkPos);
            }
        }

        return chunk;
    }

    public int[] LoadChunkFromStream(Vector2Int chunkPos, int layerIndex)
    {

        int tileCount = DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize;

        if (chunkPos.x < 0 || chunkPos.x >= DL.Instance.All.Width || chunkPos.y < 0 || chunkPos.y >= DL.Instance.All.Height)
        {
            int[] empty = new int[tileCount];
            Array.Fill(empty, -1);

            return empty;
        }

        int offset = GetLayerChunkStartIndex(layerIndex, DL.Instance.All.ChunkSize, DL.Instance.All.Width, DL.Instance.All.Height) * 4
            + 4; // Array Data는 앞에 4의 Header를 가지고 있음, 일단 Layer 시작 지점을 가져옴
        int chunkIndex = Pos2ArrayIndex(chunkPos, DL.Instance.All.Width);
        offset += GetChunkStartPos(chunkIndex, DL.Instance.All.ChunkSize) * 4;

        Array.Copy(BitConverter.GetBytes(tileCount), 0, _streamBuffer, 0, 4);
        // Byte로 데이터를 읽어오기에 tileCount * 4로 읽어줘야함. Int32 기준
        MapStream.Seek(offset, SeekOrigin.Begin);
        MapStream.Read(_streamBuffer, 4, tileCount * 4);

        int[] result = TypeByte2TypeConverter.Convert<int[]>(_streamBuffer);

        return result;
    }

    public int[] LoadChunkFromHeight(Vector2Int chunkPos)
    {

        int tileCount = DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize;

        if (chunkPos.x < 0 || chunkPos.x >= DL.Instance.All.Width || chunkPos.y < 0 || chunkPos.y >= DL.Instance.All.Height)
        {
            int[] empty = new int[tileCount];
            Array.Fill(empty, -1);

            return empty;
        }

        int offset = 4; // Array Data는 앞에 4의 Header를 가지고 있음, 일단 Layer 시작 지점을 가져옴
        int chunkIndex = Pos2ArrayIndex(chunkPos, DL.Instance.All.Width);
        offset += GetChunkStartPos(chunkIndex, DL.Instance.All.ChunkSize) * 4;

        Array.Copy(BitConverter.GetBytes(tileCount), 0, _heightBuffer, 0, 4);
        // Byte로 데이터를 읽어오기에 tileCount * 4로 읽어줘야함. Int32 기준
        HeightStream.Seek(offset, SeekOrigin.Begin);
        HeightStream.Read(_heightBuffer, 4, tileCount * 4);

        int[] result = TypeByte2TypeConverter.Convert<int[]>(_heightBuffer);

        return result;
    }

    public int[] LoadChunkFromWall03(Vector2Int chunkPos)
    {

        int tileCount = DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize;

        if (chunkPos.x < 0 || chunkPos.x >= DL.Instance.All.Width || chunkPos.y < 0 || chunkPos.y >= DL.Instance.All.Height)
        {
            int[] empty = new int[tileCount];
            Array.Fill(empty, -1);

            return empty;
        }

        int offset = 4; // Array Data는 앞에 4의 Header를 가지고 있음, 일단 Layer 시작 지점을 가져옴
        int chunkIndex = Pos2ArrayIndex(chunkPos, DL.Instance.All.Width);
        offset += GetChunkStartPos(chunkIndex, DL.Instance.All.ChunkSize) * 4;

        Array.Copy(BitConverter.GetBytes(tileCount), 0, _heightBuffer, 0, 4);
        // Byte로 데이터를 읽어오기에 tileCount * 4로 읽어줘야함. Int32 기준
        Wall03Stream.Seek(offset, SeekOrigin.Begin);
        Wall03Stream.Read(_heightBuffer, 4, tileCount * 4);

        int[] result = TypeByte2TypeConverter.Convert<int[]>(_heightBuffer);

        return result;
    }

    public int[] LoadChunkFromWall47(Vector2Int chunkPos)
    {

        int tileCount = DL.Instance.All.ChunkSize * DL.Instance.All.ChunkSize;

        if (chunkPos.x < 0 || chunkPos.x >= DL.Instance.All.Width || chunkPos.y < 0 || chunkPos.y >= DL.Instance.All.Height)
        {
            int[] empty = new int[tileCount];
            Array.Fill(empty, -1);

            return empty;
        }

        int offset = 4; // Array Data는 앞에 4의 Header를 가지고 있음, 일단 Layer 시작 지점을 가져옴
        int chunkIndex = Pos2ArrayIndex(chunkPos, DL.Instance.All.Width);
        offset += GetChunkStartPos(chunkIndex, DL.Instance.All.ChunkSize) * 4;

        Array.Copy(BitConverter.GetBytes(tileCount), 0, _heightBuffer, 0, 4);
        // Byte로 데이터를 읽어오기에 tileCount * 4로 읽어줘야함. Int32 기준
        Wall47Stream.Seek(offset, SeekOrigin.Begin);
        Wall47Stream.Read(_heightBuffer, 4, tileCount * 4);

        int[] result = TypeByte2TypeConverter.Convert<int[]>(_heightBuffer);

        return result;
    }

    // 원하는 TilePos와 LayerIndex를 넘겨주면 해당하는 Tile값을 넘겨줌
    // 외부에서는 그저 호출만 하면 됨
    // Cach나 ViewBox에서는 자동으로 읽어온다.
    public int GetTile(Vector2Int tilePos, int layerIndex)
    {
        Vector2Int chunkPos = GetChunkPos(tilePos);
        if (CheckChunkInViewBox(chunkPos))
        {
            return LoadTileFromViewBox(tilePos, layerIndex);
        }
        else
        {
            return LoadTileFromCached(tilePos, layerIndex);
        }
    }

    public int GetHeight(Vector2Int tilePos, int layerIndex)
    {
        Vector2Int localTilePos = GetLocalTilePos(tilePos);
        Vector2Int localChunkPos = GetChunkPos(tilePos);
        int mappingIndex = LoadedChunkIndex[localChunkPos];
        int localIndex = GetChunkStartPos(mappingIndex, DL.Instance.All.ChunkSize)
            + Pos2ArrayIndex(localTilePos, DL.Instance.All.ChunkSize);

        int shift = layerIndex * 4;
        int mask = 0b1111 << shift;
        return (LoadedChunkHeightData[localIndex] & mask) >> shift;
    }

    private int LoadTileFromCached(Vector2Int playerTilePos, int layerIndex)
    {
        Vector2Int chunkPos = GetChunkPos(playerTilePos);
        if (!CachedChunk.TryGetValue(new(chunkPos.x, chunkPos.y, layerIndex), out ChunkGen chunkData))
        {
            ChunkGen chunkGen = new(LoadChunkFromStream(chunkPos, layerIndex));
            CachedChunk[new(chunkPos.x, chunkPos.y, layerIndex)] = chunkGen;
        }

        int result = chunkData.ChunkData[Pos2ArrayIndex(GetLocalTilePos(playerTilePos), DL.Instance.All.ChunkSize)];
        CheckChunkGen();
        return result;
    }

    private void CheckChunkGen()
    {
        List<Vector3Int> toRemove = new();
        foreach (var kvp in CachedChunk)
        {
            kvp.Value.Gen++;
            if (kvp.Value.Gen > MaxGen)
                toRemove.Add(kvp.Key);
        }
        foreach (var key in toRemove)
            CachedChunk.Remove(key);
    }

    private int LoadTileFromViewBox(Vector2Int tilePos, int layerIndex)
    {
        return LoadedChunkMapData[GetIndexInViewBox(tilePos, layerIndex)];
    }

    public Vector2Int LastChunkPos;
    public Vector2Int NewChunkPos;

    private void CheckChunkMove(Vector2Int playerTilePos)
    {
        NewChunkPos = GetChunkPos(playerTilePos);

        if (NewChunkPos != LastChunkPos)
        {
            // Player allocated Chunk Change

            // Check Blank Out

            int absX = Mathf.Abs(LastChunkPos.x - NewChunkPos.x);
            int absY = Mathf.Abs(LastChunkPos.y - NewChunkPos.y);

            if (absX > ChunkBlank || absY > ChunkBlank)
            {
                // 마지막 청크 로딩 위치 기준
                // 움직임을 염두로 둔 Blank 범위를 벗어남
                // 청크를 새롭게 로딩

                UpdateChangeChunk(GetChunkPos(playerTilePos));
                Shader.SetGlobalInt("_CenterChunkX", NewChunkPos.x);
                Shader.SetGlobalInt("_CenterChunkY", NewChunkPos.y);
                LastChunkPos = NewChunkPos;
                MapManager.Instance.ChangeMapping();
            }    
        }
    }


    // 마지막으로 청크 로딩한 지점 기준으로 LocalChunk위치를 구함
    private Vector2Int GetLocalChunkPos(Vector2Int chunkPos, Vector2Int centerPos)
    {
        int sideViewChunkSize = (_viewChunkSize - 1) / 2;
        int correctX = chunkPos.x - centerPos.x + sideViewChunkSize;
        int correctY = chunkPos.y - centerPos.y + sideViewChunkSize;

        return new(correctX, correctY);
    }

    private Vector2Int ConvertLocalChunkPos2WorldChunkPos(Vector2Int localChunkPos, Vector2Int playerChunk)
    {
        int sideViewChunkSize = (_viewChunkSize - 1) / 2;
        int correctX = playerChunk.x + localChunkPos.x - sideViewChunkSize;
        int correctY = playerChunk.y + localChunkPos.y - sideViewChunkSize;

        return new(correctX, correctY);
    }

    private Vector2Int GetLocalTilePos(Vector2Int playerTilePos)
    {
        return new(playerTilePos.x % DL.Instance.All.ChunkSize, playerTilePos.y % DL.Instance.All.ChunkSize);
    }

    private int GetIndexInViewBox(Vector2Int worldPos, int layerIndex)
    {
        Vector2Int localTilePos = GetLocalTilePos(worldPos);
        Vector2Int localChunkPos = GetChunkPos(worldPos);
        int mappingIndex = LoadedChunkIndex[localChunkPos];
        int localIndex = GetChunkStartPos(mappingIndex, DL.Instance.All.ChunkSize)
            + Pos2ArrayIndex(localTilePos, DL.Instance.All.ChunkSize);
        return GetLayerChunkStartIndex(layerIndex, DL.Instance.All.ChunkSize, _viewChunkSize, _viewChunkSize) + localIndex;
    }

    private int Pos2ArrayIndex(Vector2Int vector2Int, int xSize)
    {
        return vector2Int.x + vector2Int.y * xSize;
    }

    private int GetLayerChunkStartIndex(int layerIndex, int chunkSize, int chunkCountX, int chunkCountY)
    {
        return chunkSize * chunkSize * chunkCountX * chunkCountY * layerIndex;
    }

    public int GetLayerStartIndex(int layerIndex)
    {
        return GetViewBoxSize() * layerIndex;
    }

    private int GetHeightChunkStartIndex(int chunkSize, int chunkCountX, int chunkCountY)
    {
        return chunkSize * chunkSize * chunkCountX * chunkCountY;
    }

    // 지금 보여야하는 박스 안에 있는가..?
    // 실시간으로 진행되는게 아니라 ViewBox를 생성하는 그 타이밍에 생성됨
    private bool CheckChunkInViewBox(Vector2Int chunkPos)
    {
        Vector2Int chunkLocalPos = GetLocalChunkPos(chunkPos, LastChunkPos);
        return ((chunkLocalPos.x >= 0 && chunkLocalPos.x < _viewChunkSize) && (chunkLocalPos.y >= 0 && chunkLocalPos.y < _viewChunkSize));
    }

    private Vector2Int GetChunkPos(Vector2Int playerTilePos)
    {
        int chunkSize = DL.Instance.All.ChunkSize;

        return new(playerTilePos.x / chunkSize, playerTilePos.y / chunkSize);
    }

    private int GetChunkStartPos(int chunkIndex, int chunkSize)
    {
        return chunkSize * chunkSize * chunkIndex;
    }

    public void SetViewChunkSize(int viewBoxSize)
    {
        // 한쪽 방향 기준 몇개의 청크가 필요한지
        int maxSideChunkSize = (viewBoxSize + 15) / DL.Instance.All.ChunkSize;
        // 전체 필요 청크 갯수
        _viewChunkSize = maxSideChunkSize * 2 + 1 + ChunkBlank * 2;
    }

    public int Prime { get { return (int)TileMapBasePrimeEnum.ChunkManager; } }
}

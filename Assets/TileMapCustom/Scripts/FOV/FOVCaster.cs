using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class FOVCaster : MonoBehaviour
{
    [Header("FOV Settings")]
    public int Radius;  // FOV 반지름
    public int RemainRadius;
    [Range(0f, 1f)]
    public float ShadowCorrect;
    public float BoxSize;

    private bool _isCast = false;
    private Vector2Int _lastTilePos;      // 플레이어의 현재 타일 좌표
    private float _tileSize;
    private int _lastRadius;
    private TileMapData _mapData;
    private GameObject _player;

    private IntervalNodePool _intervalNodePool;
    private OctantTransform[] octantTransforms =
    {
        new OctantTransform(1, 0, 0, 1),
        new OctantTransform(0, 1, 1, 0),
        new OctantTransform(0, -1, 1, 0),
        new OctantTransform(-1, 0, 0, 1),
        new OctantTransform(-1, 0, 0, -1),
        new OctantTransform(0, -1, -1, 0),
        new OctantTransform(0, 1, -1, 0),
        new OctantTransform(1, 0, 0, -1)
    };

    // 현재 보고 있는 맵
    private float[] _fovMap;

    void FixedUpdate()
    {
        if (!_isCast)
            return;

        if (_lastRadius != Radius)
        {
            _fovMap = new float[(Radius * 2 + 1) * (Radius * 2 + 1)];
            _lastRadius = Radius;
        }
        _tileSize = MapManager.Instance.TileSize;
        Vector2Int newTilePos = GetCurrentTilePos();

        if (newTilePos != _lastTilePos)
        {
            ComputeVisibility(newTilePos, Radius);
            _lastTilePos = newTilePos;
            MapManager.Instance.ChangeFOVData(_fovMap, Radius);
        }
    }

    Vector2Int GetCurrentTilePos()
    {
        int tileX = Mathf.FloorToInt(_player.transform.position.x / _tileSize);
        int tileY = Mathf.FloorToInt(_player.transform.position.y / _tileSize);
        return new Vector2Int(tileX, tileY);
    }

    public void StartCast()
    {
        Init();
        _isCast = true;
    }

    private void Init()
    {
        _mapData = MapManager.Instance.MapData;
        _tileSize = MapManager.Instance.TileSize;
        _lastTilePos = new Vector2Int(0, 0);
        _player = MapManager.Instance.Player;
        _intervalNodePool = new();
        _blockedIntervalTree = new(_intervalNodePool);
    }

    public void ComputeVisibility(Vector2Int viewerPos, int viewRadius)
    {
        Array.Fill(_fovMap, 0);
        SetLight(viewerPos, viewerPos.x, viewerPos.y, viewRadius, 0); // 시야자의 셀은 항상 보입니다.

        for (int i = 0; i < octantTransforms.Length; i++)
        {
            CastLight(viewerPos, viewRadius, octantTransforms[i]);
        }
    }

    private IntervalTree _blockedIntervalTree;

    private void CastLight(Vector2Int viewerPos, int viewRadius, OctantTransform transform)
    {
        bool previousWasBlocked = false;
        float blockedStartSlope = -1;
        float blockedEndSlope = -1;
        _blockedIntervalTree.Clear();

        for (int x = 1; x <= viewRadius; x++)
        {
            // 만약 다음 블럭의 처음과 끝이 이미 가려졌다면 종료 그 이후는 전부 가려짐
            if (_blockedIntervalTree.Overlaps(new(MathF.Min((0 - 0.5f) / (x + 0.5f), (0 - 0.5f) / (x - 0.5f)), (x + 0.5f) / (x - 0.5f))))
            {
                break;
            }

            for (int y = 0; y <= x; y++)
            {
                int gridX = viewerPos.x + x * transform.xx + y * transform.xy;
                int gridY = viewerPos.y + x * transform.yx + y * transform.yy;

                if (gridX < 0 || gridX >= _mapData.Width || gridY < 0 || gridY >= _mapData.Height)
                {
                    continue;
                }

                GetSlope(x, y, out float leftBlockSlope, out float rightBlockSlope);
                Interval currentBlockInterval = new(rightBlockSlope, leftBlockSlope);
                if (_blockedIntervalTree.Overlaps(currentBlockInterval))
                    continue;

                int distance = Mathf.Max(x, y);
                if (distance <= viewRadius)
                {
                    SetLight(viewerPos, gridX, gridY, viewRadius, distance);
                }

                bool currentBlocked = IsWall(gridX, gridY);
                if (currentBlocked)
                {
                    _blockedIntervalTree.Insert(currentBlockInterval);
                }
                /*
                if (currentBlocked)
                {
                    if (previousWasBlocked)
                    {
                        blockedEndSlope = leftBlockSlope;
                    }
                    else
                    {
                        blockedStartSlope = rightBlockSlope;
                        blockedEndSlope = leftBlockSlope;
                    }
                    previousWasBlocked = true;
                }
                else
                {
                    if (previousWasBlocked)
                    {
                        Debug.Log($"{blockedStartSlope} start : {blockedEndSlope} end");
                        blockedIntervalTree.Insert(new(blockedStartSlope, blockedEndSlope));
                    }
                    previousWasBlocked = false;
                }
                */
            }

            if (previousWasBlocked)
            {
                _blockedIntervalTree.Insert(new(blockedStartSlope, blockedEndSlope));
            }
        }
    }

    private void GetSlope(int x, int y, out float leftBlockSlope, out float rightBlockSlope)
    {
        float boxSize = BoxSize * 0.5f;
        leftBlockSlope = (y + boxSize) / (x - boxSize);
        rightBlockSlope = MathF.Min((y - boxSize) / (x + boxSize), (y - boxSize) / (x - boxSize));

        leftBlockSlope = Mathf.Max(0, leftBlockSlope);
        rightBlockSlope = Mathf.Min(leftBlockSlope, rightBlockSlope);
    }

    private bool IsWall(int x, int y)
    {
        int tileValue = _mapData.GetTile(x, y);
        bool isWall = MapManager.Instance.CheckWall(tileValue);
        return isWall;
    }

    private void SetLight(Vector2Int standardPos, int x, int y, int radius, int distance)
    {
        int correctX = x - standardPos.x + radius;
        int correctY = y - standardPos.y + radius;
        int index = correctY * (radius * 2 + 1) + correctX;
        float light = 1f - (float)distance / (float)radius;
        light += ShadowCorrect;
        light = Mathf.Clamp01(light);
        _fovMap[index] = light;
    }
}

class OctantTransform
{
    public int xx, xy, yx, yy;

    public OctantTransform(int xx, int xy, int yx, int yy)
    {
        this.xx = xx;
        this.xy = xy;
        this.yx = yx;
        this.yy = yy;
    }
}

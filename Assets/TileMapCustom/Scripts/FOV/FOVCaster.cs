using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using MM = MapManager;
using static MapBufferChanger;

[RequireComponent(typeof(MapManager))]
public class FOVCaster : MonoBehaviour, ITileMapOption
{
    [Header("FOV Settings")]
    public int Radius;  // FOV 반지름
    public int RemainRadius;
    [Range(0f, 1f)]
    public float ShadowCorrect;
    public float BoxSize;

    private bool _isActive = false;
    private int _lastRadius;
    private Action _bufferChangeEnd;


    // FOVDataBuffer
    private GraphicsBuffer _fovDataBuffer;
    public readonly int FovDataBufferHeaderSize = 1;
    // FOVDataBuffer Property
    public GraphicsBuffer FOVDataBuffer => _fovDataBuffer;


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

    public int Prime { get { return (int)TileMapOptionPrimeEnum.FOVCaster; } }

    public void Init()
    {
        _fovMap = new float[(Radius * 2 + 1) * (Radius * 2 + 1)];
        _intervalNodePool = new();
        _blockedIntervalTree = new(_intervalNodePool);
        _lastRadius = 0;
    }
    public void InitMap(MapEnum mapType)
    {
        // 따로 로직 필요 X
        _lastRadius = 0;
        ChangeFOVData();
    }

    public void StartMap(MapEnum mapType)
    {
        OnOption();
    }


    public void OnOption()
    {
        if (_isActive)
            return;

        PlayerMoveChecker.Instance.AddMoveAction(ComputeVisibility);
        PlayerMoveChecker.Instance.AddMoveEndAction(ChangeFOVData);
        Shader.SetGlobalFloat("_FOVActive", 1.0f);
        _isActive = true;
    }

    public void OffOption()
    {
        if (!_isActive)
            return;

        PlayerMoveChecker.Instance.DeleteMoveAction(ComputeVisibility);
        PlayerMoveChecker.Instance.DeleteMoveEndAction(ChangeFOVData);
        Shader.SetGlobalFloat("_FOVActive", 0.0f);
        _isActive = false;
    }

    public TileMapOptionEnum OptionType { get { return TileMapOptionEnum.FOVCaster; } }

    void Update()
    {
        if (!_isActive)
            return;

        Shader.SetGlobalInt("_FOVRadius", Radius);
    }

    void FixedUpdate()
    {
        if (!_isActive)
            return;

        if (_lastRadius != Radius)
        {
            _fovMap = new float[(Radius * 2 + 1) * (Radius * 2 + 1)];
            ChangeFOVData();
            _lastRadius = Radius;
        }
    }

    private void ChangeFOVData(Vector2Int tilePos)
    {
       ChangeFOVData();
    }

    public void AddBufferChangeEndAction(Action action)
    {
        _bufferChangeEnd += action;
    }

    public void ChangeFOVData()
    {
        if (_lastRadius != Radius)
        {
            SetDataBuffer(new float[(Radius * 2 + 1) * (Radius * 2 + 1)], ref _fovDataBuffer, FovDataBufferHeaderSize,
            0);
            _bufferChangeEnd?.Invoke();
        }
        _lastRadius = Radius;
        FOVDataBuffer.SetData(_fovMap, 0, 1, _fovMap.Length);
    }

    // 0 -> false, 0 < ~ -> true
    public float IsInFOV(Vector2Int tilePos)
    {
        if (!_isActive)
            return 1.0f;

        int relativeX = tilePos.x - PlayerMoveChecker.Instance.NewTilePos.x;
        int relativeY = tilePos.y - PlayerMoveChecker.Instance.NewTilePos.y;
        int correctX = relativeX + Radius;
        int correctY = relativeY + Radius;

        if (Mathf.Abs(relativeX) > Radius || Mathf.Abs(relativeY) > Radius)
            return 0;

        if (0 < _fovMap[(Radius * 2 + 1) * correctY + correctX])
        {
            return _fovMap[(Radius * 2 + 1) * correctY + correctX];
        }
        else
            return 0;
    }

    public void ComputeVisibility(Vector2Int viewerPos)
    {
        Array.Fill(_fovMap, 0);
        SetLight(viewerPos, viewerPos.x, viewerPos.y, Radius, 0); // 시야자의 셀은 항상 보입니다.

        for (int i = 0; i < octantTransforms.Length; i++)
        {
            CastLight(viewerPos, Radius, octantTransforms[i]);
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

                if (gridX < 0 || gridX >= MM.Instance.MapData.Width || gridY < 0 || gridY >= MM.Instance.MapData.Height)
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
        int tileValue = MM.Instance.MapData.GetTile(x, y);
        bool isWall = MM.Instance.CheckWall(tileValue);
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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WallColliderGenerator : MonoBehaviour
{
    [Header("Active Settings")]
    public int ActiveRange = 1;


    private int _lastRange;

    void Start()
    {
        _lastRange = 0;
        Init();
    }

    public void Init()
    {
        _lastRange = 0;
        PlayerMoveChecker.Instance.AddMoveAction(UpdateActiveTile);
    }

    private void UpdateActiveTile(Vector2Int newTilePos)
    {
        WallColliderManager.Instance.UpdateActiveTile(PlayerMoveChecker.Instance.LastTilePos, _lastRange, newTilePos, ActiveRange);
        _lastRange = ActiveRange;
    }

    private void OnDisable()
    {
        if(MapManager.Instance.WallRoot != null)
            WallColliderManager.Instance.UpdateActiveTile(PlayerMoveChecker.Instance.LastTilePos, _lastRange, new Vector2Int(0, 0), 0);

        PlayerMoveChecker.Instance.DeleteMoveAction(UpdateActiveTile);
    }
}

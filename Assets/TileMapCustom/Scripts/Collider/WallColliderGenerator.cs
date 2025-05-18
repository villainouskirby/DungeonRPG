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

    // 맵 이동 간에 벽 생성 스크립트를 다시 Init 해줘야함..
    // 이 부분은 플레이어 초기화 부분과 연결될거같아서 그 때 추가하는 걸로 어처피 플레이말고는 해당사항없음
    // (맵 이동시 남아있는 오브젝트는 플레이어가 유일)
    // 일단 Master에서 임의로 실행
    public void Init()
    {
        _lastRange = 0;
        UpdateActiveTile(PlayerMoveChecker.Instance.LastTilePos);
        PlayerMoveChecker.Instance.AddMoveAction(UpdateActiveTile);
    }

    private void UpdateActiveTile(Vector2Int newTilePos)
    {
        WallColliderManager.Instance.UpdateActiveTile(PlayerMoveChecker.Instance.LastTilePos, _lastRange, newTilePos, ActiveRange);
        _lastRange = ActiveRange;
    }

    private void OnDisable()
    {
        if(TileMapMaster.Instance.WallRoot != null)
            WallColliderManager.Instance.UpdateActiveTile(PlayerMoveChecker.Instance.LastTilePos, _lastRange, new Vector2Int(0, 0), 0);

        PlayerMoveChecker.Instance.DeleteMoveAction(UpdateActiveTile);
    }
}

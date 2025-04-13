// 미사용 cs
// 원래 Collider 없이 MapData 기반으로 벽 구현할려고 했는데 훨씬 좋은 방식이 떠올라서 폐기..
// 방식은 아까워서 냅둠

/*
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class TileMapColliderMovement : MonoBehaviour
{
    [Header("Wall Settings")]
    public bool UseGenericWall = true;
    public int[] CustomWallTileType = new int[]
    {
        -1,
        1
    };
    private int[] WallTileType;

    private Rigidbody2D _rb;
    private BoxCollider2D _col;
    private TileMapData _mapData;
    private Vector2[] _relativeCheckPoint;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<BoxCollider2D>();
        _mapData = MapManager.Instance.MapData;

        CalRelativeCheckPoint();
    }

    private void CalRelativeCheckPoint()
    {
        Vector2 halfSize = _col.size * 0.5f;

        _relativeCheckPoint = new Vector2[]
        {
            new Vector2(-halfSize.x, -halfSize.y),
            new Vector2(halfSize.x, -halfSize.y),
            new Vector2(-halfSize.x, halfSize.y),
            new Vector2(halfSize.x, halfSize.y)
        };
    }


    public void ApplyVelocity(Vector2 velocity)
    {
        if (velocity == Vector2.zero) return;

        Vector2 targetPos = _rb.position + velocity * Time.fixedDeltaTime;

        if (CanMoveTo(targetPos))
        {
            _rb.MovePosition(targetPos);
        }
        else
        {
            Vector2 adjustedPos = GetAdjustedPos(_rb.position, targetPos, velocity.normalized);
            _rb.MovePosition(adjustedPos);
        }
    }

    private Vector2 GetAdjustedPos(Vector2 currentPos, Vector2 targetPos, Vector2 direction)
    {
        float maxDistance = Vector2.Distance(currentPos, targetPos);

        for (float distance = 0; distance <= maxDistance; distance += 0.1f) // 작은 간격으로 이동 검사
        {
            Vector2 pos = currentPos + direction * distance;
            if (CanMoveTo(pos))
                return pos;
        }

        return _rb.position;
    }

    private bool CanMoveTo(Vector2 targetPos)
    {
        for(int i = 0; i < 4; i++)
        {
            Vector2 checkPoint = targetPos + _relativeCheckPoint[i];
            if (IsTile(checkPoint))
                return false;
        }

        return true;
    }


    private bool IsTile(Vector2 pos)
    {
        int tileX = Mathf.FloorToInt(pos.x);
        int tileY = Mathf.FloorToInt(pos.y);

        if (tileX < 0 || tileX >= _mapData.Width || tileY < 0 || tileY >= _mapData.Height)
            return false;
        int tileType = _mapData.GetTile(tileX, tileY);
        return WallTileType.Contains(tileType);
    }
}
*/
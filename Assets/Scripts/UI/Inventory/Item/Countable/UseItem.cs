using Cysharp.Threading.Tasks;
using UnityEngine;

public class UseItem : CountableItem, IUsableItem
{
    public UseItemData UseData { get; }

    public UseItem(UseItemData data, int amount = 1) : base(data, amount)
    {
        UseData = data;
    }

    public override Item Clone(int amount) => new UseItem(UseData, amount);

    public async UniTask<bool> Use()
    {
        if (Amount <= 0) return false;

        // 사용 가능 조건 체크 (거리/쿨다운 등)
        if (!UseItemRuntime.CanUse(UseData)) return false;

        // 사용 이펙트 적용
        bool ok = await UseItemRuntime.ApplyEffect(UseData);
        if (!ok) return false;

        // 소모
        Amount--;
        return true;
    }
}

// 이건 그냥 예시 사용아이템 로직 나중에 추가해야할듯
public static class UseItemRuntime
{
    static Transform Player => GameObject.FindWithTag("Player")?.transform; // 또는 PlayerController.instance.transform

    public static bool CanUse(UseItemData data)
    {
        // 필요하면 여기서 쿨다운/상태 잠금/소지 제한 검사 등
        return Player != null;
    }

    public static async Cysharp.Threading.Tasks.UniTask<bool> ApplyEffect(UseItemData data)
    {
        var row = data.Info;

        // 예시: 전방 레이캐스트로 대상 타격 (use_distance, damage)
        if (row.damage > 0 && row.use_distance > 0)
        {
            var pos = Player.position;
            var dir = Player.right; // 플레이어 바라보는 방향 기준으로 맞춰서 구현
            if (Physics2D.Raycast(pos, dir, row.use_distance, ~0))
            {
                var hit = Physics2D.Raycast(pos, dir, row.use_distance);
                if (hit.collider != null)
                {
                    hit.collider.gameObject.SendMessage("ApplyDamage", row.damage, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        // 예시: 소리 범위 어그로 (sound_range)
        if (row.sound_range > 0)
        {
            AlertEnemies(Player.position, row.sound_range);
        }

        await Cysharp.Threading.Tasks.UniTask.Yield();
        return true;
    }

    static void AlertEnemies(Vector3 center, float radius)
    {
        // 간단 브로드캐스트. 프로젝트에 맞게 대체 가능
        var cols = Physics2D.OverlapCircleAll(center, radius);
        foreach (var c in cols)
            c.gameObject.SendMessage("OnHeardSound", center, SendMessageOptions.DontRequireReceiver);
    }
}
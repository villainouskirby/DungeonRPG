using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ThrowItemManager : MonoBehaviour
{
    public static ThrowItemManager instance;

    [Header("조준 UI(옵션)")]
    [SerializeField] private ThrowAimUI aimUI;   // 없으면 런타임에 찾음
    [Header("에이밍시 플레이어 잠금 여부")]
    [SerializeField] private bool lockPlayerWhileAiming = true;

    private PlayerController player; // 캐싱
    private bool isUsing = false;

    // (선택) 시트 테이블 캐시가 필요하다면 여기서
    static Dictionary<string, Item_Info_ThrowItem> _useById;

    void Awake()
    {
        if (instance && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }

    void Start()
    {
        if (!player) player = FindObjectOfType<PlayerController>();
        if (!aimUI) aimUI = FindObjectOfType<ThrowAimUI>(includeInactive: true);
    }

    void EnsureTable()
    {
        if (_useById == null)
            _useById = SheetDataUtil.DicByKey(Item_Info.ThrowItem, r => r.id); // "PAR_MIN_ROC" 등
    }

    /// <summary>
    /// 사용 아이템(투척형) 처리: 조준→확정/취소만 담당. 실제 상호작용은 TODO에.
    /// </summary>
    public async UniTask<bool> UseThrowItem(ThrowItemData data)
    {
        if (isUsing) return false;
        if (data == null) { Debug.LogError("ThrowItemManager: data is null"); return false; }

        if (!player) player = FindObjectOfType<PlayerController>();
        if (!player) { Debug.LogError("ThrowItemManager: PlayerController not found"); return false; }

        if (!aimUI) aimUI = FindObjectOfType<ThrowAimUI>(includeInactive: true);
        if (!aimUI) { Debug.LogError("ThrowItemManager: ThrowAimUI not found"); return false; }

        EnsureTable();

        // ① 키 결정: 우선 data.Info?.id, 없으면 ITM의 PAR_DT 사용
        string dt = data.Info != null ? data.Info.id : data.PAR_DT;
        if (string.IsNullOrEmpty(dt))
        {
            Debug.LogError($"ThrowItemManager: PAR_DT missing on item (name={data.Name})");
            return false;
        }

        // ② 상세 로드/바인딩
        if (!_useById.TryGetValue(dt, out var row))
        {
            Debug.LogError($"ThrowItemManager: UseItem DT not found: {dt}");
            return false;
        }
        if (data.Info == null) data.SetInfo(row); // 캐시

        int maxDist = Mathf.Max(0, row.use_distance);

        // 사거리 0이면 즉시사용(조준 생략)
        if (maxDist == 0)
        {
            // TODO: 즉시 사용형 효과 (비워둠)
            return true;
        }

        isUsing = true;
        if (lockPlayerWhileAiming) player.LockState();

        var origin = player.transform.position;
        var result = await aimUI.AimAndSelect(origin, maxDist);

        if (lockPlayerWhileAiming) player.UnlockState();
        isUsing = false;

        if (!result.confirmed) return false;

        // === TODO: 실제 투척/스폰/대미지/사운드 처리 ===
        // 예) ProjectileManager.Spawn(data, player.transform.position, result.target);
        // if (row.sound_range > 0) AlertEnemies(player.transform.position, row.sound_range);

        return true;
    }
    // (선택) 어그로 브로드캐스트
    void AlertEnemies(Vector3 center, float radius)
    {
        var cols = Physics2D.OverlapCircleAll(center, radius);
        foreach (var c in cols)
            c.gameObject.SendMessage("OnHeardSound", center, SendMessageOptions.DontRequireReceiver);
    }
}
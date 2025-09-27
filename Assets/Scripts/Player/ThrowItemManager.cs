using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ItemDataExtensions;
using UnityEngine;

public class ThrowItemManager : MonoBehaviour
{
    public static ThrowItemManager instance;

    [Header("조준 UI(옵션)")]
    [SerializeField] private ThrowAimUI aimUI;   // 없으면 런타임에 찾음
    [Header("에이밍시 플레이어 잠금 여부")]
    [SerializeField] private bool lockPlayerWhileAiming = true;

    [Header("투척 비주얼")]
    [SerializeField] private float throwSpeed = 10f;  // m/s, 거리/속도 = 비행시간
    [SerializeField] private float baseArcHeight = 2f;// 포물선 기본 높이(거리 비례 가산)
    [SerializeField] private string projectileSortingLayer = "Default";
    [SerializeField] private int projectileOrderInLayer = 10;

    private PlayerController player; // 캐싱
    private bool isUsing = false;

    // (선택) 시트 테이블 캐시가 필요하다면 여기서
    static Dictionary<string, Item_Info_ThrowableItem> _useById;

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

    //void EnsureTable()
    //{
    //    if (_useById == null)
    //        _useById = SheetDataUtil.DicByKey(Item_Info.ThrowableItem, r => r.id); // "PAR_MIN_ROC" 등
    //}

    public async UniTask<bool> UseThrowItem(ItemData data)
    {
        if (isUsing) return false;
        if (data == null) { Debug.LogError("ThrowItemManager: data is null"); return false; }

        if (!player) player = FindObjectOfType<PlayerController>();
        if (!player) { Debug.LogError("ThrowItemManager: PlayerController not found"); return false; }

        if (!aimUI) aimUI = FindObjectOfType<ThrowAimUI>(includeInactive: true);
        if (!aimUI) { Debug.LogError("ThrowItemManager: ThrowAimUI not found"); return false; }


        /* // 키 결정: 우선 data.Info?.id, 없으면 ITM의 PAR_DT 사용
        string dt = data.Info != null ? data.Info.id : data.PAR_DT;
        if (string.IsNullOrEmpty(dt))
        {
            Debug.LogError($"ThrowItemManager: PAR_DT missing on item (name={data.Name})");
            return false;
        }

        // 상세 로드/바인딩
        if (!_useById.TryGetValue(dt, out var row))
        {
            Debug.LogError($"ThrowItemManager: UseItem DT not found: {dt}");
            return false;
        }
        if (data.Info == null) data.SetInfo(row); // 캐시 */

        ThrowableItemDataExtension throwableData = data.Extensions[ThrowableItemDataExtension.Name.Throwable] as ThrowableItemDataExtension;

        int maxDist = Mathf.Max(0, throwableData.Distance);

        // 사거리 0이면 즉시사용(조준 생략)
        if (maxDist == 0)
        {
            // TODO: 즉시 사용형 효과 (버프/즉발 스킬 등)
            return true;
        }

        isUsing = true;
        if (lockPlayerWhileAiming) player.LockState();

        var origin = player.transform.position;
        var aim = await aimUI.AimAndSelect(origin, maxDist);


        if (lockPlayerWhileAiming) player.UnlockState();
        isUsing = false;

        if (!aim.confirmed) return false;

        var sprite = data.IconSprite;
        await FlyProjectileVisual(origin, aim.target, sprite);

        // 소리 등록: row.sound_range(시트값) 사용, TTL(지속시간)은 3초 예시
        if (!ThrowNoiseManager.Instance)
        {
            var go = new GameObject("ThrowNoiseManager");
            go.AddComponent<ThrowNoiseManager>();
        }
        if (throwableData.SoundRange > 0f)
            ThrowNoiseManager.Instance.RegisterNoise(aim.target, throwableData.SoundRange, 3f);


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
    async UniTask FlyProjectileVisual(Vector3 origin, Vector3 target, Sprite sprite)
    {
        // 투사체 오브젝트 생성
        var go = new GameObject("ThrownItemVisual");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = projectileSortingLayer;
        sr.sortingOrder = short.MaxValue;

        // 시작 위치/회전
        go.transform.position = origin;
        go.transform.rotation = Quaternion.identity;

        // 비행 시간 = 거리 / 속도
        float dist = Vector3.Distance(origin, target);
        float duration = Mathf.Max(0.05f, dist / Mathf.Max(0.01f, throwSpeed));

        // 베지어 제어점(ThrowAimUI와 동일한 방식으로)
        var dir = (target - origin);
        dir.z = 0f;
        Vector3 control = (origin + target) * 0.5f + Vector3.up * (baseArcHeight + 0.25f * dir.magnitude);

        // 보간
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float tt = Mathf.Clamp01(t);

            // 2차 베지어 Q(t)
            Vector3 p = Quad(origin, control, target, tt);
            go.transform.position = p;

            // 방향(미분)으로 회전
            Vector3 v = QuadDeriv(origin, control, target, tt);
            if (v.sqrMagnitude > 0.0001f)
            {
                float ang = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
                go.transform.rotation = Quaternion.Euler(0, 0, ang);
            }

            await UniTask.Yield();
        }

        // 도착 지점 정렬
        go.transform.position = target;

        // TODO: 여기서 충돌 이펙트/사운드 재생 가능

        // 비주얼 제거 (필요시 풀링으로 교체)
        Destroy(go);
    }
    // 2차 베지어
    static Vector3 Quad(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        float u = 1f - t;
        return u * u * a + 2f * u * t * b + t * t * c;
    }
    // 2차 베지어 도함수(방향)
    static Vector3 QuadDeriv(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        return 2f * (1f - t) * (b - a) + 2f * t * (c - b);
    }
}
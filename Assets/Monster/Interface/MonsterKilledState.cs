using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DBUtility;
using UnityEngine;
using UnityEngine.AI;

public sealed class MonsterKilledState : IMonsterState
{
    static readonly HashSet<GameObject> s_despawning = new();
    static readonly Dictionary<GameObject, MonsterKilledState> s_instances = new();
    static readonly Dictionary<GameObject, int> s_versions = new();
    int _ver = 0;
    readonly MonsterContext ctx;
    readonly MonsterController ctr;
    readonly MonsterStateMachine root;
    readonly GameObject go;
    public static bool IsDespawning(GameObject go) => s_despawning.Contains(go);

    // 파밍 완료 시 호출: 즉시 3초 페이드 시작
    public static void OnFarmSuccess(GameObject go)
    {
        if (go == null) return;
        if (!s_instances.TryGetValue(go, out var inst)) return;
        if (s_despawning.Contains(go)) return;   // 이미 페이드 중이면 무시
        inst.GrantLootAndFadeNow();              // 인스턴스 메서드 호출
    }
    public static void ResetAlphaOnSpawn(GameObject go)
    {
        if (!go) return;
        var srs = go.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in srs)
        {
            if (!sr) continue;
            var c = sr.color;
            if (c.a < 1f) sr.color = new Color(c.r, c.g, c.b, 1f);
        }
        if (!s_versions.TryGetValue(go, out var v)) v = 0;
        s_versions[go] = v + 1;
        // 혹시 남아있을지 모를 플래그/참조도 정리
        s_despawning.Remove(go);
        s_instances.Remove(go);
    }
    public MonsterKilledState(MonsterContext c, MonsterStateMachine m, GameObject go, MonsterController mc)
    { ctx = c; root = m; this.go = go; ctr = mc; }

    public void Enter()
    {
        ctx.indicator?.Show(MonsterStateTag.Killed);
        ctx.SafeStopAgent();
        ctx.anim?.Play("Die");
        ctx.animationHub?.SetTag(MonsterStateTag.Killed, ctx);
        ctr._killed = true;
        if (ctx.alert) ctx.alert.gameObject.SetActive(false);

        if (!s_versions.TryGetValue(go, out var v)) v = 0;
        _ver = s_versions[go] = v + 1;

        s_instances[go] = this;

        _isFading = false;
        /*
        // 1) 몬스터에 설정된 드롭테이블 ID (예: DT_BTL_NOR)
        string dtId = ctr.mdata?.Monster_DT;
        if (string.IsNullOrEmpty(dtId))
        {
            Debug.LogWarning($"[MonsterKilledState] Monster_DT is empty on {ctr.name}");
            GoVanish();
            return;
        }

        // 2) 드롭테이블 시트에서 로드
        var dropTableDic = SheetDataUtil.DicByKey(Monster_Info.Monster_DropTable, x => x.DropTable_id);
        if (!dropTableDic.TryGetValue(dtId, out var dropRow))
        {
            Debug.LogWarning($"[MonsterKilledState] DropTable not found: {dtId}");
            GoVanish();
            return;
        }

        // 3) 파싱 & 굴림 (예: "MIN_CPR/100/2/3")
        List<DropResolved> rolls = DropTableResolver.RollDrops(dropRow.DropTable_Info);

        // 4) 지급: ItemDataConstructor + AddInventory(data)
        var inv = UIPopUpHandler.Instance.GetScript<Inventory>(); // 프로젝트에서 쓰는 인벤토리 접근
        if (inv == null)
        {
            Debug.LogWarning("[MonsterKilledState] Inventory not found via UIPopUpHandler");
        }
        else
        {
            foreach (var r in rolls)
            {
                // itemId는 시트의 항목 ID 그대로 사용 (예: "MIN_CPR")
                var data = ItemDataConstructor.GetItemData(r.itemId);
                if (data == null)
                {
                    Debug.LogWarning($"[MonsterKilledState] ItemData not found for id '{r.itemId}'");
                    continue;
                }

                inv.AddItem(data, r.count, true);
            }
        }
        */
        LifeCycleAsync().Forget();
    }
    bool _isFading;

    // 27초 대기 후(총 30초-3초) 자동 페이드
    async UniTaskVoid LifeCycleAsync()
    {
        const float lifeSeconds = 30f;
        const float fadeSeconds = 3f;
        int waitMs = Mathf.Max(0, (int)((lifeSeconds - fadeSeconds) * 1000));
        await UniTask.Delay(waitMs);

        if (!IsCurrentInstance() || !IsCurrentVersion() || _isFading) return;

        await FadeAndReleaseAsync(fadeSeconds);
    }
    void GrantLootAndFadeNow()
    {
        if (!IsCurrentInstance() || !IsCurrentVersion()) return;
        // 1) 몬스터에 설정된 드롭테이블 ID (예: DT_BTL_NOR)
        string dtId = ctr.mdata?.Monster_DT;
        if (string.IsNullOrEmpty(dtId))
        {
            Debug.LogWarning($"[MonsterKilledState] Monster_DT is empty on {ctr.name}");
            StartFadeNow();
            return;
        }

        // 2) 드롭테이블 시트에서 로드
        var dropTableDic = SheetDataUtil.DicByKey(Monster_Info.Monster_DropTable, x => x.DropTable_id);
        if (!dropTableDic.TryGetValue(dtId, out var dropRow))
        {
            Debug.LogWarning($"[MonsterKilledState] DropTable not found: {dtId}");
            StartFadeNow();
            return;
        }

        // 3) 파싱 & 굴림
        List<DropResolved> rolls = DropTableResolver.RollDrops(dropRow.DropTable_Info);

        // 4) 지급: ItemDataConstructor + AddInventory(data)
        var inv = UIPopUpHandler.Instance.GetScript<Inventory>(); // 프로젝트 인벤토리
        if (inv == null)
        {
            Debug.LogWarning("[MonsterKilledState] Inventory not found via UIPopUpHandler");
        }
        else
        {
            foreach (var r in rolls)
            {
                var data = ItemDataConstructor.GetItemData(r.itemId);
                if (data == null)
                {
                    Debug.LogWarning($"[MonsterKilledState] ItemData not found for id '{r.itemId}'");
                    continue;
                }
                inv.AddItem(data, r.count, true);
            }
        }

        // 지급 끝났으면 즉시 3초 페이드 시작
        StartFadeNow();
    }
    // 플레이어 파밍 완료 시 바로 호출
    public void StartFadeNow()
    {
        if (!IsCurrentInstance() || !IsCurrentVersion()) return;
        if (_isFading) return;
        FadeAndReleaseAsync(3f).Forget();
    }

    async UniTask FadeAndReleaseAsync(float fadeSeconds)
    {
        if (!IsCurrentInstance() || !IsCurrentVersion()) return;
        _isFading = true;
        s_despawning.Add(go);

        // 알파 서서히 0으로
        var srs = go.GetComponentsInChildren<SpriteRenderer>(true);
        var cache = new List<(SpriteRenderer sr, Color c)>(srs.Length);
        foreach (var sr in srs) if (sr) cache.Add((sr, sr.color));

        float t = 0f;
        while (t < fadeSeconds)
        {
            if (!IsCurrentInstance() || !IsCurrentVersion()) return;
            float a = Mathf.Lerp(1f, 0f, t / fadeSeconds);
            for (int i = 0; i < cache.Count; i++)
            {
                var (sr, c) = cache[i];
                if (sr) sr.color = new Color(c.r, c.g, c.b, a);
            }
            t += Time.deltaTime;
            await Cysharp.Threading.Tasks.UniTask.Yield();
        }
        for (int i = 0; i < cache.Count; i++)
        {
            var (sr, c) = cache[i];
            if (sr) sr.color = new Color(c.r, c.g, c.b, 0f);
        }

        if (!IsCurrentInstance() || !IsCurrentVersion()) return;
        // 정리
        s_despawning.Remove(go);
        s_instances.Remove(go);

        // 최종 반환 (기존 코드 유지)
        SpawnerPool.Instance.MonsterPool.Release(ctx.id, go);
    }
    void GoVanish()
    {
        ctx.animationHub?.SetTag(MonsterStateTag.Killed, ctx);
        VanishAndRelease().Forget();
    }

    async UniTaskVoid VanishAndRelease()
    {
        await UniTask.Delay(1000);
        SpawnerPool.Instance.MonsterPool.Release(ctx.id, go);
    }
    bool IsCurrentInstance()
    {
        return s_instances.TryGetValue(go, out var inst) && ReferenceEquals(inst, this);
    }

    bool IsCurrentVersion()
    {
        return s_versions.TryGetValue(go, out var cur) && cur == _ver;
    }
    public void Tick() { }
    public void Exit() { }
}
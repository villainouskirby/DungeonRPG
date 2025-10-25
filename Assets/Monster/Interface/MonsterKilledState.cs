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
    static readonly Dictionary<GameObject, int> s_harvestsLeft = new();
    int _ver = 0;
    bool _killedSfxPlayed;
    readonly MonsterContext ctx;
    readonly MonsterController ctr;
    readonly MonsterStateMachine root;
    readonly GameObject go;
    public static bool IsDespawning(GameObject go) => s_despawning.Contains(go);
    public static int HarvestsLeft(GameObject go) // 외부에서 갈무리 남은 횟수 조회용
    {
        if (go != null && s_harvestsLeft.TryGetValue(go, out var left)) return left;
        return 0;
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
        s_despawning.Remove(go);
        s_instances.Remove(go);
        s_harvestsLeft.Remove(go);
    }
    public MonsterKilledState(MonsterContext c, MonsterStateMachine m, GameObject go, MonsterController mc)
    { ctx = c; root = m; this.go = go; ctr = mc; }

    public void Enter()
    {
        if (!_killedSfxPlayed)
        {
            string sfxName = null;
            switch (ctx.data.category)
            {
                case MonsterData.MonsterCategory.Cleaner:
                    sfxName = "SFX_CleanerDie";
                    break;
                case MonsterData.MonsterCategory.Hound:
                    sfxName = "SFX_HoundDie";
                    break;
                case MonsterData.MonsterCategory.Beetle:
                    sfxName = "SFX_BettleDie";
                    break;
                case MonsterData.MonsterCategory.Titan:
                    sfxName = "SFX_TitanDie";
                    break;
                default:
                    sfxName = "SFX_GenericDie";
                    break;
            }

            if (!string.IsNullOrEmpty(sfxName))
            {
                _killedSfxPlayed = true;
                // 3D 원샷: 몬스터 위치에서, 루프=false
                SoundManager.Instance.PlaySound3D(
                    sfxName,
                    ctx.transform,
                    0f,
                    false,
                    SoundType.SFX,
                    true,
                    1.5f,
                    25f
                );
            }
        }
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

        s_harvestsLeft[go] = 2;

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

    public static void OnFarmHarvest(GameObject go)
    {
        if (go == null) return;
        if (!s_instances.TryGetValue(go, out var inst)) return;
        if (s_despawning.Contains(go)) return;
        inst.TryHarvestOnce();  // 인스턴스 실행
    }
    void TryHarvestOnce()
    {
        if (!IsCurrentInstance() || !IsCurrentVersion() || _isFading) return;
        if (!s_harvestsLeft.TryGetValue(go, out var left) || left <= 0) return;

        GrantLootOnce();           // 아이템 1회 지급
        left -= 1;
        s_harvestsLeft[go] = left;

        if (left <= 0)
        {
            // 남은 횟수 없으면 3초 페이드 시작
            StartFadeNow();
        }
    }
    void GrantLootOnce()
    {
        // 1) 드롭테이블 ID
        string dtId = ctr.mdata?.Monster_DT;
        if (string.IsNullOrEmpty(dtId))
        {
            Debug.LogWarning($"[MonsterKilledState] Monster_DT is empty on {ctr.name}");
            return;
        }

        // 2) 드롭테이블 로드
        var dropTableDic = SheetDataUtil.DicByKey(Monster_Info.Monster_DropTable, x => x.DropTable_id);
        if (!dropTableDic.TryGetValue(dtId, out var dropRow))
        {
            Debug.LogWarning($"[MonsterKilledState] DropTable not found: {dtId}");
            return;
        }

        // 3) 파싱 & 굴림
        List<DropResolved> rolls = DropTableResolver.RollDrops(dropRow.DropTable_Info);

        // 4) 지급
        var inv = UIPopUpHandler.Instance.GetScript<Inventory>();
        if (inv == null)
        {
            Debug.LogWarning("[MonsterKilledState] Inventory not found via UIPopUpHandler");
            return;
        }
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
        s_harvestsLeft.Remove(go);

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
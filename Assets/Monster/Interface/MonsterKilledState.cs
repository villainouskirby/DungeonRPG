using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DBUtility;
using UnityEngine;
using UnityEngine.AI;

public sealed class MonsterKilledState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterController ctr;
    readonly MonsterStateMachine root;
    readonly GameObject go;

    public MonsterKilledState(MonsterContext c, MonsterStateMachine m, GameObject go, MonsterController mc)
    { ctx = c; root = m; this.go = go; ctr = mc; }

    public void Enter()
    {
        ctx.indicator?.Show(MonsterStateTag.Killed);
        ctx.SafeStopAgent();
        ctx.anim?.Play("Die");

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

                // AddInventory(data) 시그니처에 맞춰 개수만큼 반복 추가
                for (int i = 0; i < r.count; i++)
                    inv.AddItem(data, r.count, true);
            }
        }

        GoVanish();
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

    public void Tick() { }
    public void Exit() { }
}
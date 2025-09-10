using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
public sealed class MonsterKilledState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterController ctr;
    readonly MonsterStateMachine root;
    readonly GameObject go;
    Sprite sprite;
    string dt;
    public Monster_Info_Monster_DropTable dropData;
    public Dictionary<string, Item_Info_Item> ItemDic;
    public Dictionary<string, Monster_Info_Monster_DropTable> monsterDropDic;
    public MonsterKilledState(MonsterContext c, MonsterStateMachine m, GameObject go, MonsterController mc)
    { ctx = c; root = m; this.go = go; ctr = mc; }

    public void Enter()
    {
        ctx.agent.isStopped = true;
        ctx.anim.Play("Die");


        // 드롭테이블 로드
        dt = ctr.mdata.Monster_DT;
        monsterDropDic = SheetDataUtil.DicByKey(Monster_Info.Monster_DropTable, x => x.DropTable_id);
        if (!monsterDropDic.TryGetValue(dt, out dropData))
        {
            Debug.LogWarning($"[MonsterKilledState] DropTable not found: {dt}");
            Cleanup();
            return;
        }

        // 드랍 테이블 파싱
        var drops = DropTableResolver.RollDrops(dropData.DropTable_Info);
        ItemDic = SheetDataUtil.DicByKey(Item_Info.Item, x => x.id);

        // 인벤토리에 추가
        foreach (var d in drops)
        {
            if (!ItemDic.TryGetValue(d.itemId, out var itemInfo))
            {
                Debug.LogWarning($"[MonsterKilledState] Unknown item id: {d.itemId}");
                continue;
            }

            for (int i = 0; i < d.count; i++)
            {
                ThrowItemData itemData = new(ItemDic["ITM_MIN_ROC"], sprite, "PAR_MIN_ROC");
                UIPopUpHandler.Instance.InventoryScript.AddItem(itemData);
            }
        }

        Cleanup();
    }
    void Cleanup()
    {
        SpawnerPool.Instance.MonsterPool.Release(ctx.id, go);
    }
    public void Tick() { }
    public void Exit() { }
}
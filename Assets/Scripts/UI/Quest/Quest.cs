using Events;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Quest : UIBase
{
    [SerializeField] private QuestUI _questUI;
    [SerializeField] private Inventory _inventory;

    private List<QuestInfo> _questInfos = new();

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
        EventManager.Instance.QuestCompleteEvent.AddListener(QuestClear);
    }

    private void OnDestroy()
    {
        EventManager.Instance.QuestCompleteEvent.RemoveListener(QuestClear);
    }

    public void AddQuest(QuestInfo info)
    {
        if (_questInfos.Exists(e => e.Info.id == info.Info.id))
        {
            Debug.LogError("이미 존재하는 퀘스트");
            return;
        }

        using (var args = QuestAllocateEventArgs.Get())
        {
            args.Init(info.Info.id, info.Info.targetNPC);
            EventManager.Instance.QuestAllocateEvent.Invoke(args);
            args.Release();
        }

        _questInfos.Add(info);
        UpdateSlot(_questInfos.Count - 1);
    }

    private void UpdateSlot(int index)
    {
        _questUI.SetQuestSlot(index, _questInfos[index]);
    }
    
    public QuestInfo GetQuestInfo(string id)
    {
        return _questInfos.Find(info => info.Info.id == id);
    }

    public QuestInfo GetQuestInfo(int index)
    {
        return (_questInfos.Count > index && index >= 0) ? _questInfos[index] : null;
    }

    public void RemoveQuest(int index)
    {
        QuestInfo info = GetQuestInfo(index);
        if (info == null) return;

        info.Dispose();

        _questInfos.RemoveAt(index);
        _questUI.RemoveSlot(index);
    }

    public void QuestClear(int index)
    {
        QuestInfo info = GetQuestInfo(index);
        if (info == null) return;

        using (var args = QuestUnlockedEventArgs.Get())
        {
            var newInfo = Array.Find(Quest_Info.Quest, quest => quest.id == info.Info.unlock_id);

            if (newInfo == null) return;

            args.Init(newInfo.id, newInfo.npc);
            EventManager.Instance.QuestUnlockedEvent.Invoke(args);
            args.Release();
        }

        for (int i = 0; i < info.Rewards.Length; i++)
        {
            if (info.Rewards[i] != null)
            {
                Item item = info.Rewards[i];

                if (item is CountableItem ci)
                {
                    _inventory.AddItemForce(ci.Data, ci.Amount);
                }
                else
                {
                    _inventory.AddItemForce(item.Data);
                }
            }
        }

        RemoveQuest(index);
    }

    public QuestSlotUI GetQuestSlotUI(string id)
    {
        return _questUI.GetQuestSlotUI(_questInfos.FindIndex(e => e.Info.id == id));
    }
    
    public void QuestClear(QuestCompleteEventArgs args)
    {
        QuestClear(_questInfos.FindIndex(info => info.Info.id == args.QuestID));
    }
}

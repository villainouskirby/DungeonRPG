using System.Collections.Generic;
using UnityEngine;

public class Quest : UIBase
{
    [SerializeField] private QuestUI _questUI;
    [SerializeField] private Inventory _inventory;

    private List<QuestInfo> _questInfos = new List<QuestInfo>();

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
    }

    public void AddQuest(QuestInfo info)
    {
        _questInfos.Add(info);
        UpdateSlot(_questInfos.Count - 1);
    }

    private void UpdateSlot(int index)
    {
        _questUI.SetQuestSlot(index, _questInfos[index]);
    }

    public QuestInfo GetQuestInfo(int index)
    {
        return (_questInfos.Count > index && index >= 0) ? _questInfos[index] : null;
    }

    public void RemoveQuest(int index)
    {
        _questInfos.RemoveAt(index);
        _questUI.RemoveSlot(index);
    }

    public void QuestClear(int index)
    {
        QuestInfo info = _questInfos[index];
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

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

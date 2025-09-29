using Events;

[System.Serializable]
public class QuestInfo
{
    public Quest_Info_Quest Info => _info;
    private Quest_Info_Quest _info;

    public Mission[] Missions = new Mission[3];

    public Item[] Rewards = new Item[3];

    public bool IsQuestCleared = false;

    public QuestInfo(Quest_Info_Quest info)
    {
        _info = info;
    }

    ~QuestInfo()
    {
        foreach (var mission in Missions)
        {
            mission.UnRegisterProcess();
        }
    }

    public void CheckQuestClear()
    {
        bool isClear = Missions[0].IsMissionCleared && Missions[1].IsMissionCleared && Missions[2].IsMissionCleared;

        if (isClear != IsQuestCleared)
        {
            IsQuestCleared = isClear;

            using (var args = QuestClearEventArgs.Get())
            {
                args.Init(_info.id, isClear);
                EventManager.Instance.QuestClearEvent.Invoke(args);
                args.Release();
            }
        }
    }
}

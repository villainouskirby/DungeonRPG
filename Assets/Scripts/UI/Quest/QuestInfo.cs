using UnityEngine;

[System.Serializable]
public class QuestInfo
{
    public Quest_Info_Quest Info => _info;
    private Quest_Info_Quest _info;

    public Mission[] Missions = new Mission[3];

    public Item[] Rewards = new Item[3];

    public bool IsQuestCleared => Missions[0].IsMissionCleared && Missions[1].IsMissionCleared && Missions[2].IsMissionCleared;

    public QuestInfo(Quest_Info_Quest info)
    {
        _info = info;
    }

    ~QuestInfo()
    {
        foreach (var mission  in Missions)
        {
            mission.UnRegisterProcess();
        }
    }
}

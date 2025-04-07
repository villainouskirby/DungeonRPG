using UnityEngine;

[System.Serializable]
public class QuestInfo
{
    public bool IsMainQuest;
    public Mission[] Missions = new Mission[3];

    [Multiline]
    public string QuestDescription;

    public Item[] Rewards = new Item[3];

    public bool IsQuestCleared => Missions[0].IsMissionCleared && Missions[1].IsMissionCleared && Missions[2].IsMissionCleared;
}

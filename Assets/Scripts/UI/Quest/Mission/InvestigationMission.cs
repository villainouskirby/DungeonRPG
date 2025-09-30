using Events;
using System;
using EventArgs = Events.EventArgs;

public class InvestigationMission : Mission
{
    public Quest_Info_Investigation InvestigationInfo => _investigationInfo;
    private Quest_Info_Investigation _investigationInfo;

    public override void Init(string questID)
    {
        ID = questID;

        _investigationInfo = Array.Find(Quest_Info.Investigation, info => info.id == questID);

        Progress = 0;
        MaxProgress = 1;
    }

    public override string GetExplanation()
    {
        return "";
    }

    public override void RegisterProcess()
    {
        switch (_investigationInfo.type)
        {
            case "Conversation":
                EventManager.Instance.NPCConversationEvent.AddListener(UpdateProgress);
                break;
        }
    }

    public override void UnRegisterProcess()
    {
        switch (_investigationInfo.type)
        {
            case "Conversation":
                EventManager.Instance.NPCConversationEvent.RemoveListener(UpdateProgress);
                break;
        }
    }

    public override void UpdateProgress(EventArgs eventArgs)
    {
        Progress++;

        if (CheckIsMissionCleared())
        {
            UnRegisterProcess();
        }
    }
}
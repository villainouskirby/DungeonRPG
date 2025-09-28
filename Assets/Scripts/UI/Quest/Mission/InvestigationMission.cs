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
    }

    public override void RegisterProcess()
    {
    }

    public override void UnRegisterProcess()
    {
    }

    public override void UpdateProgress(EventArgs eventArgs)
    {
    }
}
using Events;
using System;
using EventArgs = Events.EventArgs;

public class GatheringMission : Mission
{
    public Quest_Info_Gathering GatheringInfo => _gatheringInfo;
    private Quest_Info_Gathering _gatheringInfo;

    public override void Init(string questID)
    {
        ID = questID;

        _gatheringInfo = Array.Find(Quest_Info.Gathering, info => info.id == questID);
    }

    public override void RegisterProcess()
    {
    }

    public override void UnregisterProcess()
    {
    }

    public override void UpdateProgress(EventArgs args)
    {
    }
}
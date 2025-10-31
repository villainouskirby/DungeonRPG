using Events;
using System;
using EventArgs = Events.EventArgs;

public class HuntingMission : Mission
{
    public Quest_Info_Hunting HuntingInfo => _huntingInfo;
    private Quest_Info_Hunting _huntingInfo;

    public override void Init(string questID)
    {
        ID = questID;

        _huntingInfo = Array.Find(Quest_Info.Hunting, info => info.id == questID);

        Progress = 0;
        MaxProgress = _huntingInfo.count;
    }

    public override string GetExplanation()
    {
        return HuntingInfo.Goal;
    }

    public override void RegisterProcess()
    {
        EventManager.Instance.MonsterKilledEvent.AddListener(UpdateProgress);
    }

    public override void UnRegisterProcess()
    {
        EventManager.Instance.MonsterKilledEvent.RemoveListener(UpdateProgress);
    }

    public override void UpdateProgress(EventArgs eventArgs)
    {
        if ((eventArgs as MonsterKilledEventArgs).MonsterID == _huntingInfo.object_id)
        {
            Progress++;

            if (CheckIsMissionCleared())
            {
                UnRegisterProcess();
            }
        }
    }
}

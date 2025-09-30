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

        Progress = UIPopUpHandler.Instance.GetScript<Inventory>().GetItemAmountSum(_gatheringInfo.object_id);
        MaxProgress = _gatheringInfo.count;
    }

    public override string GetExplanation()
    {
        return "";
    }

    public override void RegisterProcess()
    {
        EventManager.Instance.InventoryChangedEvent.AddListener(UpdateProgress);
    }

    public override void UnRegisterProcess()
    {
        EventManager.Instance.InventoryChangedEvent.RemoveListener(UpdateProgress);
    }

    public override void UpdateProgress(EventArgs eventArgs)
    {
        var args = eventArgs as InventoryChangedEventArgs;

        if (args.ItemData.SID == _gatheringInfo.object_id)
        {
            Progress += args.Amount;
        }

        CheckIsMissionCleared();
    }
}
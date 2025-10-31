using Events;
using System;
using EventArgs = Events.EventArgs;

public class InvestigationMission : Mission
{
    public Quest_Info_Investigation InvestigationInfo => _investigationInfo;
    private Quest_Info_Investigation _investigationInfo;

    private InventoryBehaviorEventArgs.Behavior _inventoryBehavior;

    public override void Init(string questID)
    {
        ID = questID;

        _investigationInfo = Array.Find(Quest_Info.Investigation, info => info.id == questID);

        Progress = 0;
        MaxProgress = 1;

        switch (_investigationInfo.type)
        {
            case "Inventory_drink":
                _inventoryBehavior = InventoryBehaviorEventArgs.Behavior.Drink;
                break;

            case "Inventory_register":
                _inventoryBehavior = InventoryBehaviorEventArgs.Behavior.Register;
                break;

            case "Inventory_equip":
                _inventoryBehavior = InventoryBehaviorEventArgs.Behavior.Equip;
                break;

            default:
                _inventoryBehavior = InventoryBehaviorEventArgs.Behavior.None;
                break;
        }

        if (_investigationInfo.type == "Go")
        {
            Progress = 1;
        }
    }

    public override string GetExplanation()
    {
        return InvestigationInfo.Goal;
    }

    public override void RegisterProcess()
    {
        switch (_investigationInfo.type)
        {
            case "Conversation":
                EventManager.Instance.NPCConversationEvent.AddListener(UpdateProgress);
                break;

            case "Inventory_drink":
            case "Inventory_register":
            case "Inventory_equip":
                EventManager.Instance.InventoryBehaviorEvent.AddListener(UpdateProgress);
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

            case "Inventory_drink":
            case "Inventory_register":
            case "Inventory_equip":
                EventManager.Instance.InventoryBehaviorEvent.RemoveListener(UpdateProgress);
                break;
        }
    }

    private bool CheckID(string id)
    {
        return id == InvestigationInfo.object_id;
    }

    private bool CheckBehavior(InventoryBehaviorEventArgs args)
    {
        if (args.InventoryBehavior == _inventoryBehavior || CheckID(args.ItemID))
        {
            return true;
        }

        return false;
    }

    public override void UpdateProgress(EventArgs eventArgs)
    {
        switch (_investigationInfo.type)
        {
            case "Conversation":
                if (CheckID((eventArgs as NPCConversationEventArgs).NPCName))
                {
                    Progress++;
                }

                break;

            case "Inventory_drink":
            case "Inventory_register":
            case "Inventory_equip":
                if (CheckBehavior(eventArgs as InventoryBehaviorEventArgs))
                {
                    Progress++;
                }

                break;
        }

        if (CheckIsMissionCleared())
        {
            UnRegisterProcess();
        }
    }
}
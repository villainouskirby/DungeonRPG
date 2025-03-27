using UnityEngine;

public class EquipmentPopUpUI : ItemPopUpUI
{
    [SerializeField] private EquipmentUI _equipmentUI;

    public void OpenInfo(EquipmentType type)
    {
        ItemData data = _equipmentUI.GetItemData(type);
        if (data == null)
        {
            CloseInfo();
            return;
        }

        base.OpenInfo(data);
    }
}

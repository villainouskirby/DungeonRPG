using UnityEngine;

public class RemovePopUpUI : AmountSelectPopUpUI
{
    [Header("Inventory UI")]
    [SerializeField] private InventoryUI _inventoryUI;


    public override void SetItemData(int index)
    {
        _popUpName.text = "Remove";
        ItemData itemData = _inventoryUI.GetItemData(index);
        SetItemData(index, itemData.IconSprite, itemData.Name, _inventoryUI.GetItemAmount(index));
    }

    protected override void ExecuteConfirm()
    {
        if (_inputAmount == _maxInputAmount) _inventoryUI.RemoveItem(_selectedIndex);
        else _inventoryUI.RemoveItem(_selectedIndex, _inputAmount);
    }
}

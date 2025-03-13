using UnityEngine;

public class SmithSlotUI : SlotUI
{
    [SerializeField] private GameObject _deactivePanel;

    public void SetItemInfo(Sprite sprite, bool isActive)
    {
        SetItemInfo(sprite);
        _deactivePanel.SetActive(isActive);
    }
}

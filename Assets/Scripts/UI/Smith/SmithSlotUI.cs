using UnityEngine;

public class SmithSlotUI : SlotUI
{
    [SerializeField] private GameObject _deactivePanel;

    private void Awake()
    {
        if (_deactivePanel == null) _deactivePanel = transform.GetChild(0).gameObject;
    }

    public void SetItemInfo(Sprite sprite, bool isActive)
    {
        SetItemInfo(sprite);
        _deactivePanel.SetActive(isActive);
    }
}

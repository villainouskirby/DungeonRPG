using TMPro;
using UnityEngine;

public class RegionGuideSlotUI : SlotUI
{
    [SerializeField] private TextMeshProUGUI _nameText;

    public void SetItemInfo(Sprite sprite, string name)
    {
        SetItemInfo(sprite);
        _nameText.text = name;
    }
}

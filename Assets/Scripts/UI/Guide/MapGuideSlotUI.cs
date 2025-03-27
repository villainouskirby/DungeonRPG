using TMPro;
using UnityEngine;

public class MapGuideSlotUI : SlotUI
{
    [SerializeField] private TextMeshProUGUI _nameText;

    public void SetItemInfo(Sprite sprite, string name)
    {
        SetItemInfo(sprite);
        _nameText.text = name;
    }
}

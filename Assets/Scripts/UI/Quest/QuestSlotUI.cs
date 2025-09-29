using TMPro;
using UnityEngine;

public class QuestSlotUI : SlotUI
{
    [SerializeField] private TextMeshProUGUI _questNameText;

    public void SetItemInfo(Sprite sprite, string name)
    {
        SetItemInfo(sprite);
        _questNameText.text = name;
    }
}
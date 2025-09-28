using TMPro;
using UnityEngine;

public class QuestSlotUI : SlotUI
{
    [SerializeField] private TextMeshProUGUI _progressText;


    public void SetItemInfo(Sprite sprite, string name)
    {
        SetItemInfo(sprite);
        _progressText.text = name;
    }
}
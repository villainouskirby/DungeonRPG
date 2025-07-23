using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SaveSlot : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _slotNameText;
    [SerializeField]
    private TMP_Text _dateText;
    [SerializeField]
    private TMP_Text _inGameDateText;
    [SerializeField]
    private TMP_Text _questTypeText;
    [SerializeField]
    private TMP_Text _questInfoText;
    [SerializeField]
    private TMP_Text _locationText;

    public void Set(SaveSlotData data)
    {
        _slotNameText.text = data.SlotName;
        _dateText.text = data.Date;
        _inGameDateText.text = $"{data.InGameDate}Day";
        _questTypeText.text = data.QuestType;
        _questInfoText.text = data.QuestInfo;
        _locationText.text = data.Location;
    }
}

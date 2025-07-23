using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveFileManager : MonoBehaviour
{
    public Transform SlotRoot;
    public SaveSlot[] SaveSlots;

    public SaveSlotData DefaultSlotData;

    [ContextMenu("Set SaveSlot")]
    public void SetSlotEditor()
    {
        SaveSlots = new SaveSlot[SlotRoot.childCount];
        for (int i = 0; i < SlotRoot.childCount; i++)
            SaveSlots[i] = SlotRoot.GetChild(i).GetComponent<SaveSlot>();
    }

    public void Open()
    {
        RefreshSlots();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < SaveManager.SaveFileData.ManualSaveSlot.Length; i++)
        {
            if (SaveManager.SaveFileData.ManualSaveSlot[i].Exist)
                SaveSlots[i].Set(SaveManager.SaveFileData.ManualSaveSlot[i]);
            else
                SetDefault(SaveSlots[i], SaveManager.SaveFileData.ManualSaveSlot[i], i);
        }
    }

    private void SetDefault(SaveSlot saveSlot, SaveSlotData data, int index)
    {
        data.SlotName = index switch
        {
            (int)SaveSlotIndex.Auto1 => "자동1",
            (int)SaveSlotIndex.Auto2 => "자동2",
            (int)SaveSlotIndex.Auto3 => "자동3",
            _ => "없음"
        };

        saveSlot.Set(data);
    }
}

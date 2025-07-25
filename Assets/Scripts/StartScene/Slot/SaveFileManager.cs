using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveFileManager : MonoBehaviour
{
    public Transform SlotRoot;
    public SaveSlot[] SaveSlots;
    public SaveSlot SelectSlot;
    public GameObject SelectUI;
    public GameObject UnSelectUI;
    public ScrollRect ScrollRect;

    public SaveSlotData DefaultSlotData;
    public SaveFileMode Mode;

    public enum SaveFileMode
    {
        Save,
        Load
    }

    [ContextMenu("Set SaveSlot")]
    public void SetSlotEditor()
    {
        SaveSlots = new SaveSlot[SlotRoot.childCount];
        for (int i = 0; i < SlotRoot.childCount; i++)
            SaveSlots[i] = SlotRoot.GetChild(i).GetComponent<SaveSlot>();
    }

    public void SetSelectSlot(SaveSlot slot)
    {
        bool flag = true;
        switch (Mode)
        {
            case SaveFileMode.Load:
                if (slot.Exist)
                    flag = true;
                else
                    flag = false;
                break;
            case SaveFileMode.Save:
                SaveSlotIndex[] auto = new SaveSlotIndex[3] { SaveSlotIndex.Auto1, SaveSlotIndex.Auto2, SaveSlotIndex.Auto3 };
                if (!auto.Contains(slot.Index))
                    flag = true;
                else
                    flag = false;
                break;
        }

        if (flag)
        {
            SelectSlot?.OffOutline();
            SelectSlot = slot;
            SelectSlot?.OnOutline();
        }
        else
        {
            SelectSlot?.OffOutline();
            SelectSlot = null;
        }

        RefreshSelectUI();
    }

    public void RefreshSelectUI()
    {
        if (SelectSlot == null)
        {
            SelectUI?.SetActive(false);
            UnSelectUI?.SetActive(true);
        }
        else
        {
            SelectUI?.SetActive(true);
            UnSelectUI?.SetActive(false);
        }
    }

    public void Open()
    {
        Canvas.ForceUpdateCanvases();
        ScrollRect.verticalNormalizedPosition = 1f;
        RefreshSlots();
        RefreshSelectUI();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        SelectSlot?.OffOutline();
        SelectSlot = null;
        gameObject.SetActive(false);
    }

    public void Save()
    {
        SaveManager.SaveSlot(SelectSlot.Index, "test1");
        RefreshSlots();
    }

    public void Load()
    {
        SaveManager.SaveSlotIndex = SelectSlot.Index;
        TileMapMaster.IsLoad = true;
        SceneManager.LoadScene("testDohyun");
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < SaveManager.SaveFileData.ManualSaveSlot.Length; i++)
        {
            SaveSlots[i].SaveFileManager = this;
            if (SaveManager.SaveFileData.ManualSaveSlot[i].Exist)
                SaveSlots[i].Set(SaveManager.SaveFileData.ManualSaveSlot[i], true);
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

        saveSlot.Set(data, false);
    }
}

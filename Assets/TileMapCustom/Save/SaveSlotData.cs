using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveFileData
{
    public SaveSlotData[] ManualSaveSlot;

    public SaveFileData()
    {
        ManualSaveSlot = new SaveSlotData[8];

        for (int i = 0; i < (int)SaveSlotIndex.End; i++)
            ManualSaveSlot[i] = new((SaveSlotIndex)i);
    }
}

public class SaveSlotData
{
    public SaveSlotIndex Index;
    public string SlotName;
    public string Date;
    public string QuestType;
    public string QuestInfo;
    public int InGameDate;
    public string Location;
    public bool Exist;

    public SaveSlotData()
    {
        Index = SaveSlotIndex.None;
        SlotName = "";
        Exist = false;
    }

    public SaveSlotData(SaveSlotIndex index)
    {
        Index = index;
        SlotName = "";
        Exist = false;
    }

    public SaveSlotData(SaveSlotIndex index, string slotName, Vector3 location, bool exist)
    {
        Index = index;
        SlotName = slotName;
        Date = DateTime.Now.ToString("yy/MM/dd - hh:mm:ss");
        QuestType = "임시";
        QuestInfo = "임시 퀘스트입니다";
        InGameDate = TimeSystemUpdate.Day;
        Location = location.ToString();
        Exist = exist;
    }
}

public enum SaveSlotIndex
{
    None = -1,
    Auto1, Auto2, Auto3,
    Manual1, Manual2, Manual3, Manual4, Manual5,
    End
}
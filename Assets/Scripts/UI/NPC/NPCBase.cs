using UnityEngine;
using Events;
using System.Collections.Generic;
using System;
using DBUtility;

public class NPCBase<T> : UIBase, ISave where T : UIBase
{
    [SerializeField] protected string _npcName = "";

    //protected bool _isQuestRunning = false;
    protected bool _isQuestClear = false;
    protected Queue<string> _questID = new(); // 우선순위 큐로 수정해야함
    protected Queue<string> _allocatedQuestID = new(); // 퀘스트별로 성공 여부 저장해야할듯

    public void OpenUI() => UIPopUpHandler.Instance.ToggleUI<T>();

    protected override void InitBase()
    {
        EventManager.Instance.QuestAllocateEvent.AddListener(AllocateQuest);
        EventManager.Instance.QuestUnlockedEvent.AddListener(UnlockQuest);
        EventManager.Instance.QuestClearEvent.AddListener(SetQuestClear);
        EventManager.Instance.QuestCompleteEvent.AddListener(RemoveQuest);
    }

    private void OnDestroy()
    {
        EventManager.Instance.QuestAllocateEvent.RemoveListener(AllocateQuest);
        EventManager.Instance.QuestUnlockedEvent.RemoveListener(UnlockQuest);
        EventManager.Instance.QuestClearEvent.RemoveListener(SetQuestClear);
        EventManager.Instance.QuestCompleteEvent.RemoveListener(RemoveQuest);
    }

    public void StartTalk()
    {
        using (var args = NPCConversationEventArgs.Get())
        {
            args.Init(_npcName);
            EventManager.Instance.NPCConversationEvent.Invoke(args);
            args.Release();
        }

        bool isQuestCleared = _isQuestClear;
        bool isQuestExist = _questID.TryPeek(out var id);

        if (isQuestCleared)
        {
            CompleteQuest();
        }

        DialogueRunner runner = UIPopUpHandler.Instance.GetScript<DialogueRunner>();
        runner.Init(OpenUI, _npcName);

        if (isQuestExist)
        {
            var quest = UIPopUpHandler.Instance.GetScript<Quest>();
            var info = quest.GetQuestInfo(id);
            string dialogueID;

            if (info == null)
            {
                dialogueID = QuestConstructor.GetRawQuestInfo(id).start_text;
                runner.StartQuest(dialogueID, id);
            }
            else
            {
                dialogueID = info.Info.start_text + "-ing";
                runner.StartPrint(dialogueID).Forget();
            }

            _isQuestClear = false;
        }
        else if (isQuestCleared)
        {
            runner.StartPrint(id + "-end").Forget();
        }
        else
        {
            runner.StartPrint(_npcName).Forget();
        }
    }

    public void CompleteQuest()
    {
        if (_allocatedQuestID.Count <= 0) return;

        var id = _allocatedQuestID.Dequeue();

        using (var args = QuestCompleteEventArgs.Get())
        {
            args.Init(id, Array.Find(Quest_Info.Quest, quest => quest.id == id).npc);
            EventManager.Instance.QuestCompleteEvent.Invoke(args);
            args.Release();
        }

        _isQuestClear = false;
    }

    public void UnlockQuest(QuestUnlockedEventArgs args)
    {
        if (args.NPCName == _npcName)
        {
            _questID.Enqueue(args.QuestID);
        }
    }

    public void AllocateQuest(QuestAllocateEventArgs args)
    {
        if (args.TargetNPC == _npcName)
        {
            _allocatedQuestID.Enqueue(args.QuestID);
        }
    }

    public void SetQuestClear(QuestClearEventArgs args)
    {
        if (_npcName != args.TargetNPC) return;

        if (_allocatedQuestID.TryPeek(out var id))
        {
            if (args.QuestID == id)
            {
                _isQuestClear = args.IsClear;
            }
        }
    }

    public void RemoveQuest(QuestCompleteEventArgs args)
    {
        if (args.NPC == _npcName)
        {
            _questID.Dequeue();
        }
    }

    public void Load(SaveData saveData)
    {

    }

    public void Save(SaveData saveData)
    {

    }
}
using UnityEngine;
using Events;
using System.Collections.Generic;
using DBUtility;

public class NPCBase<T> : UIBase, ISave where T : UIBase
{
    [SerializeField] protected string _npcName = "";

    //protected bool _isQuestRunning = false;
    protected bool _isQuestClear = false;
    protected Queue<string> _questID = new(); // 우선순위 큐로 수정해야함

    public void OpenUI() => UIPopUpHandler.Instance.ToggleUI<T>();

    protected override void InitBase()
    {
        EventManager.Instance.QuestUnlockedEvent.AddListener(UnlockQuest);
        EventManager.Instance.QuestClearEvent.AddListener(SetQuestClear);
    }

    private void OnDestroy()
    {
        EventManager.Instance.QuestUnlockedEvent.RemoveListener(UnlockQuest);
        EventManager.Instance.QuestClearEvent.RemoveListener(SetQuestClear);
    }

    public void StartTalk()
    {
        using (var args = NPCConversationEventArgs.Get())
        {
            args.Init(_npcName);
            EventManager.Instance.NPCConversationEvent.Invoke(args);
            args.Release();
        }

        if (_isQuestClear)
        {
            CompleteQuest();
        }

        DialogueRunner runner = UIPopUpHandler.Instance.GetScript<DialogueRunner>();
        runner.Init(OpenUI, _npcName);

        if (_questID.TryPeek(out var id))
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
        else
        {
            runner.StartPrint(_npcName).Forget();
        }
    }

    public void UnlockQuest(QuestUnlockedEventArgs args)
    {
        if (args.NPCName == _npcName)
        {
            _questID.Enqueue(args.QuestID);
        }
    }

    public void SetQuestClear(QuestClearEventArgs args)
    {
        if (_questID.TryPeek(out var id))
        {
            if (args.QuestID == id)
            {
                _isQuestClear = args.IsClear;
            }
        }
    }

    public void CompleteQuest()
    {
        if (_questID.Count <= 0) return;

        UIPopUpHandler.Instance.GetScript<Quest>().QuestClear(_questID.Dequeue());
        _isQuestClear = false;
    }

    public void Load(SaveData saveData)
    {

    }

    public void Save(SaveData saveData)
    {

    }
}
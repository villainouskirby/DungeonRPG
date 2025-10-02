using UnityEngine;
using Events;
using System.Collections.Generic;

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
            runner.StartPrint(UIPopUpHandler.Instance.GetScript<Quest>().GetQuestInfo(id).Info.start_text).Forget();
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
        UIPopUpHandler.Instance.GetScript<Quest>().QuestClear(_questID.Dequeue());
    }

    public void Load(SaveData saveData)
    {

    }

    public void Save(SaveData saveData)
    {

    }
}
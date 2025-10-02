using UnityEngine;
using Events;
using System.Collections.Generic;

public class NPCBase<T> : UIBase, ISave where T : UIBase
{
    [SerializeField] protected string _npcName = "";

    protected Queue<string> _questID = new(); // 우선순위 큐로 수정해야함

    public void OpenUI() => UIPopUpHandler.Instance.ToggleUI<T>();

    protected override void InitBase()
    {
        EventManager.Instance.QuestUnlockedEvent.AddListener(UnlockQuest);
        
    }

    public void StartTalk()
    {
        using (var args = NPCConversationEventArgs.Get())
        {
            args.Init(_npcName);
            EventManager.Instance.NPCConversationEvent.Invoke(args);
            args.Release();
        }

        DialogueRunner runner = UIPopUpHandler.Instance.GetScript<DialogueRunner>();
        runner.Init(OpenUI, _npcName);

        if (_questID.TryPeek(out var id))
        {
            runner.StartPrint(id).Forget();
        }
    }

    public void UnlockQuest(QuestUnlockedEventArgs args)
    {
        if (args.NPCName == _npcName)
        {
            _questID.Enqueue(args.QuestID);
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
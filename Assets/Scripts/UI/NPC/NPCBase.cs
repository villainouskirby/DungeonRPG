using UnityEngine;
using UnityEngine.UI;
using Events;

public abstract class NPCBase<T> : UIBase where T : UIBase
{
    [SerializeField] private Button _talkButton;

    protected string _npcName = "";
    protected string _questID = "";
    protected bool _isQuestAccepted = false;

    protected override void InitBase()
    {
        _talkButton.onClick.AddListener(StartTalk);
        EventManager.Instance.QuestUnlockedEvent.AddListener(UnlockQuest);
    }

    private void StartTalk()
    {
        UIPopUpHandler.Instance.GetScript<DialogueRunner>().Init(_questID); // 이름 정형화 필요
    }

    public void UnlockQuest(QuestUnlockedEventArgs args)
    {
        if (args.NPCName == _npcName)
        {
            _questID = args.QuestID;
        }
    }

    public void CompleteQuest()
    {
        UIPopUpHandler.Instance.GetScript<Quest>().QuestClear(_questID);
        _questID = _npcName;
    }
}
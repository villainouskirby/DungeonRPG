using Events;
using UnityEngine;

public class QuestStateMark : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _npcSprite;
    [SerializeField] private SpriteRenderer _markSprite;
    [SerializeField] private IT_NpcFunc _npc;
    [SerializeField] private float _positionOffset = 0.2f;

    [Header("Sprites")]
    [SerializeField] private Sprite _questionMark;
    [SerializeField] private Sprite _exclamationMark;
    [SerializeField] private Sprite _dotMark;

    private void Start()
    {
        transform.position = new Vector3(_npcSprite.bounds.center.x, _npcSprite.bounds.max.y + _positionOffset);

        EventManager.Instance.QuestAllocateEvent.AddListener(SetWaitMark);
        EventManager.Instance.QuestUnlockedEvent.AddListener(ShowQuestMark);
        EventManager.Instance.QuestClearEvent.AddListener(SetClearMark);
        EventManager.Instance.QuestCompleteEvent.AddListener(CloseQuestMark);

        if (_npc.NpcName != NPC.merchant)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        EventManager.Instance.QuestAllocateEvent.RemoveListener(SetWaitMark);
        EventManager.Instance.QuestUnlockedEvent.RemoveListener(ShowQuestMark);
        EventManager.Instance.QuestClearEvent.RemoveListener(SetClearMark);
        EventManager.Instance.QuestCompleteEvent.RemoveListener(CloseQuestMark);
    }

    public void ShowQuestMark(QuestUnlockedEventArgs args)
    {
        if (args.NPCName != _npc.NpcName.ToString()) return;

        _markSprite.sprite = _exclamationMark;
        gameObject.SetActive(true);
    }

    public void CloseQuestMark(QuestCompleteEventArgs args)
    {
        if (args.NPC != _npc.NpcName.ToString()) return;

        gameObject.SetActive(false);
    }

    public void SetWaitMark(QuestAllocateEventArgs args)
    {
        if (args.TargetNPC != _npc.NpcName.ToString()) return;

        _markSprite.sprite = _dotMark;
        gameObject.SetActive(true);
    }

    public void SetClearMark(QuestClearEventArgs args)
    {
        if (args.TargetNPC != _npc.NpcName.ToString()) return;

        if (args.IsClear)
        {
            _markSprite.sprite = _questionMark;
        }
        else
        {
            _markSprite.sprite = _dotMark;
        }

        gameObject.SetActive(true);
    }
}

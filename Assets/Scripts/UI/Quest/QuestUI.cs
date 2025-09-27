using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : SlotInteractHandler
{
    [SerializeField] private Quest _quest;

    [SerializeField] private GameObject _questSlotPrefab;
    [SerializeField] private Transform _questContent;

    [SerializeField] private TextMeshProUGUI[] _missionTexts;
    [SerializeField] private TextMeshProUGUI _questContentText;
    [SerializeField] private Image[] _rewardImages;

    [SerializeField] private Button _acceptButton;
    [SerializeField] private TextMeshProUGUI _acceptButtonText;

    private List<QuestSlotUI> _questSlots = new List<QuestSlotUI>();
    private int _currentSelectedIndex;

    private void Start()
    {
        InitQuest();
    }

    private void InitQuest()
    {
        _currentSelectedIndex = -1;

        _acceptButton.onClick.RemoveAllListeners();
        _acceptButton.onClick.AddListener(AcceptQuest);
    }

    private void CreateSlot()
    {
        QuestSlotUI slotUI;
        GameObject newSlot = Instantiate(_questSlotPrefab, _questContent);
        slotUI = newSlot.GetComponent<QuestSlotUI>();
        if (slotUI == null)
        {
            Debug.Log("프리팹 오류! 퀘스트 슬롯 프리팹에 컴포넌트가 빠져있음");
            return;
        }
        _questSlots.Add(slotUI);
    }

    public void SetQuestSlot(int index, QuestInfo info)
    {
        if (index == _questSlots.Count)
        {
            CreateSlot();
        }

        Mission firstMission = info.Missions[0];
        //_questSlots[index].SetItemInfo(firstMission.Sprite, firstMission.Type, firstMission.Progress, firstMission.MaxProgress, info.IsMainQuest);
    }

    public void RemoveSlot(int index)
    {
        Destroy(_questSlots[index].gameObject);
        _questSlots.RemoveAt(index);
    }

    public void SetQuestInfo(int index)
    {
        _currentSelectedIndex = index;

        QuestInfo info = _quest.GetQuestInfo(index);
        if (info == null) return;

        // 임무내용 표시
        for (int i = 0; i < info.Missions.Length; i++)
        {
            Mission mission = info.Missions[i];
            if (mission == null)
            {
                _missionTexts[i].text = "";
                continue;
            }

            /*
            _missionTexts[i].text = mission.Content;
            if (mission.Type != QuestType.researching)
                _missionTexts[i].text += $"({mission.Progress}/{mission.MaxProgress})";*/
        }

        // 퀘스트 내용 표시
        _questContentText.text = info.QuestDescription;

        // 보상 내용 표시
        for (int i = 0; i < info.Rewards.Length; i++)
        {
            if (info.Rewards[i] == null)
            {
                _rewardImages[i].sprite = null; // 기본 비어있는 칸으로 하거나 투명하게 둬야할듯
            }
            else
            {
                _rewardImages[i].sprite = info.Rewards[i].Data.IconSprite;
            }
        }

        // 퀘스트 확인 버튼
        if (info.IsQuestCleared)
        {
            _acceptButtonText.text = "complete";
        }
        else
        {
            _acceptButtonText.text = "accept";
        }
    }

    private void AcceptQuest()
    {
        if (_currentSelectedIndex == -1) return; // -1은 아직 선택하지 않은 상태

        QuestInfo info = _quest.GetQuestInfo(_currentSelectedIndex);
        if (info.IsQuestCleared)
        {
            _quest.QuestClear(_currentSelectedIndex);
            Debug.Log("퀘스트 클리어 됨");
        }
        else
        {
            _acceptButtonText.text = "accepted";
            Debug.Log("퀘스트 수락");
        }
    }

    private int GetQuestSlotIndex(QuestSlotUI slot)
    {
        return _questSlots.IndexOf(slot);
    }

    #region Pointer Event

    public override void OnDoubleClick()
    {
    }

    public override void OnLeftClick()
    {
        SetQuestInfo(GetQuestSlotIndex(_pointedSlot as QuestSlotUI));
    }

    public override void OnPointerIn()
    {  
    }

    public override void OnPointerOut()
    {
    }

    public override void OnRightClick()
    {
    }

    #endregion
}

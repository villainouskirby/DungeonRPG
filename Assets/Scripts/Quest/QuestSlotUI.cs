using TMPro;
using UnityEngine;

public class QuestSlotUI : SlotUI
{
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private GameObject _mainQuestMark;

    public void SetItemInfo(Sprite sprite, QuestType type, int progress, int maxProgress, bool isMainQuest)
    {
        SetItemInfo(sprite);

        if (type == QuestType.researching)
        {
            _progressText.text = "";
        }
        else
        {
            _progressText.text = $"{progress}/{maxProgress}";
        }

        if (isMainQuest)
        {
            _mainQuestMark.SetActive(true);
        }
        else
        {
            _mainQuestMark.SetActive(false);
        }
    }
}

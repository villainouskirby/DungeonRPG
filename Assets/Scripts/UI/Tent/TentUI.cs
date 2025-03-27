using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TentUI : MonoBehaviour
{
    [Header("Time Text")]
    [SerializeField] private TextMeshProUGUI _currentTimeText;
    [SerializeField] private TextMeshProUGUI _wakeUpTimeText;
    [SerializeField] private TextMeshProUGUI _timeDiffText;

    [Header("Button")]
    [SerializeField] private Button _sleepButton;

    // Time => 0 -> 00:00, 1 -> 00:30, 2 -> 01:00, ... , 24 -> 12:00

    /// <summary> 시간 차 </summary>
    private int _timeDiff;

    /// <summary> 현재 시간 </summary>
    private int _currentTime;

    /// <summary> 일어나는 시간 </summary>
    private int _wakeUpTime => _currentTime + _timeDiff;

    public void AdjustTime(int time)
    {
        _timeDiff = Mathf.Clamp(_timeDiff + time, 1, 48);
        _wakeUpTimeText.text = GetTimeText(_wakeUpTime);
        SetTimeDiffText();

        // 체력 조정 수치 보여주는거 필요하면 넣기
    }

    private void Awake()
    {
        _sleepButton.onClick.AddListener(SleepAndHeal);
    }

    private void OnEnable()
    {
        _currentTime = (int)TimeSystemUpdate.Time / 30 % 48;
        _currentTimeText.text = GetTimeText(_currentTime);

        _timeDiff = 1;
        _wakeUpTimeText.text = GetTimeText(_wakeUpTime);
        SetTimeDiffText();
    }

    /// <summary> 취침 </summary>
    private void SleepAndHeal()
    {
        int healPercent = _timeDiff * 10; // TODO => 플레이어와 연동시켜 체력회복시키기
        // TODO => 시간 시스템에서 자는것을 어떻게 더할지에 대한 그런거 필요할듯
    }

    private void SetTimeDiffText()
    {
        string timeDiffText = "";
        if (_timeDiff > 1) timeDiffText += (_timeDiff / 2).ToString() + "hour ";
        if (_timeDiff % 2 != 0) timeDiffText += "30minute "; // 어차피 30분 말고 보여줄거 없음
        _timeDiffText.text = timeDiffText.Substring(0, timeDiffText.Length - 1);
    }

    private string GetTimeNumber(int time) => ((time / 10 == 0) ? "0" : "") + time.ToString();
    private string GetTimeText(int time) => $"{GetTimeNumber(time / 2)} : {GetTimeNumber(time % 2 * 30)}";
}

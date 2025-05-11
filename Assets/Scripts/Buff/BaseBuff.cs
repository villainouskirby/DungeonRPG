using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class BaseBuff : MonoBehaviour
{
    [Header("UI 레퍼런스")]
    [SerializeField] public Image icon;   // 버프 아이콘 자체 (ex: Skill 아이콘)
    [SerializeField] private TextMeshProUGUI timerTMP;   // 남은 시간 표시(선택)
    private PlayerData playerData;
    private float duration;   // 버프 지속 시간(초)
    private float currentTime; // 남은 시간
    private bool reverted = false;
    private BuffType buffType; 
    private float percentage;
    public void Awake()
    {
        playerData = FindObjectOfType<PlayerData>();
        icon = GetComponent<Image>();
        if (!timerTMP) timerTMP = GetComponentInChildren<TextMeshProUGUI>(true);
    }
    /// <summary>
    /// BuffManager에서 버프를 생성할 때 호출함
    /// </summary>
    public void Init(BuffType type, float percentage, float duration)
    {
        this.buffType = type;
        this.percentage = percentage;
        this.duration = duration;
        this.currentTime = duration;
        icon.fillAmount = 1;


        // 스탯에 버프 적용
        playerData.ApplyBuff(type, percentage);

        // 코루틴 시작 (0.1초마다 남은 시간을 감소시키는 방식)
        StartCoroutine(Activation());
    }

    private IEnumerator Activation()
    {
        while (currentTime > 0)
        {
            currentTime -= 0.1f;
            icon.fillAmount = currentTime / duration;
            UpdateTimerText();
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
        icon.fillAmount = 0;
        Revert();

        // 아이콘 오브젝트 제거
        Destroy(gameObject);
    }
    private void UpdateTimerText()
    {
        if (!timerTMP) return;              // 텍스트 UI가 없다면 무시

        if (currentTime > 60f)              // 1 분 초과 ➜ 분:초 표기
        {
            int m = Mathf.FloorToInt(currentTime / 60f);
            int s = Mathf.FloorToInt(currentTime % 60f);
            timerTMP.text = $"{m}:{s:00}";
        }
        else                                // 1 분 이하 ➜ 초만 표기
        {
            int s = Mathf.CeilToInt(currentTime);
            timerTMP.text = $"{s}s";
        }
    }

    private void OnDestroy() => Revert();
    private void OnApplicationQuit() => Revert();   // 빌드·에디터 공통
    private void OnDisable() => Revert();
    private void Revert()
    {
        if (reverted) return;          // 두 번 이상 호출 방지
        playerData.RemoveBuff(buffType, percentage);
        reverted = true;
    }
}
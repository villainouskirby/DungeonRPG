using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BuffImage : MonoBehaviour
{
    [Header("UI 레퍼런스")]
    public Image icon;   // 버프 아이콘 자체 (ex: Skill 아이콘)

    private float duration;   // 버프 지속 시간(초)
    private float currentTime; // 남은 시간

    private BuffType buffType; 
    private float percentage;
    public void Awake()
    {
        icon = GetComponent<Image>();
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
        PlayerData.instance.ApplyBuff(type, percentage);

        // 코루틴 시작 (0.1초마다 남은 시간을 감소시키는 방식)
        StartCoroutine(Activation());
    }

    private IEnumerator Activation()
    {
        while (currentTime > 0)
        {
            currentTime -= 0.1f;
            icon.fillAmount = currentTime / duration;
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
        icon.fillAmount = 0;

        PlayerData.instance.RemoveBuff(buffType, percentage);

        // 아이콘 오브젝트 제거
        Destroy(gameObject);
    }
}
/*
// EX
public enum BuffType
{
    AttackUp = 0,
    AttackDown = 1,
}

public class BaseBuff : MonoBehaviour
{
    //버프 아이콘 인스펙터에 붙일 것
    public BuffType Type; // 이런식으로? string으로 하면 결국 오타나 실수 나오는 부분이 생겨서
    // 나중에 종류 많아지는거 생각하면 이 방향이 좋을거같네요
    public string type; // 요거는 편의성 문제긴한데 Enum으로 빼고 사용하면 편할거같아요
    public float percentage;
    public float duration;
    public float currenttime;
    public Image icon;
    public void Awake()
    {
        icon = GetComponent<Image>();
    }
    public void Init(string type, float per, float du)
    {
        this.type = type;
        percentage = per;
        duration = du;
        currenttime = duration;
        icon.fillAmount = 1; // 버프 표기 방식에 따라 바꿀 수 있음
        Execute();
    }
    public void Execute()
    {
        PlayerData.instance.onBuff.Add(this);
        PlayerData.instance.ChooseBuff(type);
        StartCoroutine(Activation());
    }

    IEnumerator Activation()
    {
        while (currenttime > 0)
        {
            currenttime -= 0.1f;
            icon.fillAmount = currenttime/duration;
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
        icon.fillAmount = 0;
        currenttime = 0;
        DeActivation();
    }
    public void DeActivation()
    {
        PlayerData.instance.onBuff.Remove(this);
        PlayerData.instance.ChooseBuff(type);
        Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
*/
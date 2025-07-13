using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


public class PotionManager : MonoBehaviour
{
    public static PotionManager instance;

    [Header("버프 아이콘 프리팹")]
    public GameObject buffPrefab;
    [Header("포션 마시는 시간")]
    public float DRINK_DURATION = 2f;

    PlayerController player;
    private void Awake()
    {
        instance = this;
    }
    void Start() => player = FindObjectOfType<PlayerController>();

    private bool isDrinking = false;

    public async UniTask<bool> GetPotionID(ItemData data)
    {
        if (isDrinking || player == null) return false;
        isDrinking = true;

        PotionItemData pi = data as PotionItemData;
        bool success = await Drink();

        if (success)
        {
            // ID임시수정 주석처리
            /*
            if (pi.SID <= 10)
                CreateBuff(pi.ID, pi.Percentage, pi.Duration, pi.IconSprite);
            else if (pi.SID <= 20)
                PlayerData.instance.HPValueChange(pi.Healamount);
            */
        }

        isDrinking = false;
        return success;
    }
    private void Update()
    {

    }
    private async UniTask<bool> Drink()
    {
        if (player == null) return false;

        player.LockState();
        PlayerData.instance.StartPotionGauge(DRINK_DURATION);

        float endTime = Time.time + DRINK_DURATION;

        while (Time.time < endTime)
        {
            // 피격감지 => 피격시 false 반환
            await UniTask.NextFrame();
        }
        // 잠금해제

        PlayerData.instance.EndPotionGauge();
        player.UnlockState();

        return true;
    }

    // 버프 아이콘 생성
    public void CreateBuff(int buffID, float percentage, float duration, Sprite icon)
    {
        BuffType type = buffID switch
        {
            1 => BuffType.AttackUp,
            2 => BuffType.AttackDown,
            _ => BuffType.AttackUp
        };
        GameObject go = Instantiate(buffPrefab, transform);
        go.GetComponent<Image>().sprite = icon;
        BaseBuff buffImage = go.GetComponent<BaseBuff>();
        buffImage.Init(type, percentage, duration);
    }
}
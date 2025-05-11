using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


public class PotionManager : MonoBehaviour
{
    public static PotionManager instance;

    [Header("버프 아이콘 프리팹")]
    public GameObject buffPrefab;

    private void Awake()
    {
        instance = this;
    }

    private bool isDrinking = false;

    public async UniTask<bool> GetPotionID(ItemData data)
    {
        if (isDrinking) return false;
        isDrinking = true;

        PotionItemData piData = data as PotionItemData;
        
        if (!await Drink()) return false;

        if (piData.ID <= 10)
        {
            CreateBuff(piData.ID, piData.Percentage, piData.Duration, piData.IconSprite);
        }
        else if (piData.ID <= 20) 
        {
            PlayerData.instance.HPValueChange(piData.Healamount);
        }

        isDrinking = false;

        return true;
    }
    private void Update()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        player.UpdatePotionChargeGauge(player.PotionChargeRatio);
    }
    private async UniTask<bool> Drink()
    {
        float startTime = Time.time;

        // 상태 변화 잠금
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null) return false;
        player.LockState();
        player.StartPotionGuage();
        while (Time.time - startTime < 2)
        {
            // 피격감지 => 피격시 false 반환

            player.CancelPotionGuage();
            await UniTask.NextFrame();
        }
        // 잠금해제

        player.EndPotionGuage();
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
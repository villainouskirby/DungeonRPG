using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;


public class PotionManager : MonoBehaviour
{
    public static PotionManager instance;

    [Header("버프 아이콘 프리팹")]
    public GameObject buffPrefab;
    [Header("포션 마시는 시간")]
    public float DRINK_DURATION = 2f;

    PlayerController player;
    AttackController attackController;
    public event Action<float> OnGaugeStart;                  // duration
    public event Action<float, float, float> OnGaugeProgress; // elapsed, duration, ratio
    public event Action OnGaugeEnd;
    public bool IsDrinking { get; private set; } = false;
    public float CurrentDrinkDuration { get; private set; } = 0f;
    public float CurrentDrinkStart { get; private set; } = 0f;
    private void Awake()
    {
        instance = this;
    }
    void Start() 
    {
        player = FindObjectOfType<PlayerController>();
        attackController = FindObjectOfType<AttackController>();
    }

    private bool isDrinking = false;

    static Dictionary<string, Item_Info_Potion> _potionById;
    void EnsurePotionTable()
    {
        if (_potionById == null)
            _potionById = SheetDataUtil.DicByKey(Item_Info.Potion, p => p.id); // "PAR_POT_001" 키
    }

    public async UniTask<bool> GetPotionID(ItemData data)
    {
        if (isDrinking || player == null) return false;
        isDrinking = true;

        EnsurePotionTable();

        PotionItemData pi = data as PotionItemData;
        if (pi == null) { isDrinking = false; return false; }

        string dt = pi.SID;         // 예: "PAR_POT_001"
        if (string.IsNullOrEmpty(dt) || !_potionById.TryGetValue(dt, out var row))
        {
            Debug.LogError($"Potion DT not found: {dt}");
            isDrinking = false;
            return false;
        }

        bool success = await Drink();

        if (success)
        {
            switch (row.type) // "heal" | "add" | "remove"
            {
                case "heal":
                    // 회복: row.effect를 회복량으로 사용
                    PlayerData.instance.HPValueChange(row.effect);
                    break;

                case "add":
                    // 버프 부여: row.buff = "strong1/strong2" 등
                    foreach (var kind in ParseKinds(row.buff))
                    {
                        int buffId = MapBuffId(kind);     // strong1→1, strong2→2, strong3→3 …
                        float duration = pi.EffectDuration > 0 ? pi.EffectDuration : 10f;
                        CreateBuff(buffId, row.effect, duration, pi.IconSprite);
                    }
                    break;

                case "remove":
                    foreach (var kind in ParseKinds(row.buff))
                        RemoveBuff(kind); // 필요에 맞게 구현
                    break;

                default:
                    Debug.LogWarning($"Unknown potion type: {row.type}");
                    break;
            }
        }

        isDrinking = false;
        return success;
    }
    private void Update()
    {

    }
    private async UniTask<bool> Drink()
    {
        if (player == null && attackController == null) return false;

        player.LockState();
        attackController.LockAttack();
        float duration = DRINK_DURATION;
        float start = Time.time;
        float endTime = start + duration;
        IsDrinking = true;
        CurrentDrinkDuration = duration;
        CurrentDrinkStart = start;

        // 게이지 시작 알림
        OnGaugeStart?.Invoke(duration);


        while (Time.time < endTime)
        {
            float elapsed = Time.time - start;
            float ratio = Mathf.Clamp01(elapsed / duration);

            // 진행 알림
            OnGaugeProgress?.Invoke(elapsed, duration, ratio);

            // 피격 등으로 취소 로직이 있다면 여기서 검사 후 break/return
            await UniTask.NextFrame();
        }

        // 잠금해제

        OnGaugeEnd?.Invoke();
        IsDrinking = false;
        CurrentDrinkDuration = 0f;
        CurrentDrinkStart = 0f;
        player.UnlockState();
        attackController.UnLockAttack();
        return true;
    }
    IEnumerable<string> ParseKinds(string buff)
    {
        if (string.IsNullOrWhiteSpace(buff)) yield break;
        foreach (var k in buff.Split('/'))
            if (!string.IsNullOrWhiteSpace(k)) yield return k.Trim();
    }

    int MapBuffId(string kind)
    {
        return kind switch
        {
            "strong1" => 1, // CreateBuff 내부 switch: 1 => AttackUp 등
            "strong2" => 2,
            "strong3" => 3,
            _ => 1
        };
    }
    void RemoveBuff(string kind)
    {
        // 예: BuffManager.Instance.Remove(MapBuffType(kind));
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
using System.Threading;
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
    private CancellationTokenSource _hotCts;
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

        bool success = false;

        if (row.type == "heal")
        {
            // 회복은 마시는 동안에만 진행
            int amount = Mathf.RoundToInt(row.effect);
            success = await DrinkHeal(amount, DRINK_DURATION);
        }
        else
        {
            success = await Drink();

            if (success)
            {
                switch (row.type)
                {
                    case "add":
                        foreach (var kind in ParseKinds(row.buff))
                        {
                            int buffId = MapBuffId(kind);
                            float duration = pi.EffectDuration > 0 ? pi.EffectDuration : 10f;
                            CreateBuff(buffId, row.effect, duration, pi.IconSprite);
                        }
                        break;

                    case "remove":
                        foreach (var kind in ParseKinds(row.buff))
                            RemoveBuff(kind);
                        break;

                    default:
                        Debug.LogWarning($"Unknown potion type: {row.type}");
                        break;
                }
            }
        }

        isDrinking = false;
        return success;
    }
    private bool _cancelRequested;
    public void RequestCancelDrink() => _cancelRequested = true;
    private async UniTask<bool> Drink()
    {
        if (player == null && attackController == null) return false;

        attackController.LockAttack();

        float duration = DRINK_DURATION;
        float start = Time.time;
        float endTime = start + duration;

        _cancelRequested = false;

        player.ChangeState(new PotionConsumeState(player, duration));
        // 게이지 시작 알림
        OnGaugeStart?.Invoke(duration);


        while (Time.time < endTime)
        {
            if (_cancelRequested)
            {
                OnGaugeEnd?.Invoke();
                IsDrinking = false;
                CurrentDrinkDuration = 0f;
                CurrentDrinkStart = 0f;
                attackController.UnLockAttack();
                return false;          // 취소: 효과 미적용
            }
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
        attackController.UnLockAttack();
        return true;
    }
    private async UniTask<bool> DrinkHeal(int totalAmount, float duration)
    {
        if (player == null) return false;

        totalAmount = Mathf.Max(0, totalAmount);
        duration = Mathf.Max(0.01f, duration);

        // 상태 진입 (포션 섭취 상태: 이동 가능/입력해제 시 즉시정지 등은 기존 상태머신 로직 활용)
        player.ChangeState(new PotionConsumeState(player, duration));

        _cancelRequested = false;

        float start = Time.time;
        float endTime = start + duration;

        OnGaugeStart?.Invoke(duration);

        if (totalAmount == 0)
        {
            OnGaugeEnd?.Invoke();
            return true;
        }

        // 초당 회복량
        float ratePerSec = totalAmount / duration;
        float applied = 0f; // 누적 적용량(소수 포함)

        while (Time.time < endTime)
        {
            if (_cancelRequested)
            {
                OnGaugeEnd?.Invoke();
                return false; // 취소됨
            }

            float elapsed = Time.time - start;
            float ratio = Mathf.Clamp01(elapsed / duration);
            OnGaugeProgress?.Invoke(elapsed, duration, ratio);

            float delta = ratePerSec * Time.deltaTime;

            // 다음 틱으로 넘어가며 MaxHP 클램프는 PlayerData.HPValueChange에서 처리된다고 가정
            PlayerData.instance.HPValueChange(delta);
            applied += delta;

            await UniTask.NextFrame();
        }

        // 루프 종료(정상 완료): 반올림 오차 보정
        float remainder = Mathf.Max(0f, totalAmount - applied);
        if (remainder > 0f)
            PlayerData.instance.HPValueChange(remainder);

        OnGaugeEnd?.Invoke();
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
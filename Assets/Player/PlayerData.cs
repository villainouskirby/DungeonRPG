using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;

    [Header("Base Stats (기본 스탯 값)")]
    [SerializeField] private float baseAttack = 10f;
    [SerializeField] private float baseSpeed = 10f;

    [Header("Max Stats (스탯 최대 값)")]
    [SerializeField] private FloatVariableSO MaxHP;
    [SerializeField] private FloatVariableSO MaxStamina;
    public event System.Action<float, float> OnHPChanged;
    public event System.Action<float> OnStunRequested;
    [Header("Current Stats (게임 중 변동)")]
    [SerializeField] private FloatVariableSO currentAtk;
    [SerializeField] private FloatVariableSO currentSpeed;
    [SerializeField] public FloatVariableSO currentHP;
    [SerializeField] public FloatVariableSO currentStamina;

    [Header("스테미나 리젠 속도")]
    [SerializeField] private float StaminaSpeed = 2f;
    [Header("무방비 상태 지속시간")]
    [SerializeField] private float exhaustedDuration = 2f;  // 스테미너 0 → 무방비 2초
    public bool IsExhausted { get; private set; } = false;
    Coroutine exhaustCo;

    [Header("Sprint")]
    [SerializeField] private float runCostPerSec = 5f;          // 달리기 1초당 소모
    [SerializeField] private float chargeCostPerSec = 15f;
    [SerializeField] private float sprintResumeThreshold = 10f; // 재시작 가능 최소 스태미나
    [SerializeField] private float exhaustRegenBlockSec = 1.5f; // 바닥난 직후 리젠 금지 시간
    public bool SprintLocked { get; private set; } = false;

    //스테미나 필드
    int regenBlockCount = 0;
    public bool isStaminaBlocked => regenBlockCount > 0;
    void AddRegenBlock() { regenBlockCount++; }
    void RemoveRegenBlock() { regenBlockCount = Mathf.Max(0, regenBlockCount - 1); }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        StartCoroutine(StaminaRegen());
    }

    private void Update()
    {
        if (currentHP.Value > MaxHP.Value)
        {
            currentHP.Value = MaxHP.Value;
        }
        if (currentStamina.Value > MaxStamina.Value)
        {
            currentStamina.Value = currentStamina.Value;
        }
        TryUnlockSprint();
    }
    public void HPValueChange(float value)
    {
        float old = currentHP.Value;
        currentHP.Value += value;

        if (currentHP.Value > MaxHP.Value) currentHP.Value = MaxHP.Value;
        if (currentHP.Value < 0f) currentHP.Value = 0f;

        if (Mathf.Abs(currentHP.Value - old) > Mathf.Epsilon)
            OnHPChanged?.Invoke(old, currentHP.Value);
    }
    public void PlayerStun(float stunDuration)
    {
        OnStunRequested?.Invoke(stunDuration);
    }
    #region 스태미나
    public void StaminaValueChange(float value)
    {
        currentStamina.Value += value;
    }
    public bool SpendStamina(float amount)
    {
        if (IsExhausted) return false;
        if (currentStamina.Value < amount) return false;
        currentStamina.Value -= amount;
        return true;
    }
    public void ForceExhaustToZero()
    {
        if (currentStamina.Value > 0f) currentStamina.Value = 0f;
        EnterExhaust();
    }
    private IEnumerator StaminaRegen()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        while (true)
        {
            if (!isStaminaBlocked)
            {
                currentStamina.Value += StaminaSpeed;

                if (currentStamina.Value > MaxStamina.Value)
                    currentStamina.Value = MaxStamina.Value;
            }

            yield return wait;
        }
    }
    public void BlockStaminaRegen(float seconds)
    {
        StartCoroutine(BlockRegenFor(seconds));
    }
    IEnumerator BlockRegenFor(float sec)
    {
        AddRegenBlock();
        yield return new WaitForSeconds(sec);
        RemoveRegenBlock();
    }


    // 달리기 시작 가능 여부 (MoveState → RunState 진입 체크용)
    public bool CanStartSprint()
    {
        return !IsExhausted && !SprintLocked && currentStamina.Value >= sprintResumeThreshold;
    }

    // 락 해제 규칙: Shift를 “떼고” + 스태미나가 일정 이상
    private void TryUnlockSprint()
    {
        if (SprintLocked)
        {
            if (!Input.GetKey(KeyCode.LeftShift) && currentStamina.Value >= sprintResumeThreshold)
                SprintLocked = false;
        }
    }
    // 액션형 소비: 행동 "종료 시점"에 호출
    // - stamina < cost였어도 모션은 이미 정상 출력됨 → 여기서 0 처리 + 무방비 진입
    // - stamina >= cost면 정상 차감
    public bool ConsumeActionStamina(float cost, bool allowDebt = true)
    {
        if (IsExhausted) return false;                 // 무방비면 액션 자체가 불가

        if (currentStamina.Value >= cost)
        {
            currentStamina.Value -= cost;              // 정상 차감
            BlockStaminaRegen(1f);
            return true;
        }

        if (allowDebt)                                  // 빚 허용: 0으로 만들고 무방비
        {
            currentStamina.Value = 0f;
            EnterExhaust();                             // 2초 무방비 + 리젠금지
            return true;                                // 모션은 이미 끝났으므로 true
        }

        return false;
    }
    public bool ConsumeComboAttackStamina(float cost, bool allowDebt = true)
    {
        if (IsExhausted) return false;                 // 무방비면 액션 자체가 불가

        if (currentStamina.Value >= cost)
        {
            currentStamina.Value -= cost;              // 정상 차감
            BlockStaminaRegen(1.5f);
            return true;
        }

        if (allowDebt)                                  // 빚 허용: 0으로 만들고 무방비
        {
            currentStamina.Value = 0f;
            EnterExhaust();                             // 2초 무방비 + 리젠금지
            return true;                                // 모션은 이미 끝났으므로 true
        }

        return false;
    }
    // 달리기 “지속 소모” (RunState에서 매 프레임 호출)
    public bool TryConsumeSprintThisFrame(float dt)
    {
        if (IsExhausted || SprintLocked) return false;

        float need = runCostPerSec * dt;
        if (currentStamina.Value <= need)
        {
            currentStamina.Value = 0f;
            EnterExhaust();                             // 0 → 무방비 진입
            return false;                               // 더 이상 달릴 수 없음
        }

        currentStamina.Value -= need;
        BlockStaminaRegen(1f);
        return true;
    }
    // 차징 어택 스테미나 소모
    public bool TryConsumeChargeThisFrame(float dt)
    {
        if (IsExhausted || SprintLocked) return false;

        float need = chargeCostPerSec * dt;
        if (currentStamina.Value <= need)
        {
            currentStamina.Value = 0f;
            EnterExhaust();                             // 0 → 무방비 진입
            return false;                               // 더 이상 달릴 수 없음
        }

        currentStamina.Value -= need;
        BlockStaminaRegen(1f);
        return true;
    }
    void EnterExhaust()
    {
        StopCoroutine(nameof(ExhaustRoutine));
        StartCoroutine(ExhaustRoutine());
    }

    IEnumerator ExhaustRoutine()
    {
        IsExhausted = true;
        AddRegenBlock();                       // 무방비 동안 리젠 금지
        yield return new WaitForSeconds(exhaustedDuration);
        RemoveRegenBlock();
        IsExhausted = false;
    }
    #endregion
    #region 포션
    // 포션 게이지 UI
    //public void StartPotionGauge(float durationSec)
    //{
    //    potionDuration = durationSec;
    //    potionChargeStart = Time.time;
    //    isPotionCharging = true;

    //    chargeUI.ShowPotionGauge();
    //}
    //public void CancelPotionGauge()
    //{
    //    isPotionCharging = false;
    //    chargeUI.HideAll();
    //}
    //public void EndPotionGauge()
    //{
    //    isPotionCharging = false;
    //    chargeUI.HideAll();
    //}
    // 버프가 생길 때 호출: 플레이어의 현재 스탯을 바로 변경
    public void ApplyBuff(BuffType type, float percentage)
    {
        switch (type)
        {
            case BuffType.AttackUp:
                currentAtk.Value += baseAttack * percentage;
                break;

            case BuffType.AttackDown:
                currentAtk.Value -= baseAttack * percentage;
                break;

            case BuffType.SpeedUp:
                currentSpeed.Value += baseSpeed * percentage;
                break;

            case BuffType.SpeedDown:
                currentSpeed.Value -= baseSpeed * percentage;
                break;
        }
    }
    // 버프가 끝날 때 호출: ApplyBuff에서 적용했던 값 반대로 적용하기
    public void RemoveBuff(BuffType type, float percentage)
    {
        // ApplyBuff와 반대로 돌려주면 됨
        ApplyBuff(Opposite(type), percentage);
    }

    private BuffType Opposite(BuffType t) => t switch
    {
        BuffType.AttackUp => BuffType.AttackDown,
        BuffType.AttackDown => BuffType.AttackUp,
        BuffType.SpeedUp => BuffType.SpeedDown,
        BuffType.SpeedDown => BuffType.SpeedUp,
        _ => t
    };
    #endregion
}
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

    [Header("Current Stats (게임 중 변동)")]
    [SerializeField] private FloatVariableSO currentAtk;
    [SerializeField] private FloatVariableSO currentSpeed;
    [SerializeField] public FloatVariableSO currentHP;
    [SerializeField] public FloatVariableSO currentStamina;

    [Header("스테미나 리젠 속도")]
    [SerializeField] private float StaminaSpeed = 2f;

    [Header("Sprint")]
    [SerializeField] private float runCostPerSec = 5f;          // 달리기 1초당 소모
    [SerializeField] private float sprintResumeThreshold = 10f; // 재시작 가능 최소 스태미나
    [SerializeField] private float exhaustRegenBlockSec = 1.5f; // 바닥난 직후 리젠 금지 시간
    public bool SprintLocked { get; private set; } = false;

    public bool isStaminaBlocked = false;

    [Header("포션 UI")]
    [SerializeField] private ChargeUIController chargeUI;
    private bool isPotionCharging = false;
    private float potionChargeStart;
    private float potionDuration = 2f;   // 기본값, StartPotionGauge 때 갱신

    public float PotionChargeRatio =>
        isPotionCharging ? Mathf.Clamp01((Time.time - potionChargeStart) / 2f) : 0f;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        StartCoroutine(StaminaRegen());
        if (chargeUI) chargeUI.HideAll();
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
        if (isPotionCharging)
            chargeUI.SetPotionRatio(PotionChargeRatio);
    }
    public void HPValueChange(float value)
    {
        currentHP.Value += value;
    }
    public void StaminaValueChange(float value)
    {
        currentStamina.Value += value;
    }
    public bool SpendStamina(float amount)
    {
        if (currentStamina.Value < amount) return false;
        currentStamina.Value -= amount;
        return true;
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
        StartCoroutine(BlockRegenCoroutine(seconds));
    }

    private IEnumerator BlockRegenCoroutine(float seconds)
    {
        isStaminaBlocked = true;
        yield return new WaitForSeconds(seconds);
        isStaminaBlocked = false;
    }
    // ▼ 달리기 “지속 소모” API (RunState에서 매 프레임 호출)
    public bool TryConsumeSprintThisFrame(float dt)
    {
        if (SprintLocked) return false;

        float need = runCostPerSec * dt;
        if (currentStamina.Value < need)
        {
            // 바닥 → 락 + 리젠 블락 + 0으로 고정
            currentStamina.Value = 0f;
            SprintLocked = true;
            BlockStaminaRegen(exhaustRegenBlockSec);
            return false;
        }

        currentStamina.Value -= need;
        return true;
    }

    // 달리기 시작 가능 여부 (MoveState → RunState 진입 체크용)
    public bool CanStartSprint()
    {
        return !SprintLocked && currentStamina.Value >= sprintResumeThreshold;
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
    // 포션 게이지 UI
    public void StartPotionGauge(float durationSec)
    {
        potionDuration = durationSec;
        potionChargeStart = Time.time;
        isPotionCharging = true;

        chargeUI.ShowPotionGauge();
    }
    public void CancelPotionGauge()
    {
        isPotionCharging = false;
        chargeUI.HideAll();
    }
    public void EndPotionGauge()
    {
        isPotionCharging = false;
        chargeUI.HideAll();
    }
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
}
using Core;
using UnityEngine;

/// <summary>
/// 플레이어 동작 전역 허브:
/// - 전투/행동 ON/OFF (가드, 공격, 회피, 강공격, 포션)
/// - 플레이어 생성 시 MaxHP=100 초기화(필요 시 CurrentHP도 채움)
/// </summary>
public class PlayerManager : Singleton<PlayerManager>
{
    [Header("Action Gates (전역 차단 스위치)")]
    [SerializeField] private bool allowAttack = true;
    [SerializeField] private bool allowHeavyAttack = true;
    [SerializeField] private bool allowGuard = true;
    [SerializeField] private bool allowDodge = true;
    [SerializeField] private bool allowPotion = true;

    public bool CanAttack => allowAttack;
    public bool CanHeavyAttack => allowHeavyAttack && allowAttack;
    public bool CanGuard => allowGuard;
    public bool CanDodge => allowDodge;
    public bool CanUsePotion => allowPotion;

    // 필요 시 외부에서 켜고 끄도록 공개 API 제공
    public void SetAttack(bool on) => allowAttack = on;
    public void SetHeavyAttack(bool on) => allowHeavyAttack = on;
    public void SetGuard(bool on) => allowGuard = on;
    public void SetDodge(bool on) => allowDodge = on;
    public void SetPotion(bool on) => allowPotion = on;

    // 플레이어 생성/로딩 직후 호출: MaxHP 100으로 초기화
    public void InitializePlayerStatsToDefaults()
    {
        if (PlayerData.Instance != null)
        {
            // PlayerData에 만든 public 메서드(아래 2-1 참고)
            PlayerData.Instance.ForceSetMaxHP(100f, fillCurrent: true);
        }
    }

    // 편의상 Start에서 한 번 호출(프리팹이 씬에 미리 존재하는 경우)
    void Start()
    {
        InitializePlayerStatsToDefaults();
    }
}
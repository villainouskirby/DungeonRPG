using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerSoundRange : MonoBehaviour, IPlayerChangeState
{
    public static PlayerSoundRange Instance { get; private set; }

    [Header("상태별 소음 반경(인스펙터에서 조절)")]
    [Tooltip("서 있는 상태(Idle)")]
    [SerializeField] private float idleRadius = 0f;

    [Tooltip("웅크리기(Sneak)")]
    [SerializeField] private float sneakRadius = 0f;

    [Tooltip("웅크린 이동(SneakMove)")]
    [SerializeField] private float sneakMoveRadius = 0f;

    [Tooltip("걷기(Move)")]
    [SerializeField] private float moveRadius = 7f;

    [Tooltip("차지 중(Charging)")]
    [SerializeField] private float chargingRadius = 7f;

    [Tooltip("평타(노멀 어택)")]
    [SerializeField] private float normalAttackRadius = 7f;

    [Tooltip("회피/구르기(Escape)")]
    [SerializeField] private float escapeRadius = 7f;

    [Tooltip("달리기(Run)")]
    [SerializeField] private float runRadius = 10f;

    [Tooltip("그 외 기타 상태(매칭 안 될 때)")]
    [SerializeField] private float otherRadius = 0f;
    private PlayerStateMachine stateMachine;
    private PlayerController pc;

    // 현재 상태가 내는 소음(반경) – 몬스터한테 공개되는 값
    public float NoiseRadius { get; private set; } = 0f;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        pc = GetComponent<PlayerController>();
        if (pc == null)
            Debug.LogError("[PlayerSoundRange] PlayerController 를 찾지 못했습니다.");
    }

    private void Update()
    {
        UpdateNoiseByState();
    }

    public void ChangeState(IPlayerState s) => stateMachine.ChangeState(s);
    public IPlayerState GetCurrentState() => stateMachine.GetCurrentState();
    public void RestorePreviousState() => stateMachine.RestorePreviousState();

    // 상태에 따른 소음 반경 설정
    private void UpdateNoiseByState()
    {
        var st = pc ? pc.GetCurrentState() : null;

        if (st is IdleState) NoiseRadius = idleRadius;
        else if (st is SneakState) NoiseRadius = sneakRadius;
        else if (st is SneakMoveState) NoiseRadius = sneakMoveRadius;
        else if (st is MoveState) NoiseRadius = moveRadius;
        else if (st is ChargingState) NoiseRadius = chargingRadius;
        else if (st is NormalAttackState) NoiseRadius = normalAttackRadius;
        else if (st is EscapeState) NoiseRadius = escapeRadius;
        else if (st is RunState) NoiseRadius = runRadius;
        else NoiseRadius = otherRadius;
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (Application.isPlaying == false) return;   // 에디터 정지-상태에서는 표시 안 함

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);   // 주황, 35% 투명
        Gizmos.DrawWireSphere(transform.position, NoiseRadius);
        Gizmos.DrawSphere(transform.position, 0.05f); // 중심점 표시(선택)
    }
#endif
}
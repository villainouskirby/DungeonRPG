using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerSoundRange : MonoBehaviour, IPlayerChangeState
{
    public static PlayerSoundRange Instance { get; private set; }

    [Header("상태별 소음 반경 / 강도 (인스펙터에서 조절)")]
    [Header("서 있는 상태(Idle)")]
    [SerializeField] private float idleRadius = 0f;
    [SerializeField] private int idleIntensity = 0;
    [Header("웅크리기(Sneak)")]
    [SerializeField] private float sneakRadius = 0f;
    [SerializeField] private int sneakIntensity = 1;

    [Header("웅크린 이동(SneakMove)")]
    [SerializeField] private float sneakMoveRadius = 0f;
    [SerializeField] private int sneakMoveIntensity = 1;
    [Header("걷기(Move)")]
    [SerializeField] private float moveRadius = 7f;
    [SerializeField] private int moveIntensity = 3;
    [Header("차지 중(Charging)")]
    [SerializeField] private float chargingRadius = 7f;
    [SerializeField] private int chargingIntensity = 3;
    [Header("평타(노멀 어택)")]
    [SerializeField] private float normalAttackRadius = 7f;
    [SerializeField] private int normalAttackIntensity = 5;
    [Header("회피/구르기(Escape)")]
    [SerializeField] private float escapeRadius = 7f;
    [SerializeField] private int escapeIntensity = 5;
    [Header("달리기(Run)")]
    [SerializeField] private float runRadius = 10f;
    [SerializeField] private int runIntensity = 5;
    [Header("그 외 기타 상태(매칭 안 될 때)")]
    [SerializeField] private float otherRadius = 0f;
    [SerializeField] private int otherIntensity = 0;
    private PlayerStateMachine stateMachine;
    private PlayerController pc;

    // 현재 상태가 내는 소음(반경) – 몬스터한테 공개되는 값
    public float NoiseRadius { get; private set; } = 0f;
    public int NoiseIntensity { get; private set; } = 0;

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

        if (st is IdleState) { NoiseRadius = idleRadius; NoiseIntensity = idleIntensity; }
        else if (st is SneakState) { NoiseRadius = sneakRadius; NoiseIntensity = sneakIntensity; }
        else if (st is SneakMoveState) { NoiseRadius = sneakMoveRadius; NoiseIntensity = sneakMoveIntensity; }
        else if (st is MoveState) { NoiseRadius = moveRadius; NoiseIntensity = moveIntensity; }
        else if (st is ChargingState) { NoiseRadius = chargingRadius; NoiseIntensity = chargingIntensity; }
        else if (st is NormalAttackState) { NoiseRadius = normalAttackRadius; NoiseIntensity = normalAttackIntensity; }
        else if (st is EscapeState) { NoiseRadius = escapeRadius; NoiseIntensity = escapeIntensity; }
        else if (st is RunState) { NoiseRadius = runRadius; NoiseIntensity = runIntensity; }
        else { NoiseRadius = otherRadius; NoiseIntensity = otherIntensity; }
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
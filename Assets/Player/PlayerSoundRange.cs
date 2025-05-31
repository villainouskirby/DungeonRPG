using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerSoundRange : MonoBehaviour, IPlayerChangeState
{
    public static PlayerSoundRange Instance { get; private set; }

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

        stateMachine = new PlayerStateMachine();

        stateMachine.ChangeState(new IdleState(this));
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
        var st = pc.GetCurrentState();

        if (st is IdleState or SneakState) NoiseRadius = 1f;
        else if (st is SneakMoveState) NoiseRadius = 3f;
        else if (st is MoveState or ChargingState or NormalAttackState or EscapeState) NoiseRadius = 7f;
        else if (st is RunState) NoiseRadius = 10f;
        else NoiseRadius = 0f;   // 예외·공격 등
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;   // 재생 중일 때만
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, NoiseRadius);
    }
#endif
}
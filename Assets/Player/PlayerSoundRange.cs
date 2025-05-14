using UnityEngine;

public class PlayerSoundRange : MonoBehaviour, IPlayerChangeState
{
    private PlayerStateMachine stateMachine;

    // 현재 상태가 내는 소음(반경) – 몬스터한테 공개되는 값
    public float NoiseRadius { get; private set; } = 0f;

    private void Awake()
    {
        stateMachine = new PlayerStateMachine();
        stateMachine.ChangeState(new IdleState(this));
    }

    private void Update()
    {
        stateMachine.Update();
        UpdateNoiseByState();
    }

    public void ChangeState(IPlayerState s) => stateMachine.ChangeState(s);
    public IPlayerState GetCurrentState() => stateMachine.GetCurrentState();
    public void RestorePreviousState() => stateMachine.RestorePreviousState();

    // 상태에 따른 소음 반경 설정
    private void UpdateNoiseByState()
    {
        var st = GetCurrentState();

        if (st is IdleState or SneakState) NoiseRadius = 1f;
        else if (st is SneakMoveState) NoiseRadius = 3f;
        else if (st is MoveState or ForageState) NoiseRadius = 5f;
        else if (st is RunState) NoiseRadius = 7f;
        else NoiseRadius = 0f;   // 예외·공격 등
    }
}
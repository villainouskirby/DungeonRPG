using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerSoundRange : MonoBehaviour, IPlayerChangeState
{
    private PlayerStateMachine stateMachine;
    [SerializeField] IPlayerState nowState;

    [Header("Trigger Settings")]
    public CircleCollider2D triggerCollider; // 반드시 isTrigger = true 설정


    private void Awake()
    {
        stateMachine = new PlayerStateMachine();
        stateMachine.ChangeState(new IdleState(this));


    }
    void Start()
    {

    }

    
    public void ChangeState(IPlayerState newState)
    {
        stateMachine.ChangeState(newState);
    }
    public IPlayerState GetCurrentState()
    {
        return stateMachine.GetCurrentState();
    }
    public void RestorePreviousState()
    {
        stateMachine.RestorePreviousState();
    }
    void Update()
    {
        stateMachine.Update();
        //Debug.Log(GetCurrentState());
        UpdateByState();
    }
    public void UpdateByState()
    {
        var current = GetCurrentState();

        // 예시로, 상태 이름(string) 또는 타입으로 분기
        if (current is IdleState || current is SneakState)
        {
            // Idle, Sneak -> 트리거 매우 작게
            triggerCollider.enabled = true;
            triggerCollider.radius = 1f;
        }
        else if (current is SneakMoveState)
        {
            // SneakMove -> 작게
            triggerCollider.enabled = true;
            triggerCollider.radius = 3f;

        }
        else if (current is MoveState || current is ForageState)
        {
            // Move, Forage, Attack -> 조금 크게
            triggerCollider.enabled = true;
            triggerCollider.radius = 5f;

        }
        else if (current is RunState)
        {
            // RunState -> 크게
            triggerCollider.enabled = true;
            triggerCollider.radius = 7f;

        }
    }
}
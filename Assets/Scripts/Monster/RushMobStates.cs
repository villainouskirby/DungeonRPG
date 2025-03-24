using UnityEngine;

public class RushMobIdleState : IMobState
{
    private RushMob mob;

    public RushMobIdleState(RushMob mob) { this.mob = mob; }

    public void Enter()
    {
        Debug.Log("RushMob Idle 상태 시작");
    }

    public void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        //if (moveInput != 0) mob.ChangeState(new MoveState(mob));
        // MoveStates를 둘지 아니면 그냥 여기 안에서 조금씩 배회하는 움직임 넣기


        //else if () mob.ChangeState(new WarningState(mob)); 플레이어를 감지하면 WarningState로

    }

    public void Exit() => Debug.Log("Idle 상태 종료");
    public override string ToString() => "Idle";
}

public class RushMobWarningState : IMobState
{
    private RushMob mob;

    public RushMobWarningState(RushMob mob) { this.mob = mob; }

    public void Enter()
    {
        Debug.Log("RushMob Warning 상태 시작");
    }

    public void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        //if (moveInput != 0) mob.ChangeState(new MoveState(mob));


        //else if () mob.ChangeState(new WarningState(mob)); 플레이어를 감지하면 WaringState로
        //else if () mob.ChangeState(new DashState(mob));
        //else if () mob.ChangeState(new AttackState(mob));
    }

    public void Exit() => Debug.Log("Warning 상태 종료");
    public override string ToString() => "Warning";
}

public class RushMobRushState : IMobState
{
    private RushMob mob;
    public RushMobRushState(RushMob mob) { this.mob = mob; }

    public void Enter()
    {
        Debug.Log("Rush 상태 시작");
        mob.StartRush();
    }

    public void Update()
    {

    }

    public void Exit()
    {
        Debug.Log("Rush 상태 종료");
    }
    public override string ToString() => "Rush";
}

public class RushMobGroggyState : IMobState
{
    private RushMob mob;
    public RushMobGroggyState(RushMob mob) { this.mob = mob; }

    public void Enter()
    {
        Debug.Log("Rush 상태 시작");
        mob.StartRush();
    }

    public void Update()
    {

    }

    public void Exit()
    {
        Debug.Log("Rush 상태 종료");
    }
    public override string ToString() => "Rush";
}

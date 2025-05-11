using UnityEngine;
public class NormalMobIdleState : IMobState
{
    private NormalMob mob;

    public NormalMobIdleState(NormalMob mob) { this.mob = mob; }

    public void Enter()
    {
        Debug.Log("NormalMob Idle 상태 시작");
    }

    public void Update()
    {
    }

    public void Exit() => Debug.Log("Idle 상태 종료");
    public override string ToString() => "Idle";
}

public class NormalMobDetectState : IMobState
{
    private NormalMob mob;

    public NormalMobDetectState(NormalMob mob) { this.mob = mob; }

    public void Enter()
    {
        Debug.Log("NormalMob Detect 상태 시작");
    }

    public void Update()
    {
        
    }

    public void Exit() => Debug.Log("Detect 상태 종료");
    public override string ToString() => "Detect";
}
public class NormalMobCombatState : IMobState
{
    private NormalMob mob;

    public NormalMobCombatState(NormalMob mob) { this.mob = mob; }

    public void Enter()
    {
        Debug.Log("NormalMob Combat 상태 시작");
    }

    public void Update()
    {
    }

    public void Exit() => Debug.Log("Combat 상태 종료");
    public override string ToString() => "Combat";
}
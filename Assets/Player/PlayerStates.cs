using UnityEngine;

public class IdleState : IPlayerState
{
    private IPlayerChangeState player;

    public IdleState(IPlayerChangeState player) { this.player = player; }

    public void Enter()
    {
        //Debug.Log("Idle 상태 시작");
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        { player.ChangeState(new EscapeState(player)); return; }
        //if (Input.GetKeyDown(KeyCode.E))
        //{ player.ChangeState(new GuardState(player)); return; }
        if (Input.GetMouseButtonDown(1))
        { player.ChangeState(new ChargingState(player)); return; }

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        if (moveX != 0 || moveY != 0) player.ChangeState(new MoveState(player));

        else if (Input.GetKeyDown(KeyCode.LeftControl)) player.ChangeState(new SneakState(player));
        
    }

    public void Exit() { } //Debug.Log("Idle 상태 종료");
    public override string ToString() => "Idle";
}
public class MoveState : IPlayerState
{
    private IPlayerChangeState player;
    public MoveState(IPlayerChangeState player) { this.player = player; }

    public void Enter()
    {
        Debug.Log("Move 상태 시작");
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        { player.ChangeState(new EscapeState(player)); return; }
        //if (Input.GetKeyDown(KeyCode.E))
        //{ player.ChangeState(new GuardState(player)); return; }
        if (Input.GetMouseButtonDown(1))
        { player.ChangeState(new ChargingState(player)); return; }

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
       
        if (moveX == 0 && moveY == 0) player.ChangeState(new IdleState(player));
        if (Input.GetKeyDown(KeyCode.LeftControl)) player.ChangeState(new SneakMoveState(player));
        if (Input.GetKey(KeyCode.X)) player.ChangeState(new RunState(player));
    }

    public void Exit()
    {
        //Debug.Log("Move 상태 종료");
    }

    public override string ToString() => "Move";
}
public class RunState : IPlayerState
{
    private IPlayerChangeState player;
    public RunState(IPlayerChangeState player) { this.player = player; }

    public void Enter()
    {
        //Debug.Log("Run 상태 시작");
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        { player.ChangeState(new EscapeState(player)); return; }
        //if (Input.GetKeyDown(KeyCode.E))
        //{ player.ChangeState(new GuardState(player)); return; }
        if (Input.GetMouseButtonDown(1))
        { player.ChangeState(new ChargingState(player)); return; }

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
      
        if (moveX == 0 && moveY == 0) player.ChangeState(new IdleState(player));
        if (Input.GetKeyUp(KeyCode.X)) player.ChangeState(new MoveState(player));
        if (Input.GetKeyDown(KeyCode.LeftControl)) player.ChangeState(new SneakMoveState(player));
    }

    public void Exit()
    {
        //Debug.Log("Run 상태 종료");
    }

    public override string ToString() => "Run";
}
public class SneakMoveState : IPlayerState
{
    private IPlayerChangeState player;
    public SneakMoveState(IPlayerChangeState player) { this.player = player; }

    public void Enter()
    {
        //Debug.Log("SneakMoveState 상태 시작");
    }

    public void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        
        if (moveX == 0 && moveY == 0)
        {
            if (Input.GetKey(KeyCode.LeftControl))
                player.ChangeState(new SneakState(player));
            else
                player.ChangeState(new IdleState(player)); ;
        }
        else
        {
            if (Input.GetKeyUp(KeyCode.LeftControl))
                player.ChangeState(new MoveState(player));
        }
    }

    public void Exit()
    {
        //Debug.Log("SneakMoveState 상태 종료");
    }

    public override string ToString() => "SneakMoveState";
}
public class SneakState : IPlayerState
{
    private IPlayerChangeState player;
    public SneakState(IPlayerChangeState player) { this.player = player; }

    public void Enter()
    {
        //Debug.Log("SneakState 상태 시작");
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        { player.ChangeState(new EscapeState(player)); return; }
        //if (Input.GetKeyDown(KeyCode.E))
        //{ player.ChangeState(new GuardState(player)); return; }
        if (Input.GetMouseButtonDown(1))
        { player.ChangeState(new ChargingState(player)); return; }

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        
        if (moveX == 0 && moveY == 0) {
            if (Input.GetKeyUp(KeyCode.LeftControl)) { player.ChangeState(new IdleState(player)); }
        }
        else { player.ChangeState(new SneakMoveState(player)); }
        
    }

    public void Exit()
    {
        //Debug.Log("SneakState 상태 종료");
    }

    public override string ToString() => "SneakState";
}
/*
public class ForageState : IPlayerState
{
    private IPlayerChangeState player;

    public ForageState(IPlayerChangeState player) { this.player = player; }

    public void Enter()
    {
        Debug.Log("Forage 상태 시작");
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        { player.ChangeState(new EscapeState(player)); return; }
        if (Input.GetKeyDown(KeyCode.E))
        { player.ChangeState(new GuardState(player)); return; }
        if (Input.GetMouseButtonDown(1))
        { player.ChangeState(new ChargingState(player)); return; }

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        if (moveX != 0 || moveY != 0) player.ChangeState(new MoveState(player));
        if (Input.GetKeyUp(KeyCode.F)) player.ChangeState(new IdleState(player));
    }

    public void Exit() { }// Debug.Log("Forage 상태 종료");
    public override string ToString() => "Forage";
}
public class GuardState : IPlayerState
{
    private readonly IPlayerChangeState player;
    public GuardState(IPlayerChangeState player) => this.player = player;

    public void Enter() => Debug.Log("Guard ON");
    public void Exit() => Debug.Log("Guard OFF");

    public void Update()
    {
         가드 해제 
        if (Input.GetKeyUp(KeyCode.Q))
            player.ChangeState(new IdleState(player));
    }

    public override string ToString() => "Guard";
}
*/
public class ChargingState : IPlayerState
{
    private readonly IPlayerChangeState player;
    private readonly PlayerController pc;
    private readonly AttackController ac;

    private float timer;
    private bool released;
    public ChargingState(IPlayerChangeState p)
    {
        player = p;
        pc = p as PlayerController;
        if (pc) ac = pc.GetComponent<AttackController>();  
        else ac = (p as MonoBehaviour)?.GetComponent<AttackController>();

    }

    public void Enter()
    {
        if (ac == null || ac.HeavyOnCooldown)          // 쿨타임이면 진입 거부
        { player.ChangeState(new IdleState(player)); return; }

        timer = ac.maxChargeTime;    // 최대 충전 시간

        if (!ac.TryStartCharging())      // 충전에 실패하면 즉시 Idle
        { player.ChangeState(new IdleState(player)); return; }
        timer = ac.maxChargeTime;
        released = false;
        Debug.Log("Charging…");
    }

    public void Update()
    {
        if (ac == null) return;
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            ac.CancelCharging();
            player.ChangeState(new EscapeState(player));
            return;
        }
        bool upEvent = Input.GetMouseButtonUp(1);
        bool isHolding = Input.GetMouseButton(1);
        // 우클릭을 떼면 공격 발사
        if (!released && (upEvent || !isHolding))
        {
            ac.ReleaseCharging();
            released = true;                         // 중복 방지
            player.ChangeState(new IdleState(player));
            return;
        }

        timer -= Time.deltaTime;
        if (!released && timer <= 0f)
        {
            ac.ReleaseCharging();
            released = true;
            player.ChangeState(new IdleState(player));
        }
    }

    public void Exit() { }
    public override string ToString() => "Charging";
}
public class NormalAttackState : IPlayerState
{
    private readonly IPlayerChangeState owner;
    public NormalAttackState(IPlayerChangeState p) => owner = p;

    public void Enter() { }
    public void Update() { }
    public void Exit() { }
    public override string ToString() => "NormalAttack";
}
public class EscapeState : IPlayerState
{
    private readonly PlayerController pc;   // 반드시 PlayerController
    private float timer;                    // dive + prone + getUp 총 시간
    private bool invalid = false;

    public EscapeState(IPlayerChangeState owner)
    {
        pc = owner as PlayerController;
        if (pc == null)
        {
            //Debug.LogError("EscapeState: owner is not PlayerController!");
            invalid = true;
            return;
        }
        timer = pc.diveTime + pc.proneTime + pc.getUpTime;
    }

    public void Enter()
    {
        if (invalid || !pc.TryBeginEscape())
        {
            pc?.ChangeState(new IdleState(pc));  // pc가 null이면 아무 일도 안 함
        }
    }

    public void Update()
    {
        if (invalid) return;
        timer -= Time.deltaTime;
        if (timer <= 0f || !pc.EscapeActive)
        {
            pc.ChangeState(new IdleState(pc));
        }
    }

    public void Exit() { }

    public override string ToString() => "Escape";
}

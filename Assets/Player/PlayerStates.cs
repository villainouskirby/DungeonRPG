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
        if (Input.GetKeyDown(KeyCode.E))
        { player.ChangeState(new GuardState(player)); return; }
        if (Input.GetMouseButtonDown(1))
        { player.ChangeState(new ChargingState(player)); return; }

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        if (moveX != 0 || moveY != 0) player.ChangeState(new MoveState(player));

        else if (Input.GetKeyDown(KeyCode.LeftControl)) player.ChangeState(new SneakState(player));
        else if (Input.GetKeyDown(KeyCode.F)) player.ChangeState(new ForageState(player));
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
        if (Input.GetKeyDown(KeyCode.E))
        { player.ChangeState(new GuardState(player)); return; }
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
        if (Input.GetKeyDown(KeyCode.E))
        { player.ChangeState(new GuardState(player)); return; }
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
        if (Input.GetKeyDown(KeyCode.E))
        { player.ChangeState(new GuardState(player)); return; }
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
public class EscapeState : IPlayerState
{
    private readonly IPlayerChangeState player;
    private float timer;
    private const float duration = 0.5f;   // 회피 모션 길이

    public EscapeState(IPlayerChangeState player) => this.player = player;

    public void Enter()
    {
        timer = duration;
        // 회피 애니메이션, 무적 프레임, 힘 적용 등
        Debug.Log("Escape!");
    }

    public void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) player.ChangeState(new IdleState(player));
        // Escape 동안엔 아무 입력도 받지 않음
    }

    public void Exit() { }
    public override string ToString() => "Escape";
}
public class GuardState : IPlayerState
{
    private readonly IPlayerChangeState player;
    public GuardState(IPlayerChangeState player) => this.player = player;

    public void Enter() => Debug.Log("Guard ON");
    public void Exit() => Debug.Log("Guard OFF");

    public void Update()
    {
        /* 가드 해제 */
        if (Input.GetKeyUp(KeyCode.Q))
            player.ChangeState(new IdleState(player));
    }

    public override string ToString() => "Guard";
}
public class ChargingState : IPlayerState
{
    private readonly PlayerController pc;
    private readonly IPlayerChangeState player;
    private float timer;

    public ChargingState(IPlayerChangeState p)
    {
        player = p;
        pc = p as PlayerController;
        if (pc == null) return;
            
    }

    public void Enter()
    {
        if (pc == null) { player.ChangeState(new IdleState(player)); return; }
        timer = pc.maxChargeTime;     // 최대 충전 시간
        pc.StartCharging();
        Debug.Log("Charging…");
    }

    public void Update()
    {
        if (pc == null) return;
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            pc.CancelCharging();
            player.ChangeState(new EscapeState(player));
            return;
        }
        pc.UpdateAttackChargeGauge(pc.AttackChargeRatio); //게이지 전달

        // 우클릭을 떼면 공격 발사
        if (Input.GetMouseButtonUp(1))
        {
            pc.ReleaseCharging();
            player.ChangeState(new IdleState(player));
            return;
        }

        // 시간 경과로 자동 발사
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            pc.ReleaseCharging();
            player.ChangeState(new IdleState(player));
        }
    }

    public void Exit() { }
    public override string ToString() => "Charging";
}
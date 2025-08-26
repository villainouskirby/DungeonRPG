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
        if (Input.GetKeyDown(KeyCode.Space)) { player.ChangeState(new EscapeState(player)); return; }
        if (Input.GetMouseButtonDown(1)) { player.ChangeState(new GuardState(player)); return; }

        Vector2 mv = (player as PlayerController)?.ReadMoveRaw()
                     ?? new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (mv.x != 0 || mv.y != 0)
            player.ChangeState(new MoveState(player));
        else if (Input.GetKeyDown(KeyCode.LeftControl))
            player.ChangeState(new SneakState(player));

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
        if (Input.GetKeyDown(KeyCode.Space))
        { player.ChangeState(new EscapeState(player)); return; }
        if (Input.GetMouseButtonDown(1))
        { player.ChangeState(new GuardState(player)); return; }
        //if (Input.GetMouseButtonDown(0))
        //{ player.ChangeState(new ChargingState(player)); return; }

        Vector2 mv = (player as PlayerController)?.ReadMoveRaw() 
                 ?? new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
       
        if (mv.x == 0 && mv.y == 0) player.ChangeState(new IdleState(player));
        if (Input.GetKeyDown(KeyCode.LeftControl)) player.ChangeState(new SneakMoveState(player));
        if (Input.GetKey(KeyCode.LeftShift) && PlayerData.instance.CanStartSprint()) player.ChangeState(new RunState(player));
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
        if (Input.GetKeyDown(KeyCode.Space))
        { player.ChangeState(new EscapeState(player)); return; }
        if (Input.GetMouseButtonDown(1))
        { player.ChangeState(new GuardState(player)); return; }
        //if (Input.GetMouseButtonDown(0))
        //{ player.ChangeState(new ChargingState(player)); return; }

        Vector2 mv = (player as PlayerController)?.ReadMoveRaw()
                 ?? new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (mv.x == 0 && mv.y == 0) player.ChangeState(new IdleState(player));
        if (Input.GetKeyUp(KeyCode.LeftShift) || !PlayerData.instance.TryConsumeSprintThisFrame(Time.deltaTime))
        { 
            player.ChangeState(new MoveState(player));
            return;
        }
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
        Vector2 mv = (player as PlayerController)?.ReadMoveRaw()
                 ?? new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (mv.x == 0 && mv.y == 0)
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
        if (Input.GetKeyDown(KeyCode.Space))
        { player.ChangeState(new EscapeState(player)); return; }
        if (Input.GetMouseButtonDown(1))
        { player.ChangeState(new GuardState(player)); return; }
        //if (Input.GetMouseButtonDown(0))
        //{ player.ChangeState(new ChargingState(player)); return; }

        Vector2 mv = (player as PlayerController)?.ReadMoveRaw()
                 ?? new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));


        if (mv.x == 0 && mv.y == 0)  { if (Input.GetKeyUp(KeyCode.LeftControl)) { player.ChangeState(new IdleState(player)); } }
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
*/
public class GuardState : IPlayerState
{
    readonly PlayerController pc;
    readonly AttackController ac;
    readonly PlayerDefense pd;

    public GuardState(IPlayerChangeState player)
    {
        pc = player as PlayerController;
        ac = pc?.GetComponent<AttackController>();
        pd = pc?.GetComponent<PlayerDefense>();
    }

    public void Enter()
    {
        Debug.Log("Guard ON");
        if (pc) pc.rb.velocity = Vector2.zero;      // 즉시 정지
    }

    public void Update()
    {
        // 공격키 → 가드 해제 & 공격 전환
        if (Input.GetMouseButtonDown(0))
        {
            pc.ChangeState(new IdleState(pc));
            return;
        }

        // 우클릭을 떼면 가드 종료
        if (Input.GetMouseButtonUp(1))
        {
            pc.ChangeState(new IdleState(pc));
            return;
        }

        // 가드가 불가능해진 경우(스태미너 부족·쿨타임 등)
        if (pd && !pd.GuardAvailable)
        {
            pc.ChangeState(new IdleState(pc));
            return;
        }
    }

    public void Exit() => Debug.Log("Guard OFF");
    public override string ToString() => "Guard";
}
public class ChargingState : IPlayerState
{
    private readonly IPlayerChangeState player;
    private readonly PlayerController pc;
    private readonly AttackController ac;

    float startTime;
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
        startTime = Time.time;
        Debug.Log("Charging…");
    }

    public void Update()
    {
        if (ac == null) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ac.CancelCharging();
            player.ChangeState(new EscapeState(player));
            return;
        }
        PlayerData.instance.isStaminaBlocked = true;
        // 차징 도중 스태미너 지속 소모
        float need = ac != null ? ac.GetType()
            .GetField("heavyChargeStaminaPerSec", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null
            ? (float)ac.GetType().GetField("heavyChargeStaminaPerSec", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(ac)
            : 25f : 25f; // 뭔코든진 모르겠는데 나중에 렉걸리면 수정

        float consume = need * Time.deltaTime;
        if (!PlayerData.instance.SpendStamina(consume))
        {
            // 스태미너 바닥 → 강공격 취소 및 Idle
            ac.CancelCharging();
            player.ChangeState(new IdleState(player));
            PlayerData.instance.BlockStaminaRegen(1f);
            return;
        }
        // 우클릭을 떼면 공격 발사
        if (!released && Input.GetMouseButtonUp(0))
        {
            ac.ReleaseCharging();
            released = true;
            Debug.Log("차징 공격 발사");
            player.ChangeState(new IdleState(player));
            PlayerData.instance.BlockStaminaRegen(1f);
            return;
        }

        float held = Time.time - startTime;
        if (held > ac.maxChargeTime)
            held = ac.maxChargeTime;
    }

    public void Exit() { }
    public override string ToString() => "Charging";
}
public class NormalAttackState : IPlayerState
{
    private readonly IPlayerChangeState owner;
    private float remain;        // 후딜이 끝날 때까지 유지
    public NormalAttackState(IPlayerChangeState p, float afterDelay)
    {
        owner = p;
        remain = afterDelay;
    }

    public void Enter() { }
    public void Update()
    {
        remain -= Time.deltaTime;
        if (remain <= 0f)
            owner.ChangeState(new IdleState(owner));  // 복귀
    }
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

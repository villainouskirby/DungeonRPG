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
public sealed class PotionConsumeState : IPlayerState
{
    private readonly IPlayerChangeState owner;
    private readonly PlayerController pc;
    private readonly PotionManager pm;
    private readonly float duration;

    private float startTime;
    private bool finished;

    public PotionConsumeState(IPlayerChangeState p, float durationSec)
    {
        owner = p;
        pc = p as PlayerController;
        pm = PotionManager.instance;
        duration = Mathf.Max(0.01f, durationSec);
    }

    public void Enter()
    {

        startTime = Time.time;
        finished = false;

        if (pm != null) pm.OnGaugeEnd += HandleGaugeEnd;

        // HP 변동 감지 → 섭취 취소
        if (PlayerData.instance != null)
            PlayerData.instance.OnHPChanged += HandleHpChanged;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // 회피 시 포션 사용 취소
        { 
            owner.ChangeState(new EscapeState(owner));
            pm.RequestCancelDrink();
            return;
        }
        // 안전망: 이벤트 못 받아도 시간 경과 시 종료
        if (!finished && Time.time - startTime >= duration)
        {
            finished = true;
            owner.ChangeState(new IdleState(owner));
        }
    }

    public void Exit()
    {
        if (pm != null) pm.OnGaugeEnd -= HandleGaugeEnd;
        if (PlayerData.instance != null)
            PlayerData.instance.OnHPChanged -= HandleHpChanged;
    }

    public override string ToString() => "PotionConsume";

    private void HandleGaugeEnd()
    {
        if (!finished)
        {
            finished = true;
            owner.ChangeState(new IdleState(owner));
        }
    }

    private void HandleHpChanged(float oldHp, float newHp)
    {
        if (!finished && pm != null && newHp < oldHp)
            pm.RequestCancelDrink();
    }
}
public sealed class StunState : IPlayerState
{
    private readonly PlayerController pc;
    private float remain;
    private bool finished;
    public StunState(PlayerController controller, float duration)
    {
        pc = controller;
        remain = duration;
    }
    public void Enter()
    {
        finished = false;
        if (pc)
        {
            pc.rb.velocity = Vector2.zero;
            pc.anim.Play("Stun"); // 스턴 전용 애니메이션 클립 있으면 여기
        }
    }
    public void Update()
    {
        if (finished) return;

        remain -= Time.deltaTime;
        if (remain <= 0f)
        {
            finished = true;
            pc.ChangeState(new IdleState(pc));
        }
        else
        {
            // 매 프레임 이동 강제 정지
            if (pc) pc.rb.velocity = Vector2.zero;
        }
    }
    public void Exit()
    {
        // 스턴 해제 시 추가 처리 필요하다면 작성
    }
    public override string ToString() => "Stun";
}
public class ChargingState : IPlayerState
{
    private readonly IPlayerChangeState player;
    private readonly PlayerController pc;
    private readonly AttackController ac;

    float startTime;
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
        if (ac == null || ac.HeavyOnCooldown)
        {
            player.ChangeState(new IdleState(player));
            return;
        }

        // 차징 시도
        if (!ac.TryStartCharging())
        {
            player.ChangeState(new IdleState(player));
            return;
        }

        // 이번 차징 세션 누적 소모 상한 20 시작
        if (PlayerData.instance != null)
            PlayerData.instance.BeginChargeSpendCap(19f);

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
            if (PlayerData.instance != null) PlayerData.instance.EndChargeSpendCap();
            player.ChangeState(new EscapeState(player));
            return;
        }
        bool ok = PlayerData.instance != null
                 ? PlayerData.instance.TryConsumeChargeThisFrame(Time.deltaTime)
                 : true;

        // Exhaust(실제 고갈) 되면 차징 중단
        if (!ok)
        {
            ac.CancelCharging();
            if (PlayerData.instance != null) PlayerData.instance.ForceExhaustToZero();
            if (PlayerData.instance != null) PlayerData.instance.EndChargeSpendCap();
            player.ChangeState(new IdleState(player));
            return;
        }

        // 마우스 좌클릭을 떼면 발사
        if (!released && Input.GetMouseButtonUp(0))
        {
            ac.ReleaseCharging();
            released = true;

            // 차징 종료 정리
            if (PlayerData.instance != null)
            {
                PlayerData.instance.BlockStaminaRegen(1f);
                PlayerData.instance.EndChargeSpendCap();
            }

            player.ChangeState(new IdleState(player));
            return;
        }
        float held = Mathf.Min(Time.time - startTime, ac.maxChargeTime);
    }

    public void Exit() { if (PlayerData.instance != null) PlayerData.instance.EndChargeSpendCap(); }
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

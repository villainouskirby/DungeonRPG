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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (PlayerManager.Instance == null || PlayerManager.Instance.CanDodge)
                player.ChangeState(new EscapeState(player));
            return;
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (PlayerManager.Instance == null || PlayerManager.Instance.CanGuard)
                player.ChangeState(new GuardState(player));
            return;
        }
        var pf = PlayerFarming.Instance;
        if (pf != null && Input.GetKeyDown(pf.FarmingKey) && pf.CanFarmNow())
        {
            player.ChangeState(new FarmingState(player));
            return;
        }
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
        SoundManager.Instance.PlaySound2D("SFX_PlayerWalk", 0.1f, true, SoundType.SFX);
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
        if (Input.GetKey(KeyCode.LeftShift) && PlayerData.Instance.CanStartSprint()) player.ChangeState(new RunState(player));
    }

    public void Exit()
    {
        //Debug.Log("Move 상태 종료");
        SoundManager.Instance.StopLoopSound("SFX_PlayerWalk");
    }

    public override string ToString() => "Move";
}
public class RunState : IPlayerState
{
    private IPlayerChangeState player;
    public RunState(IPlayerChangeState player) { this.player = player; }

    public void Enter()
    {
        SoundManager.Instance.PlaySound2D("SFX_PlayerRun", 0.1f, true, SoundType.SFX);
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
        if (Input.GetKeyUp(KeyCode.LeftShift) || !PlayerData.Instance.TryConsumeSprintThisFrame(Time.deltaTime))
        { 
            player.ChangeState(new MoveState(player));
            return;
        }
        if (Input.GetKeyDown(KeyCode.LeftControl)) player.ChangeState(new SneakMoveState(player));
    }

    public void Exit()
    {
        SoundManager.Instance.StopLoopSound("SFX_PlayerRun");
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
        SoundManager.Instance.PlaySound2D("SFX_PlayerSneakMove", 0.3f, true, SoundType.SFX);
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
        SoundManager.Instance.StopLoopSound("SFX_PlayerSneakMove");
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
        if (PlayerManager.Instance != null && !PlayerManager.Instance.CanGuard)
        {
            pc?.ChangeState(new IdleState(pc));
            return;
        }
        SoundManager.Instance.PlaySound2D("SFX_PlayerGuardUp");
        Debug.Log("Guard ON");
        if (pc) pc.rb.velocity = Vector2.zero;      // 즉시 정지
    }

    public void Update()
    {
        if (PlayerManager.Instance != null && !PlayerManager.Instance.CanGuard) // 가드 유지 중에도 막히면 해제
        {
            pc.ChangeState(new IdleState(pc));
            return;
        }
        PlayerData.Instance?.BlockStaminaRegen(1f);
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

    public void Exit()
    {
        Debug.Log("Guard OFF");
    }
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
        pm = PotionManager.Instance;
        duration = Mathf.Max(0.01f, durationSec);
    }

    public void Enter()
    {

        startTime = Time.time;
        finished = false;

        if (pm != null) pm.OnGaugeEnd += HandleGaugeEnd;

        // HP 변동 감지 → 섭취 취소
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnHPChanged += HandleHpChanged;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // 회피 시 포션 사용 취소
        {
            if (PlayerManager.Instance == null || PlayerManager.Instance.CanDodge)
            {
                owner.ChangeState(new EscapeState(owner));
                pm.RequestCancelDrink();
            }
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
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnHPChanged -= HandleHpChanged;
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
        SoundManager.Instance.PlaySound2D("SFX_PlayerStrongAttackCharge", 0f, true, SoundType.SFX);
        // 이번 차징 세션 누적 소모 상한 20 시작
        if (PlayerData.Instance != null)
            PlayerData.Instance.BeginChargeSpendCap(19f);

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
            if (PlayerData.Instance != null) PlayerData.Instance.EndChargeSpendCap();
            player.ChangeState(new EscapeState(player));
            return;
        }
        bool ok = PlayerData.Instance != null
                 ? PlayerData.Instance.TryConsumeChargeThisFrame(Time.deltaTime)
                 : true;

        // Exhaust(실제 고갈) 되면 차징 중단
        if (!ok)
        {
            ac.CancelCharging();
            if (PlayerData.Instance != null) PlayerData.Instance.ForceExhaustToZero();
            if (PlayerData.Instance != null) PlayerData.Instance.EndChargeSpendCap();
            player.ChangeState(new IdleState(player));
            return;
        }

        // 마우스 좌클릭을 떼면 발사
        if (!released && Input.GetMouseButtonUp(0))
        {
            ac.ReleaseCharging();
            released = true;

            // 차징 종료 정리
            if (PlayerData.Instance != null)
            {
                PlayerData.Instance.BlockStaminaRegen(1f);
                PlayerData.Instance.EndChargeSpendCap();
            }
            
            return;
        }
        float held = Mathf.Min(Time.time - startTime, ac.maxChargeTime);
    }

    public void Exit() 
    {
        SoundManager.Instance.StopLoopSound("SFX_PlayerStrongAttackCharge");
        if (PlayerData.Instance != null) PlayerData.Instance.EndChargeSpendCap(); 
    }
    public override string ToString() => "Charging";
}
public class NormalAttackState : IPlayerState
{
    private readonly IPlayerChangeState owner;
    private float remain;        // 후딜이 끝날 때까지 유지
    private const float BUFFER_WINDOW_SEC = 0.5f;
    private enum NextIntent { None, Guard, Escape }
    private NextIntent queued = NextIntent.None;
    public NormalAttackState(IPlayerChangeState p, float afterDelay)
    {
        owner = p;
        remain = afterDelay;
    }

    public void Enter() { }
    public void Update()
    {
        if (queued == NextIntent.None && remain <= BUFFER_WINDOW_SEC)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (PlayerManager.Instance == null || PlayerManager.Instance.CanDodge)
                    queued = NextIntent.Escape;
            }
            else if (Input.GetMouseButtonDown(1))
            {
                if (PlayerManager.Instance == null || PlayerManager.Instance.CanGuard)
                    queued = NextIntent.Guard;
            }
        }

        remain -= Time.deltaTime;
        if (remain <= 0f)
        {
            switch (queued)
            {
                case NextIntent.Escape:
                    if (PlayerManager.Instance == null || PlayerManager.Instance.CanDodge)
                    {
                        var pc = owner as PlayerController;
                        if (pc != null)
                        {
                            Vector2 mv = pc.ReadMoveRaw();
                            if (mv != Vector2.zero)
                            {
                                if (Mathf.Abs(mv.x) > Mathf.Abs(mv.y))
                                    pc.SetFacingDirection(mv.x < 0 ? 2 : 3);
                                else
                                    pc.SetFacingDirection(mv.y > 0 ? 0 : 1);
                            }
                        }
                        owner.ChangeState(new EscapeState(owner));
                        break;
                    }
                    goto default;

                case NextIntent.Guard:
                    if (PlayerManager.Instance == null || PlayerManager.Instance.CanGuard)
                    {
                        owner.ChangeState(new GuardState(owner));
                        break;
                    }
                    goto default;

                default:
                    owner.ChangeState(new IdleState(owner));
                    break;
            }
        }
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
        if (UIPopUpHandler.Instance.IsUIOpen) { return; }
        if ((PlayerManager.Instance != null && !PlayerManager.Instance.CanDodge) ||
       invalid || !pc.TryBeginEscape())
        {
            pc?.ChangeState(new IdleState(pc));
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

public sealed class FarmingState : IPlayerState
{
    private readonly IPlayerChangeState owner;
    private readonly PlayerController pc;
    private PlayerFarming pf;

    private float total;   // 필요 시간
    private float remain;  // 남은 시간
    private bool finished;

    public FarmingState(IPlayerChangeState p)
    {
        owner = p;
        pc = p as PlayerController;
    }

    public void Enter()
    {
        pf = PlayerFarming.Instance;
        if (pf == null || !pf.CanFarmNow())
        {
            owner.ChangeState(new IdleState(owner));
            return;
        }

        // 움직임 정지(원하면 유지)
        if (pc != null) pc.rb.velocity = Vector2.zero;

        total = Mathf.Max(0.05f, pf.GetRequiredTime()); // 1f 기본
        remain = total;
        finished = false;

        pf.BeginFarmVisuals();

        // 피격 감지 → HP 감소 시 캔슬
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnHPChanged += OnHPChanged;
    }

    public void Update()
    {
        if (finished) return;

        if (Input.GetKeyDown(KeyCode.Space) &&
            (PlayerManager.Instance == null || PlayerManager.Instance.CanDodge))
        {
            CancelAndGoEscape();
            return;
        }

        if (Input.GetKeyDown(PlayerFarming.Instance.FarmingKey))
        {
            CancelAndGoIdle();
            return;
        }

        if (!pf.IsTargeting || pf.TargetObj == null)
        {
            CancelAndGoIdle();
            return;
        }

        remain -= Time.deltaTime;
        pf.UpdateGage(remain, total);

        if (remain <= 0f)
        {
            finished = true;
            CompleteAndExit();
        }
    }

    public void Exit()
    {
        // 안전 정리
        pf?.CancelFarmVisuals();
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnHPChanged -= OnHPChanged;
    }

    public override string ToString() => "Farming";

    private void CancelAndGoIdle()
    {
        finished = true;
        owner.ChangeState(new IdleState(owner));
    }

    private void CancelAndGoEscape()
    {
        finished = true;
        owner.ChangeState(new EscapeState(owner));
    }

    private void CompleteAndExit()
    {
        // 성공 처리 (몬스터/자원 내부에서 정리)
        // 내부적으로 디스폰/남은 횟수/풀반납 등 처리됨
        var beforeTarget = pf.TargetObj;
        InvokeSuccess();

        // 타깃이 사라졌거나 더 이상 파밍 불가면 Idle
        if (pf.TargetObj == null || beforeTarget != pf.TargetObj || !pf.CanFarmNow())
            owner.ChangeState(new IdleState(owner));
        else
            owner.ChangeState(new IdleState(owner)); // 1회 단위 종료(요구사항상 한 번 캐면 종료)
    }

    private void InvokeSuccess()
    {
        pf.SuccessFarm();
    }

    private void OnHPChanged(float oldHp, float newHp)
    {
        if (!finished && newHp < oldHp)
            CancelAndGoIdle();
    }
}
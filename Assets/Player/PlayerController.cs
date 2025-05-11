using UnityEngine;

public class PlayerController : MonoBehaviour, IPlayerChangeState
{
    /* ---------- 이동/애니메이션 ---------- */
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;

    [Header("Movement Settings")]
    public float speed = 5f;   // 현재 이동 속도(상태별로 변동)
    public float slideForce = 30f;  // 회피용
    public float slideDuration = .4f;

    /* ---------- 상태머신 ---------- */
    private PlayerStateMachine stateMachine;
    [SerializeField] IPlayerState nowState;

    /* ---------- 내부 ---------- */
    private bool stateLocked = false; // 외부(포션 등) 잠금
    private int facingDir = 1;     // 0=Up,1=Down,2=Left,3=Right
    public int FacingDir => facingDir;
    public void SetFacingDirection(int d)
    {
        facingDir = d;

        // 애니메이션·스프라이트도 즉시 갱신
        anim.SetInteger("Direction", d);

        if (d == 3) sprite.flipX = true;   // Right
        else if (d == 2) sprite.flipX = false;   // Left
    }

    /* ---------- 초기화 ---------- */
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        stateMachine = new PlayerStateMachine();
        stateMachine.ChangeState(new IdleState(this));
    }

    /* ---------- 상태 갱신 ---------- */
    private void Update()
    {
        stateMachine.Update();
        UpdateByState();
    }
    private void FixedUpdate()
    {
        /* 이동 */
        float hx = Input.GetAxis("Horizontal");
        float hy = Input.GetAxis("Vertical");

        if (hx == 0f && hy == 0f) { rb.velocity = Vector2.zero; return; }

        Vector2 dir = new(hx, hy);
        rb.velocity = dir.normalized * speed;
        anim.SetBool("iswalking", true);

        /* 바라보는 방향(좌우 Flip 포함) */
        if (Mathf.Abs(hx) > 0f) facingDir = (hx < 0) ? 2 : 3;
        else if (Mathf.Abs(hy) > 0f) facingDir = (hy > 0) ? 0 : 1;

        anim.SetInteger("Direction", facingDir);
        if (facingDir == 3) sprite.flipX = true;
        else if (facingDir == 2) sprite.flipX = false;
    }
    private void UpdateByState()
    {
        var cur = stateMachine.GetCurrentState();
        speed = cur switch
        {
            IdleState or SneakState => 0f,
            SneakMoveState or ChargingState
            or NormalAttackState => 1f,
            MoveState or ForageState => 3f,
            RunState => 5f,
            _ => speed
        };
    }

    /* ---------- IPlayerChangeState ---------- */
    public void ChangeState(IPlayerState s) { if (!stateLocked) stateMachine.ChangeState(s); }
    public IPlayerState GetCurrentState() => stateMachine.GetCurrentState();
    public void RestorePreviousState() => stateMachine.RestorePreviousState();

    /* ---------- 외부에서 상태 잠그기/풀기 ---------- */
    public void LockState()
    {
        if (!(stateMachine.GetCurrentState() is IdleState))
            ChangeState(new IdleState(this));
        stateLocked = true;
        rb.velocity = Vector2.zero;
    }
    public void UnlockState() => stateLocked = false;
}

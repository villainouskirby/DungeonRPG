#if UNITY_EDITOR
using UnityEditor;      // Handles
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IPlayerChangeState
{
    private PlayerStateMachine stateMachine;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;

    [SerializeField] IPlayerState nowState;


    [Header("Movement Settings")]
    public float speed = 5f;            // 일반 이동 속도
    public float slideForce = 30f;       // 회피 시 속도
    public float slideDuration = 0.4f;  // 회피 지속 시간

    [Header("Attack Settings")]
    [SerializeField] private float baseDamage = 10f;     // 플레이어 기본 공격력
    [SerializeField] private float[] comboRate = { 0.7f, 1.3f }; // 1 타, 2 타 배율
    [SerializeField] private float[] afterDelay = { 0.3f, 0.5f }; // 1 타, 2 타 후딜
    [SerializeField] private float comboInputTime = 0.10f;  // 후딜 끝~다음 입력 허용
    [SerializeField] private float hitboxActiveTime = 0.12f;  // 히트박스 유지 시간

    private bool isAttacking = false;
    private int comboStep = 0;   // 0=콤보 없음, 1=1타, 2=2타
    float nextAttackReadyTime = 0f;

    [SerializeField] private float beforeSpeed = 0;
    private float moveInput = 0f;

    public bool canSneak { get; set; }
  
    [Header("Direction")]
    [SerializeField] private PlayerInputDirection direction;
    [SerializeField] private PlayerLookingDirection looking;

    private Animator m_animator;


    // 회피 중인지 여부
    private bool m_sliding = false;
    private float m_slidingTimer = 0f;

    // 플레이어가 바라보는 방향 (0=위,1=아래,2=왼,3=오른쪽)
    private int m_facingDirection = 1; // 기본 아래(1)로 가정
    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        stateMachine = new PlayerStateMachine();
        stateMachine.ChangeState(new IdleState(this));
        direction = PlayerInputDirection.None;
        looking = PlayerLookingDirection.Right;
        canSneak = true;


    }
    void Start()
    {
        m_animator = GetComponent<Animator>();

    }
    
    public void SetMoveInput(float input)
    {
        moveInput = input;
    }
    
    public void StopMovement()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
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
    void Update() // 이동, 공격 로직
    {
        stateMachine.Update();
        if (isAttacking) return;
        UpdateByState();

        if (Input.GetMouseButtonDown(1) && !m_sliding)
        {
            float now = Time.time;

            // 아직 쿨타임이 안 끝났으면 입력 무시
            if (now < nextAttackReadyTime) return;

            if (now - nextAttackReadyTime <= comboInputTime)
                comboStep++;        // 콤보 이어가기
            else
                comboStep = 1;      // 첫 타로 리셋

            if (comboStep > 2) comboStep = 1;     // 현재 2단까지

            StartCoroutine(PerformAttack(comboStep));
        }
    }
    public void UpdateByState()
    {
        var current = GetCurrentState();

        // 예시로, 상태 이름(string) 또는 타입으로 분기
        if (current is IdleState || current is SneakState)
        {
        }
        else if (current is SneakMoveState)
        {
            speed = 1f;
        }
        else if (current is MoveState || current is ForageState || current is AttackState)
        {
            speed = 3f;
        }
        else if (current is RunState)
        {
            speed = 5f;
        }
    }
    void FixedUpdate()
    {
        if (isAttacking)       // 공격 중이면 강제로 제동
        {
            rb.velocity = Vector2.zero;
            return;
        }
        // 이동 입력 (Input.GetAxis)
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector2 dir = new Vector2(moveX, moveY);
        Vector2 velocity = new Vector2(moveX, moveY).normalized * speed;
        rb.velocity = velocity;
        bool isWalk = dir.magnitude > 0f;
        m_animator.SetBool("iswalking", isWalk);


        // 바라보는 방향 업데이트
        if (isWalk)
        {
            // 1) 가로축 입력이 있다면 → 좌/우 방향 우선
            if (Mathf.Abs(moveX) > 0f)
            {
                m_facingDirection = (moveX < 0) ? 2 : 3; // 왼쪽:2, 오른쪽:3
            }
            // 2) 가로축 입력이 없고, 세로축 입력이 있다면 → 위/아래 방향
            else if (Mathf.Abs(moveY) > 0f)
            {
                m_facingDirection = (moveY > 0) ? 0 : 1; // 위:0, 아래:1
            }

            // 애니메이터 Direction 파라미터 갱신
            m_animator.SetInteger("Direction", m_facingDirection);

            // 좌우 Flip 처리 (왼쪽:2 = flipX=false, 오른쪽:3 = flipX=true)
            if (m_facingDirection == 3)
                sprite.flipX = true;
            else if (m_facingDirection == 2)
                sprite.flipX = false;
        }
    }
    #region 공격 로직
    private int DirFromMouse()
    {
        // 0=Up, 1=Down, 2=Left, 3=Right 반환
        Vector2 v = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) -
                    (Vector2)transform.position;

        // X‑자(대각선) 경계: |x| > |y| → 좌/우 , 그 반대 → 상/하
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
            return (v.x > 0) ? 3 : 2;
        else
            return (v.y > 0) ? 0 : 1;
    }
    private IEnumerator PerformAttack(int step)
    {
        isAttacking = true;

        m_facingDirection = DirFromMouse();
        Vector2 forward = FacingVector();
        m_animator.SetInteger("Direction", m_facingDirection);

        string trig = $"Attack{step}" + (m_facingDirection switch
        {
            0 => "Up",
            1 => "Down",
            2 => "Left",
            _ => "Right"
        });
        m_animator.SetTrigger(trig);

        int dmg = Mathf.RoundToInt(baseDamage * comboRate[step - 1]);
        if (step == 1) DoThrust(dmg, (Vector2)transform.position, forward);
        else if (step == 2) DoSlash(dmg, (Vector2)transform.position, forward);

        //히트박스 지속
        yield return new WaitForSeconds(hitboxActiveTime);

        // afterDelay 동안 이동·입력 금지 (쿨타임)
        nextAttackReadyTime = Time.time + afterDelay[step - 1];
        yield return new WaitForSeconds(afterDelay[step - 1]);

        isAttacking = false;
    }

    private Vector2 FacingVector()
    {
        return m_facingDirection switch
        {
            0 => Vector2.up,
            1 => Vector2.down,
            2 => Vector2.left,
            3 => Vector2.right,
            _ => Vector2.zero
        };
    }

    void DoSlash(int damage, Vector2 origin, Vector2 dir)
    {
        float width = 2f;
        float length = 1f;

        Vector2 center = origin + dir * (length * 0.5f);
        Vector2 size = new(width, length);
        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, angleDeg);

        HashSet<MonsterBase> already = new();      // 같은 몬스터 두 번 때리지 않음
        foreach (var h in hits)
        {
            if (!h.CompareTag("Monster")) continue;

            if (h.TryGetComponent(out MonsterBase monster)   // 변수에 먼저 담는다
                && already.Add(monster))
            {
                monster.TakeDamage(damage);
            }
        }
    }
    void DoThrust(int damage, Vector2 origin, Vector2 dir)
    {
        float radius = 2.0f;
        float arcAngle = 60f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius);

        HashSet<MonsterBase> already = new();
        foreach (var h in hits)
        {
            if (!h.CompareTag("Monster")) continue;

            Vector2 to = (Vector2)h.transform.position - origin;
            if (Vector2.Angle(dir, to) <= arcAngle * 0.5f
                && h.TryGetComponent(out MonsterBase monster)
                && already.Add(monster))
            {
                monster.TakeDamage(damage);
            }
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;   // 플레이 중일 때만

        Vector2 origin = transform.position;
        Vector2 dir    = FacingVector();

        // Slash(직사각형)
        float width  = 2f;
        float length = 1f;
        Vector2 center   = origin + dir * (length * 0.5f);
        float   angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Matrix4x4 old = Gizmos.matrix;
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);   // 붉은색 반투명
        Gizmos.matrix =
            Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angleDeg), Vector3.one);
        Gizmos.DrawCube(Vector3.zero, new Vector3(width, length, 0));
        Gizmos.matrix = old;

        // Thrust(부채꼴)
#if UNITY_EDITOR
        Handles.color = new Color(1f, 0f, 0f, 0.25f);
        float radius   = 2.0f;
        float arcAngle = 60f;
        Handles.DrawSolidArc(origin, Vector3.forward,
                             Quaternion.Euler(0, 0, -arcAngle * 0.5f) * dir,
                             arcAngle, radius);
#endif
    }
#endif
    #endregion
}

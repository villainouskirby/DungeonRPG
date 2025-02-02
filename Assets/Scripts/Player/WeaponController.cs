using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3f;            // 일반 이동 속도
    public float slideForce = 5f;        // 회피 시 속도
    public float slideDuration = 0.4f;   // 회피 지속 시간(애니메이션 길이에 맞게 조정)

    [Header("Attack Settings")]
    public float comboResetTime = 1.0f;      // 콤보 입력 유효시간
    public float minAttackInterval = 0.9f;  // 공격 간 최소 간격
    private int m_currentAttack = 0;         // 현재 콤보 단계
    private float m_timeSinceAttack = 0f;

    private Rigidbody2D m_body2d;
    private Animator m_animator;
    private SpriteRenderer sr;

    // 회피 중인지 여부
    private bool m_sliding = false;
    private float m_slidingTimer = 0f;

    // 플레이어가 바라보는 방향 (0=위,1=아래,2=왼,3=오른쪽)
    private int m_facingDirection = 1; // 기본 아래(1)로 가정

    void Start()
    {
        m_body2d = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>(); // SpriteRenderer 참조
        //무기와 플레이어 간 충돌 방지, 떨어짐 방지
        Collider2D col1 = GetComponent<Collider2D>();
        Collider2D col2 = GameObject.Find("Player").GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(col1, col2, true);
        FixedJoint2D joint = gameObject.AddComponent<FixedJoint2D>();
        joint.connectedBody = GameObject.Find("Player").GetComponent<Rigidbody2D>();
    }
    //플레이어 이동과 똑같은 로직으로 이동
    void Update()
    {
        // 매 프레임마다 공격 쿨타임 증가
        m_timeSinceAttack += Time.deltaTime;

        // 회피 상태면 타이머 갱신
        if (m_sliding)
        {
            m_slidingTimer += Time.deltaTime;
            if (m_slidingTimer >= slideDuration)
            {
                // 회피 종료
                m_sliding = false;
                m_slidingTimer = 0f;
            }
        }

        // 이동 입력
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(KeyCode.W)) y = 1f;
        if (Input.GetKey(KeyCode.S)) y = -1f;
        if (Input.GetKey(KeyCode.A)) x = -1f;
        if (Input.GetKey(KeyCode.D)) x = 1f;

        // 이동 벡터(노멀라이즈)
        Vector2 dir = new Vector2(x, y).normalized;

        // 회피 중이 아닐 때만 이동 가능
        if (!m_sliding)
        {
            m_body2d.velocity = dir * speed;
        }

        // 이동 중인지 체크 → Animator Bool 파라미터
        bool isWalk = dir.magnitude > 0f;
        m_animator.SetBool("iswalking", isWalk);


        // 바라보는 방향 업데이트
        // 이동 중일 때만 방향 갱신
        if (isWalk)
        {
            // (위,아래,왼,오) 판정
            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                // 좌우 우선
                m_facingDirection = (x < 0) ? 2 : 3; // 왼:2, 오른쪽:3
            }
            else
            {
                // 상하 우선
                m_facingDirection = (y > 0) ? 0 : 1; // 위:0, 아래:1
            }

            // Animator에 Direction 세팅
            m_animator.SetInteger("Direction", m_facingDirection);

            // 좌우 Flip 처리 (왼쪽:2 = flipX=false / 오른쪽:3 = flipX=true)
            if (m_facingDirection == 3)
                sr.flipX = true;
            else if (m_facingDirection == 2)
                sr.flipX = false;
        }


        // 회피(sliding) 처리
        // 회피 중이 아닐 때 LeftShift를 누르면
        if (Input.GetKeyDown(KeyCode.LeftShift) && !m_sliding)
        {
            m_sliding = true;
            m_slidingTimer = 0f; // 회피 시작


            // 실제 회피 속도 적용 (수직/수평)
            Vector2 SlideDir = Vector2.zero;
            if (m_facingDirection == 0) SlideDir = Vector2.up;
            else if (m_facingDirection == 1) SlideDir = Vector2.down;
            else if (m_facingDirection == 2) SlideDir = Vector2.left;
            else if (m_facingDirection == 3) SlideDir = Vector2.right;

            m_body2d.velocity = SlideDir * slideForce;
        }


        // 콤보 공격 처리
        // L키를 누르고, 공격 간격이 충분하며, 회피 중이 아닐 때
        if (Input.GetKeyDown(KeyCode.L) && (m_timeSinceAttack > minAttackInterval) && !m_sliding)
        {
            m_currentAttack++;

            // 3단 콤보 후 1로 돌아오기
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // 콤보 리셋 시간(1초) 넘으면 다시 1타부터
            if (m_timeSinceAttack > comboResetTime)
                m_currentAttack = 1;

            // 방향 + 콤보 단계로 트리거 이름 결정
            // ex) "AttackUp1", "AttackDown2", "AttackLeft3" 등
            string attackTrigger = "";
            switch (m_facingDirection)
            {
                case 0: attackTrigger = "AttackUp" + m_currentAttack; break;
                case 1: attackTrigger = "AttackDown" + m_currentAttack; break;
                case 2: attackTrigger = "AttackLeft" + m_currentAttack; break;
                case 3: attackTrigger = "AttackRight" + m_currentAttack; break;
            }

            // 애니메이션 트리거 발동
            m_animator.SetTrigger(attackTrigger);

            // 공격 시간 리셋
            m_timeSinceAttack = 0f;
        }
    }
}

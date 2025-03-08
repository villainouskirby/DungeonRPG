using Cainos.PixelArtTopDown_Basic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;            // 일반 이동 속도
    public float slideForce = 30f;       // 회피 시 속도
    public float slideDuration = 0.4f;  // 회피 지속 시간

    [Header("Attack Settings")]
    public float comboResetTime = 1.0f;    // 콤보 입력 유효시간
    public float minAttackInterval = 0.5f; // 공격 간 최소 간격
    private int m_currentAttack = 0;       // 현재 콤보 단계
    private float m_timeSinceAttack = 0f;

    [Header("Buff Settings")]
    public string type1;
    public float per1;
    public float duration1;
    public Sprite icon1;
    public string type2;
    public float per2;
    public float duration2;
    public Sprite icon2;

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

        // 무기와 플레이어 간 충돌 방지, 떨어짐 방지
        //Collider2D col1 = GetComponent<Collider2D>();
        //Collider2D col2 = GameObject.Find("Weapon").GetComponent<Collider2D>();
        //Physics2D.IgnoreCollision(col1, col2, true);

        // 무기를 플레이어에 Joint로 연결
        //FixedJoint2D joint = gameObject.AddComponent<FixedJoint2D>();
        //joint.connectedBody = GameObject.Find("Weapon").GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 공격 간격 체크
        m_timeSinceAttack += Time.deltaTime;

        // 슬라이딩(회피) 시간 체크
       
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

        // 콤보 공격 처리
        
        if (Input.GetMouseButtonDown(0) && (m_timeSinceAttack > minAttackInterval) && !m_sliding)
        {
            m_currentAttack++;

            // 3단 콤보 후 다시 1타로
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // 콤보 리셋 시간 이후면 다시 1타부터
            if (m_timeSinceAttack > comboResetTime)
                m_currentAttack = 1;

            // 현재 바라보는 방향 + 콤보 단계를 통해 애니메이션 트리거 결정
            string attackTrigger = "";
            switch (m_facingDirection)
            {
                case 0: attackTrigger = "AttackUp" + m_currentAttack; break;
                case 1: attackTrigger = "AttackDown" + m_currentAttack; break;
                case 2: attackTrigger = "AttackLeft" + m_currentAttack; break;
                case 3: attackTrigger = "AttackRight" + m_currentAttack; break;
            }

            // 공격 애니메이션 실행
            m_animator.SetTrigger(attackTrigger);

            // 공격 시점 리셋
            m_timeSinceAttack = 0f;
        }

        
        // 슬라이딩(회피)
      
        if (Input.GetKeyDown(KeyCode.Space) && !m_sliding)
        {
            m_sliding = true;
            m_slidingTimer = 0f; // 회피 시작

            switch (m_facingDirection)
            {
                case 0: m_animator.SetTrigger("SlideUp"); break;
                case 1: m_animator.SetTrigger("SlideDown"); break;
                case 2: m_animator.SetTrigger("SlideLeft"); break;
                case 3: m_animator.SetTrigger("SlideRight"); break;
            }

            // 실제 회피 속도 적용
            Vector2 slideDir = Vector2.zero;
            if (m_facingDirection == 0) slideDir = Vector2.up;
            else if (m_facingDirection == 1) slideDir = Vector2.down;
            else if (m_facingDirection == 2) slideDir = Vector2.left;
            else if (m_facingDirection == 3) slideDir = Vector2.right;

            m_body2d.velocity = slideDir * slideForce;
        }

        //버프 활성화 버튼
        if (Input.GetKeyDown(KeyCode.B))
        {
            BuffManager.instance.CreateBuff(type1, per1, duration1, icon1);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            BuffManager.instance.CreateBuff(type2, per2, duration2, icon2);
        }
    }

    void FixedUpdate()
    {
        
        // 이동 입력 (Input.GetAxis)
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector2 dir = new Vector2(moveX, moveY);

        bool isWalk = dir.magnitude > 0f;
        m_animator.SetBool("iswalking", isWalk);

        // 슬라이딩 중이 아닐 때만 이동 가능
        if (!m_sliding)
        {
            m_body2d.velocity = dir.normalized * speed;
        }

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
                sr.flipX = true;
            else if (m_facingDirection == 2)
                sr.flipX = false;
        }
    }
}
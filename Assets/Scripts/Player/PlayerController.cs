using Cainos.PixelArtTopDown_Basic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerStateMachine stateMachine;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;

    [SerializeField] IPlayerState nowState;

    [Header("Movement Settings")]
    public float speed = 5f;            // 일반 이동 속도
    public float slideForce = 30f;       // 회피 시 속도
    public float slideDuration = 0.4f;  // 회피 지속 시간
    
    [SerializeField] private float beforeSpeed = 0;
    private float moveInput = 0f;

    public bool canSneak { get; set; }
    [Header("Buff Settings")]

    public float per1 = 0.3f;
    public float duration1 = 5f;
    public Sprite icon1;

    public float per2 = 0.2f;
    public float duration2 = 3f;
    public Sprite icon2;

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
        rb = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>(); // SpriteRenderer 참조

        // 무기와 플레이어 간 충돌 방지, 떨어짐 방지
        //Collider2D col1 = GetComponent<Collider2D>();
        //Collider2D col2 = GameObject.Find("Weapon").GetComponent<Collider2D>();
        //Physics2D.IgnoreCollision(col1, col2, true);

        // 무기를 플레이어에 Joint로 연결
        //FixedJoint2D joint = gameObject.AddComponent<FixedJoint2D>();
        //joint.connectedBody = GameObject.Find("Weapon").GetComponent<Rigidbody2D>();
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
    void Update()
    {

        //버프 활성화 버튼
        if (Input.GetKeyDown(KeyCode.B))
        {
            BuffManager.instance.CreateBuff(BuffType.AttackUp, per1, duration1, icon1);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            BuffManager.instance.CreateBuff(BuffType.AttackDown, per2, duration2, icon2);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            BuffManager.instance.CreateBuff(BuffType.SpeedUp, per1, duration1, icon1);
        }
    }

    void FixedUpdate()
    {
        Debug.Log(GetCurrentState());
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
}
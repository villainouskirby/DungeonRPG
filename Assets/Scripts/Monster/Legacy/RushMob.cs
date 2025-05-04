using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class RushMob : MonoBehaviour
{
    private MobStateMachine stateMachine;
    private Rigidbody2D rb;

    [SerializeField] IMobState nowState;

    [Header("Move")]
    [SerializeField] private float nowSpeed = 0f;
    [SerializeField] private float maxSpeed = 5f;         // 최고 속도
    [SerializeField] private float moveAccel;

    [SerializeField] private float beforeSpeed = 0;
    private float moveInput = 0f;


    [Header("Rush")]
    [SerializeField] private float RushDistance = 5f;
    [SerializeField] private float RushBeforeDelay = 0.1f;
    [SerializeField] private float RushDuration = 0.3f;
    [SerializeField] private float RushCooldown = 1f;
    [SerializeField] private float RushTime;

    private float RushCooldownTimer = 0f;


    public bool canRush { get; set; }
    private Vector2 RushStartPos;
    private Vector2 RushDirection;

    private Vector2 RushBeforeVelocity;

    [Header("Direction")]
    [SerializeField] private MobLookingDirection looking;



    [Header("PlayerHit")]
    [SerializeField]
    private float bindTimer = 1f;

    public GameObject Player;


    private SpriteRenderer sprite;
    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        stateMachine = new MobStateMachine();
        stateMachine.ChangeState(new RushMobIdleState(this));
        looking = MobLookingDirection.Right;
        canRush = true;


    }


    public void StartRush()
    {
        canRush = false;
        RushStartPos = transform.position;

        RushBeforeVelocity = rb.velocity;

        // 방향계산 넣어야함
        RushDirection = Vector2.zero;
        

        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;

        StartCoroutine(RushCoroutine(RushDirection));
    }
    private IEnumerator RushCoroutine(Vector2 direction)
    {
        float RushBeforeDelayCounter = 0f;
        while (RushBeforeDelayCounter <= RushBeforeDelay)
        {
            rb.velocity = Vector2.zero;
            RushBeforeDelayCounter += Time.deltaTime;
            yield return null;
        }
        StartCoroutine(Rush(direction));
        
    }
    private IEnumerator Rush(Vector2 direction)
    {
        Debug.Log("대시 시작");

        Vector2 RushStartPos = rb.position;
        Vector2 RushEndPos = rb.position + direction;
        RushTime = 0f;
        Debug.Log(RushStartPos);
        Debug.Log(RushEndPos);


        while (RushTime < RushDuration)
        {
            yield return new WaitForFixedUpdate();

            RushTime += Time.deltaTime;
            float t = RushTime / RushDuration;
            Vector2 newPosition = Vector2.Lerp(RushStartPos, RushEndPos, t);
            rb.MovePosition(newPosition);

        }
        rb.velocity = RushBeforeVelocity;
        rb.gravityScale = 4;

        if (RushEndPos.y > RushStartPos.y)
        {
        }
        else
        {
            stateMachine.RestorePreviousState();
        }
    }
}
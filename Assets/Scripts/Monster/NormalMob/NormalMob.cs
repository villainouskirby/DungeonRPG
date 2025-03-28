using System.Collections;
using UnityEngine;

public class NormalMob : MonoBehaviour
{
    // 플레이어 추적/공격 대상
    [SerializeField] private Transform player;

    // HP, 시야 상실 거리(lostDistance), 공격 범위(stoppingDistance)
    [SerializeField] private float hp = 10f;
    [SerializeField] private float lostDistance = 10f;
    [SerializeField] private float stoppingDistance = 1.5f;

    // 이동 속도
    [SerializeField] private float detectSpeed = 1.5f;  // 소리를 듣고 탐색 중일 때
    [SerializeField] private float combatSpeed = 3f;    // 전투 시

    // Animator (있다면)
    private Animator anim;

    // 현재 상태
    private State state;

    // 상태 정의
    private enum State
    {
        Idle,    // 대기
        Detect,  // 플레이어 소리 등으로 감지
        Combat,  // 전투
        Killed   // 사망
    }

    private void Start()
    {
        anim = GetComponent<Animator>();

        // 시작 상태 설정
        state = State.Idle;
        // 상태 머신 시작
        StartCoroutine(StateMachine());
    }

    /// <summary>
    /// 메인 상태 머신 코루틴
    /// </summary>
    private IEnumerator StateMachine()
    {
        // HP가 0 이하가 되기 전까지
        while (hp > 0)
        {
            yield return StartCoroutine(state.ToString());
        }

        // HP가 0 이하인 경우 사망
        ChangeState(State.Killed);
        yield return StartCoroutine("Killed");
    }

    //------------------------------------------------------------------------------
    //  상태별 코루틴
    //------------------------------------------------------------------------------

    /// <summary>
    /// Idle: 가만히 대기하거나, 랜덤으로 회전 등의 간단한 동작
    /// </summary>
    private IEnumerator Idle()
    {
        PlayAnimation("IdleNormal");

        // 예시: Idle 상태에서 2초 정도 가만히 있기
        float idleTime = 2f;
        float timer = 0f;

        while (timer < idleTime && state == State.Idle)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // 여기서는 Idle 상태가 계속 반복될 뿐, 특별한 변화가 없으면 다시 Idle로 돌아옴
        // 상태 전환은 OnTriggerEnter나 다른 곳에서 일어날 수 있음
    }

    /// <summary>
    /// Detect: 플레이어 소리를 들었을 때, 느린 속도로 플레이어에게 접근
    /// </summary>
    private IEnumerator Detect()
    {
        PlayAnimation("WalkFWD");

        // Detect 상태 유지
        while (state == State.Detect)
        {
            // 플레이어가 없으면 Idle로 되돌아가기
            if (player == null)
            {
                ChangeState(State.Idle);
                yield break;
            }

            // 플레이어와의 거리 계산
            float distance = Vector3.Distance(transform.position, player.position);

            // 너무 멀어졌으면 (잃어버림)
            if (distance > lostDistance)
            {
                player = null;
                ChangeState(State.Idle);
                yield break;
            }

            // 전투 범위 안에 들어오면 Combat으로 전환
            if (distance <= stoppingDistance)
            {
                ChangeState(State.Combat);
                yield break;
            }

            // 플레이어 쪽으로 천천히 이동
            MoveToward(player.position, detectSpeed);
            yield return null;
        }
    }

    /// <summary>
    /// Combat: 플레이어가 근접했을 때 전투. 이동 속도를 높이고 공격
    /// </summary>
    private IEnumerator Combat()
    {
        // 전투 상태 진입 시 애니메이션
        PlayAnimation("AttackRun");

        while (state == State.Combat)
        {
            if (player == null)
            {
                ChangeState(State.Idle);
                yield break;
            }

            float distance = Vector3.Distance(transform.position, player.position);

            // 플레이어가 멀어졌다면 -> Detect 또는 Idle로
            if (distance > stoppingDistance + 0.1f)
            {
                // 살짝 떨어진 상태: 다시 Detect(느린 추적)로 갈 수도 있고,
                // 디자인에 따라 바로 Detect가 아닌 Idle로 갈 수도 있음
                ChangeState(State.Detect);
                yield break;
            }

            // 근접 상태: 공격 애니메이션
            if (distance <= stoppingDistance)
            {
                // 공격 모션
                PlayAnimation("Attack01");
                // 공격 간격
                yield return new WaitForSeconds(1.0f);

                // 공격 처리 (ex. player의 HP를 깎는다 등)
                Debug.Log("플레이어에게 공격을 가했다.");

                // 공격 후 다시 전투 자세로 돌아옴
                PlayAnimation("AttackRun");
            }

            // 플레이어가 아주 멀어지지 않은 이상, Combat 유지
            // 그래도 살짝은 플레이어에게 다가간다고 가정
            MoveToward(player.position, combatSpeed);

            yield return null;
        }
    }

    /// <summary>
    /// Killed: 사망 처리
    /// </summary>
    private IEnumerator Killed()
    {
        PlayAnimation("Die");
        // 2초 후에 오브젝트 파괴
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    //------------------------------------------------------------------------------
    //  상태 전환 및 공용 함수
    //------------------------------------------------------------------------------

    /// <summary>
    /// 상태 전환
    /// </summary>
    private void ChangeState(State newState)
    {
        state = newState;
    }

    /// <summary>
    /// 목표 위치로 이동 (NavMeshAgent 없이)
    /// </summary>
    private void MoveToward(Vector3 targetPosition, float speed)
    {
        // 바라보기
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * 5f);
        }

        // 이동
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );
    }

    /// <summary>
    /// 애니메이션 재생 (Animator가 있다면)
    /// </summary>
    private void PlayAnimation(string animName)
    {
        if (!anim) return;

        var current = anim.GetCurrentAnimatorStateInfo(0);
        if (!current.IsName(animName))
        {
            anim.Play(animName, 0, 0f);
        }
    }

    /// <summary>
    /// HP 감소 테스트용
    /// </summary>
    public void TakeDamage(float damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            // 이미 Killed가 아니라면
            if (state != State.Killed)
            {
                ChangeState(State.Killed);
            }
        }
    }

    //------------------------------------------------------------------------------
    //  트리거 이벤트 (플레이어/소리 감지)
    //------------------------------------------------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        // 예: 플레이어 소리를 감지
        if (other.CompareTag("PlayerSound"))
        {
            // 전투 중이 아니고, 죽지 않았다면 Detect 상태로
            if (state != State.Combat && state != State.Killed)
            {
                // 실제 Player Transform 찾기 (예: PlayerSound가 플레이어 하위에 있다고 가정)
                // 여기서는 단순히 Tag("Player")를 찾아 할당
                player = GameObject.FindGameObjectWithTag("Player").transform;

                ChangeState(State.Detect);
            }
        }
        // 예: 플레이어 본체를 감지
        else if (other.CompareTag("Player"))
        {
            player = other.transform;
            // 즉시 Combat 상태
            if (state != State.Killed)
            {
                ChangeState(State.Combat);
            }
        }
    }
}
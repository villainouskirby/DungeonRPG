using System.Collections;
using UnityEngine;

public class NormalMob : MonoBehaviour
{
    // 플레이어 추적/공격 대상
    [SerializeField] private Transform player;

    // HP, 시야 상실 거리(lostDistance), 공격 범위(stoppingDistance)
    [SerializeField] private float hp = 10f;
    [SerializeField] private float lostDistance = 7f;
    [SerializeField] private float stoppingDistance = 1.5f;

    // 이동 속도
    [SerializeField] private float detectSpeed = 1f;  // 소리를 듣고 탐색 중일 때
    [SerializeField] private float combatSpeed = 5f;    // 전투 시

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

    // 메인 상태 머신 코루틴
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

    //  상태별 코루틴

    // Idle: 가만히 대기하거나, 랜덤으로 회전 등의 간단한 동작
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

    // Detect: 플레이어 소리를 들었을 때, 느린 속도로 플레이어에게 접근
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
            float distance = Vector2.Distance(transform.position, player.position);

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

    // Combat: 플레이어가 근접했을 때 전투. 이동 속도를 높이고 공격
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

            float distance = Vector2.Distance(transform.position, player.position);

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

    // 사망 처리
    
    private IEnumerator Killed()
    {
        PlayAnimation("Die");
        // 2초 후에 오브젝트 파괴
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    // 상태 전환
    
    private void ChangeState(State newState)
    {
        state = newState;
    }


    // 목표 위치로 이동 (NavMeshAgent 없이)
    // 혹시나 NavMeshAgent을 이용하여 장애물들을 피해 플레이어를 추적하는 알고리즘 필요할지도
    private void MoveToward(Vector3 targetPosition, float speed)
    {
        // 현재 위치와 목표 위치의 x, y 값만 사용
        Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 targetPos = new Vector2(targetPosition.x, targetPosition.y);

        // 2D 방향 계산
        Vector2 direction = (targetPos - currentPos).normalized;

        // 2D 회전 처리
        // GPT 선생님이 넣은 로직인데 아마 이렇겐안하겠지
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // 스프라이트의 기본 방향에 따라 회전 오프셋을 조절 (예: -90°)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle - 90), Time.deltaTime * 5f);
        }

        // 2D 이동
        Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, speed * Time.deltaTime);
        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
    }


    //애니메이션 재생용
    private void PlayAnimation(string animName)
    {
        if (!anim) return;

        var current = anim.GetCurrentAnimatorStateInfo(0);
        if (!current.IsName(animName))
        {
            anim.Play(animName, 0, 0f);
        }
    }

    // HP 감소 로직이 있다면?
    public void TakeDamage(float damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            if (state != State.Killed)
            {
                ChangeState(State.Killed);
            }
        }
    }


    //  트리거 이벤트 (플레이어/소리 감지)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerSound"))
        {
            Debug.Log("플레이어 소리 감지");
            if (state != State.Combat && state != State.Killed)
            {
                player = GameObject.FindGameObjectWithTag("Player").transform;
                ChangeState(State.Detect);
            }
        }
        else if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어 감지");
            player = other.transform;
            if (state != State.Killed)
            {
                ChangeState(State.Combat);
            }
        }
    }
}
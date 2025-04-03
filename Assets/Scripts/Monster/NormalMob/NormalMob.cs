using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NormalMob : MonoBehaviour
{
    // 추적 대상
    [SerializeField] private Transform player;    // 플레이어
    [SerializeField] private Transform spawner;   // 스포너(복귀 기준점)

    private NavMeshAgent agent; // NavMeshAgent를 통해 장애물 회피 및 경로 탐색

    // HP, 시야 상실 거리(lostDistance), 공격 범위(stoppingDistance)
    [SerializeField] private float hp = 10f;
    [SerializeField] private float lostDistance = 5f;
    [SerializeField] private float stoppingDistance = 1f;

    // 이동 속도(상황에 따라 변동)
    [SerializeField] private float detectSpeed = 1f;   // 소리를 듣고 탐색 중
    [SerializeField] private float combatSpeed = 3f;   // 전투 시

    // **스포너와의 거리 관련**
    [SerializeField] private float maxSpawnerDistance = 15f; // 이 거리 이상 멀어지면 Return
    [SerializeField] private float nearSpawnerDistance = 2f; // 이 거리 안에 도달하면 도착으로 간주

    [SerializeField] private LayerMask obstacleMask;  // 플레이어와 사이에 장애물이 있는지 확인
    private Animator anim;

    // 상태 정의
    private enum State
    {
        Idle,    // 대기
        Detect,  // 특정 소리 등을 감지했을 때 그 지점으로 이동
        Return,  // 스포너(기준점)로 복귀
        Combat,  // 전투 (플레이어 추적 및 공격)
        Killed   // 사망
    }

    // 현재 상태
    private State state;

    // 감지 위치(Detect 시 사용)
    private Vector3 detectedPosition;

    private void Start()
    {
        anim = GetComponent<Animator>();

        // NavMeshAgent 설정
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; // 2D에서 필요 없는 회전
        agent.updateUpAxis = false;   // 2D에서 Z축이 아니라 Y축을 사용하도록 강제

        // 초기 상태 설정
        ChangeState(State.Idle);
        StartCoroutine(StateMachine());
    }

    // 메인 상태 머신 코루틴
    private IEnumerator StateMachine()
    {
        while (hp > 0)
        {
            yield return StartCoroutine(state.ToString());
        }

        // HP가 0 이하이면 사망
        ChangeState(State.Killed);
        yield return StartCoroutine(nameof(Killed));
    }

    // Idle
    private IEnumerator Idle()
    {
        PlayAnimation("IdleNormal");

        while (state == State.Idle)
        {
            // 스포너와 너무 멀어졌는지 확인**
            if (CheckTooFarFromSpawner())
            {
                ChangeState(State.Return);
                yield break;
            }

            // 랜덤으로 살짝 이동
            float waitTime = Random.Range(1f, 3f);
            yield return new WaitForSeconds(waitTime);

            // 대기 상태에서 미세 이동 (에이전트로 이동)
            Vector3 randomDirection = Random.insideUnitCircle.normalized;
            float moveDistance = 1f; // 1f 정도 떨어진 지점
            Vector3 destination = transform.position + randomDirection * moveDistance;

            agent.speed = 2f;
            agent.SetDestination(destination);

            PlayAnimation("WalkFWD");

            while (agent.remainingDistance > agent.stoppingDistance)
            {
                // (2) 진행 중에도 스포너와 너무 멀어졌는지 계속 확인
                if (CheckTooFarFromSpawner())
                {
                    ChangeState(State.Return);
                    yield break;
                }

                // 상태가 바뀌면 탈출
                if (state != State.Idle) yield break;
                yield return null;
            }

            PlayAnimation("IdleNormal");
        }
    }

    // 2) Detect: 소리 감지 지점으로 이동
    private IEnumerator Detect()
    {
        PlayAnimation("WalkFWD");

        float detectDuration = 5f;
        float timer = 0f;

        agent.speed = detectSpeed;
        agent.SetDestination(detectedPosition);

        while (timer < detectDuration && state == State.Detect)
        {
            // 스포너와 너무 멀어졌는지 확인
            if (CheckTooFarFromSpawner())
            {
                ChangeState(State.Return);
                yield break;
            }

            // Combat 상태 전환 검사
            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.position);
                if (distance <= stoppingDistance && CanSeePlayer(player))
                {
                    ChangeState(State.Combat);
                    yield break;
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // detectDuration동안 플레이어를 찾지 못했다면 복귀
        ChangeState(State.Return);
    }

    // Combat: 플레이어 추적 및 공격
    private IEnumerator Combat()
    {
        PlayAnimation("AttackRun");

        float lostTimer = 0f;
        float maxLostTime = 1f;

        while (state == State.Combat)
        {
            if (player == null)
            {
                ChangeState(State.Return);
                yield break;
            }

            // 스포너와 너무 멀어졌는지 확인
            if (CheckTooFarFromSpawner())
            {
                ChangeState(State.Return);
                yield break;
            }

            // 플레이어 추적
            agent.speed = combatSpeed;
            agent.SetDestination(player.position);

            float distance = Vector2.Distance(transform.position, player.position);
            bool canSee = CanSeePlayer(player);

            if (!canSee || distance > lostDistance)
            {
                lostTimer += Time.deltaTime;
                if (lostTimer >= maxLostTime)
                {
                    ChangeState(State.Return);
                    yield break;
                }
            }
            else
            {
                lostTimer = 0f;
            }

            // 공격
            if (distance <= stoppingDistance)
            {
                PlayAnimation("Attack01");
                yield return new WaitForSeconds(1f);
                Debug.Log("플레이어 공격!");
                PlayAnimation("AttackRun");
            }
            yield return null;
        }
    }

    // Return: 스포너 지점으로 복귀
    private IEnumerator Return()
    {
        PlayAnimation("WalkFWD");

        agent.speed = detectSpeed;

        while (state == State.Return)
        {
            // 스포너로 이동
            if (spawner != null)
            {
                agent.SetDestination(spawner.position);
            }

            // 스포너 근처(2f 이내)에 도착하면 Idle
            if (spawner != null && Vector2.Distance(transform.position, spawner.position) <= nearSpawnerDistance)
            {
                Debug.Log($"스포너 지점 {nearSpawnerDistance}f 안쪽 도착 → Idle 전환");
                ChangeState(State.Idle);
                yield break;
            }

            yield return null;
        }
    }

    // Killed
    private IEnumerator Killed()
    {
        PlayAnimation("Die");
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    private void ChangeState(State newState)
    {
        state = newState;
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;
        if (hp <= 0 && state != State.Killed)
        {
            ChangeState(State.Killed);
        }
    }

    // 2D 시야 판별
    private bool CanSeePlayer(Transform target)
    {
        if (target == null) return false;

        Vector2 start = transform.position;
        Vector2 end = target.position;
        Vector2 direction = (end - start).normalized;
        float distance = Vector2.Distance(start, end);

        RaycastHit2D hit = Physics2D.Raycast(start, direction, distance, obstacleMask);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("플레이어 시야 포착!");
                return true;
            }
            else
            {
                Debug.Log("장애물에 막힘!");
                return false;
            }
        }
        return false;
    }

    // 스포너와의 거리가 너무 먼지 체크 (15f 이상이면 true)
    private bool CheckTooFarFromSpawner()
    {
        if (!spawner) return false;
        float distance = Vector2.Distance(transform.position, spawner.position);
        return distance >= maxSpawnerDistance;
    }

    // 트리거 감지
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerSound"))
        {
            Debug.Log("플레이어 소리 감지");
            if (state != State.Combat && state != State.Killed)
            {
                detectedPosition = other.transform.position;
                ChangeState(State.Detect);
            }
        }
        else if (other.CompareTag("Spawner"))
        {
            // Return 상태에서 트리거로 스포너를 감지했을 때도 Idle 처리 가능
            if (state == State.Return)
            {
                Debug.Log("스포너 지점 도착 → Idle 전환");
                ChangeState(State.Idle);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (CanSeePlayer(other.transform))
            {
                Debug.Log("플레이어 감지 (시야에 막힘 없음)");
                player = other.transform;
                if (state != State.Killed)
                {
                    ChangeState(State.Combat);
                }
            }
            else
            {
                Debug.Log("플레이어가 범위 안에 있지만 장애물에 가려짐");
            }
        }
    }

    // 애니메이션 재생
    private void PlayAnimation(string animName)
    {
        if (!anim) return;

        var current = anim.GetCurrentAnimatorStateInfo(0);
        if (!current.IsName(animName))
        {
            anim.Play(animName, 0, 0f);
        }
    }
}
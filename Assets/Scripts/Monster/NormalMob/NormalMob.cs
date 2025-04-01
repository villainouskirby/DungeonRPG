using System.Collections;
using UnityEngine;

public class NormalMob : MonoBehaviour
{
    // 추적 대상
    [SerializeField] private Transform player; //플레이어
    [SerializeField] private Transform spawner; // 스포너

    // HP, 시야 상실 거리(lostDistance), 공격 범위(stoppingDistance)
    [SerializeField] private float hp = 10f;
    [SerializeField] private float lostDistance = 7f;
    [SerializeField] private float stoppingDistance = 1f;

    // 이동 속도
    [SerializeField] private float detectSpeed = 1f;  // 소리를 듣고 탐색 중일 때
    [SerializeField] private float combatSpeed = 5f;    // 전투 시

    [SerializeField] private LayerMask obstacleMask;
    // Animator (있다면)
    private Animator anim;

    // 현재 상태
    private State state;
    private Vector3 detectedPosition;
    // 상태 정의
    private enum State
    {
        Idle,    // 대기
        Detect,  // 플레이어 소리 등으로 감지(2초간 해당 위치로 이동)
        Return,  // 스포너(기준점)로 복귀
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

    // Idle : 랜덤으로 상하좌우 소폭 이동
    private IEnumerator Idle()
    {
        PlayAnimation("IdleNormal");

        while (state == State.Idle)
        {
            // 대기 시간 랜덤(예: 1~3초)
            float waitTime = Random.Range(1f, 3f);
            yield return new WaitForSeconds(waitTime);

            // 이동할 방향(상하좌우) 랜덤 선택
            int randDir = Random.Range(0, 4);
            Vector3 moveDir = Vector3.zero;
            switch (randDir)
            {
                case 0: moveDir = Vector3.up; break; // 위
                case 1: moveDir = Vector3.down; break; // 아래
                case 2: moveDir = Vector3.left; break; // 왼
                case 3: moveDir = Vector3.right; break; // 오른
            }

            // 1f만큼 천천히 이동 (이동 속도는 detectSpeed 정도로 활용하거나 별도 설정)
            float moveDistance = 1f;
            float moveSpeed = 2f; // Idle에서 살짝 움직이는 속도(임의)
            float moved = 0f;

            PlayAnimation("WalkFWD");

            while (moved < moveDistance && state == State.Idle)
            {
                float step = moveSpeed * Time.deltaTime;
                transform.Translate(moveDir * step, Space.World);
                moved += step;
                yield return null;
            }

            // 다시 Idle 애니메이션
            PlayAnimation("IdleNormal");
        }
        // 여기서는 Idle 상태가 계속 반복될 뿐, 특별한 변화가 없으면 다시 Idle로 돌아옴
        // 상태 전환은 OnTriggerEnter나 다른 곳에서 일어날 수 있음
    }

    // Detect: 플레이어 소리를 들었을 때, 소리가 난 지점으로 이동
    private IEnumerator Detect()
    {
        PlayAnimation("WalkFWD");

        // Detect 상태 유지
        float detectDuration = 3f;
        float timer = 0f;

        while (timer < detectDuration && state == State.Detect)
        {
            // 소리 감지 지점으로 이동
            MoveToward(detectedPosition, detectSpeed);

            // 이동 중, 플레이어가 Combat 범위에 들어오고 시야에 보이면 Combat
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
        ChangeState(State.Return);
        // **3초가 지났는데** 플레이어를 찾지 못했다면 Return으로 전환

    }

    // Combat: 플레이어가 시야에 보이면 전투. 이동 속도를 높이고 공격
    private IEnumerator Combat()
    {
        // 전투 상태 진입 시 애니메이션
        PlayAnimation("AttackRun");

        float lostTimer = 0f;
        float maxLostTime = 1f; // 1초 정도 시야가 끊겨도 곧바로 돌아가지 않게

        while (state == State.Combat)
        {
            if (player == null)
            {
                // 플레이어 객체가 사라졌다면 Return
                ChangeState(State.Return);
                yield break;
            }

            float distance = Vector2.Distance(transform.position, player.position);
            bool canSee = CanSeePlayer(player);

            // '시야 상실' 혹은 '거리 초과'가 발생하면 타이머를 누적
            if (distance > lostDistance || !canSee)
            {
                lostTimer += Time.deltaTime;
                // maxLostTime을 넘겼다면 Return 상태로
                if (lostTimer >= maxLostTime)
                {
                    ChangeState(State.Return);
                    yield break;
                }
            }
            else
            {
                // 플레이어가 다시 보이면 타이머 리셋
                lostTimer = 0f;
            }

            // 근접 공격
            if (distance <= stoppingDistance)
            {
                PlayAnimation("Attack01");
                yield return new WaitForSeconds(1f);

                Debug.Log("플레이어 공격!");
                PlayAnimation("AttackRun");
            }
            else
            {
                // Combat 속도로 플레이어에게 접근
                MoveToward(player.position, combatSpeed);
            }

            yield return null;
        }
    }

    // 스포너 복귀
    private IEnumerator Return()
    {
        PlayAnimation("WalkFWD");

        while (state == State.Return)
        {
            //    Return 중에도 플레이어가 일정 거리 안에 있고,
            //    시야가 트이면 즉시 Combat
            if (player != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);
                // lostDistance 이하로 들어왔고, 가려지지 않았다면 Combat
                if (distanceToPlayer <= lostDistance && CanSeePlayer(player))
                {
                    ChangeState(State.Combat);
                    yield break;
                }
            }

            // 스포너로 이동
            if (spawner != null)
            {
                float distance = Vector2.Distance(transform.position, spawner.position);
                // 스포너 중심으로부터 1f 이내면 Idle로 전환
                if (distance <= 1f)
                {
                    ChangeState(State.Idle);
                    yield break;
                }
                MoveToward(spawner.position, detectSpeed);
            }

            yield return null;
        }
    }
    private IEnumerator Killed()
    {
        PlayAnimation("Die");
        // 오브젝트 파괴
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
            // 뭔가 충돌한 콜라이더가 있다면, 그게 플레이어인지 판별
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("플레이어 시야 포착!");
                return true;
            }
            else
            {
                Debug.Log("장애물에 걸림!");
                return false;
            }
        }

        return false;
    }
    //  트리거 이벤트 (플레이어/소리 감지)
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
        else if (other.CompareTag("Player"))
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
                // 장애물에 의해 가려져 있는 경우
                Debug.Log("플레이어가 감지 범위 안에 들어왔으나, 장애물에 가려져 보이지 않음");
            }
        }
        else if (other.CompareTag("Spawner"))
        {
            // Return 상태에서 스포너에 닿으면 Idle로 전환
            if (state == State.Return)
            {
                Debug.Log("스포너 지점 도착 → Idle 전환");
                ChangeState(State.Idle);
            }
        }
    }
}
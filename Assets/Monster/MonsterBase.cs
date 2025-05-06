using System.Collections;
using UnityEditor.Connect;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public abstract class MonsterBase : MonoBehaviour
{
    [SerializeField] protected MonsterData data;
    [SerializeField] protected Transform spawner;
    [SerializeField] protected LayerMask obstacleMask;

    protected Transform player;
    protected NavMeshAgent agent;
    protected Animator anim;
    protected float hp;

    protected enum State { Idle, Detect, Combat, Flee, Return, Escaped, Killed }
    protected State state;
    Coroutine stateRoutine;
    protected Vector3 detectedPos;   // Detect 시 목표 지점
    protected bool isfastReturn = false; // 스포너로부터 멀어져서 일어나는 귀환이면 빠르게 귀환

    /* ----------- 애니메이션 이름(하위 클래스별 상이) ----------- */
    protected virtual string IdleAnim => "Idle";
    protected virtual string WalkAnim => "Walk";
    protected virtual string RunAnim => "Run";
    protected virtual string AttackAnim => "Attack";
    protected virtual string DieAnim => "Die";

    #region Unity Lifecycle
    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        agent.updateRotation = false;  
        agent.updateUpAxis = false;
        HealthBarManager.Instance?.Register(this);
        hp = data.maxHp;

        if (data.animator) anim.runtimeAnimatorController = data.animator;
    }
    
    protected virtual void Start()
    {
        StartState(State.Idle);      // 처음 상태
    }

    void StartState(State s)
    {
        if (stateRoutine != null) StopCoroutine(stateRoutine);
        state = s;
        stateRoutine = StartCoroutine(s.ToString());
    }
    bool SeePlayer(float maxDist) => player && CanSeePlayer(player, maxDist);

    #endregion

    #region 상태 머신
    private IEnumerator StateMachine()
    {
        while (hp > 0 && state != State.Escaped)
            yield return StartCoroutine(state.ToString());
        
        if (state == State.Killed)
            yield return StartCoroutine(nameof(Killed));
        else if (state == State.Escaped)
            yield return StartCoroutine(nameof(Escaped));
    }
    IEnumerator BreakableWait(float seconds, State myState)
    {
        float t = 0f;
        while (t < seconds && state == myState)
        {
            t += Time.deltaTime;
            yield return null;
        }
    }

    /* ----- Idle ----- */
    protected virtual IEnumerator Idle()
    {
        Play(IdleAnim);
        isfastReturn = false;
        while (state == State.Idle)
        {
            // 스포너 초과 거리면 복귀
            if (TooFarFromSpawner())
            {
                ChangeState(State.Return); yield break;
            }

            if (SeePlayer(data.sightDistance))            {
                ChangeState(data.isagrresive ? State.Combat : State.Flee);
                yield break;
            }

            // 가벼운 산책
            Vector3 dest = transform.position + (Vector3)(Vector2)Random.insideUnitCircle.normalized;
            agent.speed = data.detectSpeed;
            agent.SetDestination(dest);

            Play(WalkAnim);
            float timeout = 1f;
            float timer = 0f;

            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance) // 해당 로직으로 목표한 지점까지 계속 가려는 무한루프 끝낼수있음
            {
                if (state != State.Idle) yield break;

                timer += Time.deltaTime;
                if (timer > timeout)
                {
                    Debug.Log("Idle 산책 타임아웃: 경로 도달 실패 → 다음 이동");
                    break;
                }

                yield return null;
            }

            Play(IdleAnim);
            yield return BreakableWait(Random.Range(0.5f, 2f), State.Idle);
        }
    }

    /* ----- Detect (소리 추적) ----- */
    protected virtual IEnumerator Detect()
    {
        Play(WalkAnim);
        agent.speed = data.detectSpeed;
        agent.SetDestination(detectedPos);

        while (state == State.Detect)
        {
            if (SeePlayer(data.stoppingDistance))
            {
                ChangeState(data.isagrresive ? State.Combat : State.Flee);
                yield break;
            }

            if (TooFarFromSpawner())
            {
                isfastReturn = true; 
                ChangeState(State.Return); 
                yield break; 
            }
            yield return null;

            yield return null;
        }
        ChangeState(State.Return);
    }

    /* ----- Combat ----- */
    protected virtual IEnumerator Combat()
    {
        Play(RunAnim);
        float lostTimer = 0f, lostMax = 1f;

        while (state == State.Combat)
        {
            if (!player) { ChangeState(State.Return); yield break; }
            if (TooFarFromSpawner()) 
            { 
                isfastReturn = true;
                ChangeState(State.Return);
                yield break; 
            }

            agent.speed = data.combatSpeed;
            agent.SetDestination(player.position);

            float dist = Vector2.Distance(transform.position, player.position);
            bool see = CanSeePlayer(player, data.lostDistance);

            if (!see || dist > data.lostDistance)
            {
                lostTimer += Time.deltaTime;
                if (lostTimer >= lostMax) { ChangeState(State.Return); yield break; }
            }
            else lostTimer = 0f;

            // 공격
            if (dist <= data.stoppingDistance)
            {
                Play(AttackAnim);
                yield return new WaitForSeconds(1f);  // 공격 모션 길이
                Debug.Log($"{data.monsterName} 공격 → {data.attackPower} 데미지");
                Play(RunAnim);
            }
            yield return null;
        }
    }

    /* ----- Flee ----- */
    protected virtual IEnumerator Flee() { yield break; }
    /* ----- Return (스포너 복귀) ----- */
    protected virtual IEnumerator Return()
    {
        Play(WalkAnim);
        if (isfastReturn)
        {
            agent.speed = data.FleeSpeed;
        }
        else
        {
            agent.speed = data.detectSpeed;
        }

        float checkInterval = 2f;
        float timer = 0f;

        while (state == State.Return)
        {
            timer += Time.deltaTime; //간격을 두고 플레이어 체크
            if (timer >= checkInterval)
            {
                timer = 0f;
                if (SeePlayer(data.sightDistance))
                {
                    ChangeState(data.isagrresive ? State.Combat : State.Flee);
                    yield break;
                }
            }
            if (spawner) agent.SetDestination(spawner.position);

            if (Vector2.Distance(transform.position, spawner.position)
                <= data.nearSpawnerDist)
            {
                ChangeState(State.Idle); yield break;
            }
            yield return null;
        }
    }

    /* ----- Killed ----- */
    protected virtual IEnumerator Killed()
    {
        Play(DieAnim);
        yield return new WaitForSeconds(2f);
        HealthBarManager.Instance?.Unregister(this);
        Destroy(gameObject);
    }
    /* ----- Escaped ----- */
    protected virtual IEnumerator Escaped() { yield break; }


    #endregion



    #region 공통 메서드
    protected void ChangeState(State s)
    {
        if (state == s) return;
        StartState(s);
    }

    protected bool CanSeePlayer(Transform tgt, float maxDist)
    {
        if (!tgt) return false;
        Vector2 start = transform.position, dir = (tgt.position - transform.position).normalized;
        float dist = Vector2.Distance(start, tgt.position);

        if (dist > maxDist) return false;

        var hit = Physics2D.Raycast(start, dir, dist, obstacleMask);
        return hit.collider && hit.collider.CompareTag("Player");
    }

    protected bool TooFarFromSpawner()
    {
        if (!spawner) return false;
        return Vector2.Distance(transform.position, spawner.position) >= data.maxSpawnerDist;
    }

    protected void Play(string name)
    {
        if (!anim) return;
        var cur = anim.GetCurrentAnimatorStateInfo(0);
        if (!cur.IsName(name)) anim.Play(name, 0, 0f);
    }

    protected IEnumerator WaitUntilReached()
    {
        while (agent.remainingDistance > agent.stoppingDistance) yield return null;
    }

    /* ----------- 데미지 처리 ----------- */
    public virtual void TakeDamage(float dmg)
    {
        hp -= dmg;
        HealthBarManager.Instance?.UpdateBar(this, GetHPRatio());
        Debug.Log("현재 몬스터 체력" + hp);
        if (hp <= 0 && state != State.Killed) ChangeState(State.Killed);
    }
    public float GetCurrentHP() => hp;             // 절대값
    public float GetHPRatio() => hp / data.maxHp;

    /* ----------- 트리거 (소리·플레이어) ----------- */
    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("PlayerSound") && state is not (State.Combat or State.Killed))
        {
            detectedPos = col.transform.position;
            ChangeState(State.Detect);
        }
        else if (col.CompareTag("Spawner") && state == State.Return)
        {
            ChangeState(State.Idle);
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D col)
    {
        if (isfastReturn) { return; }

        if (col.CompareTag("Player") && CanSeePlayer(col.transform, data.sightDistance))
        {
            player = col.transform;
            if (state != State.Killed) ChangeState(State.Combat);
        }
    }
    #endregion
}

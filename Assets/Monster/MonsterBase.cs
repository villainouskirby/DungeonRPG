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
        hp = data.maxHp;

        agent.stoppingDistance = data.stoppingDistance;
        agent.updateRotation = false;  
        agent.updateUpAxis = false;

        HealthBarManager.Instance?.Register(this);

        if (data.animator) anim.runtimeAnimatorController = data.animator;
    }
    protected virtual void Update()
    {
        if (state is State.Killed or State.Escaped) return;

        EnsurePlayer();
        SensePlayer();
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
    protected void ChangeState(State s)
    {
        if (state == s) return;

        if (stateRoutine != null) StopCoroutine(stateRoutine);
        state = s;
        stateRoutine = StartCoroutine(s switch
        {
            State.Idle => Idle(),
            State.Detect => Detect(),
            State.Combat => Combat(),
            State.Flee => Flee(),
            State.Return => Return(),
            State.Escaped => Escaped(),
            State.Killed => Killed(),
            _ => null
        });
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

        while (state == State.Idle)
        {
            if (TooFarFromSpawner()) { ChangeState(State.Return); yield break; }

            Vector3 dest;
            do
            {
                Vector2 rnd = Random.insideUnitCircle * data.wanderRadius; // 길이 0~R
                dest = transform.position + (Vector3)rnd;
            }
            while (Vector2.Distance(transform.position, dest) < agent.stoppingDistance + 0.5f);

            agent.speed = data.detectSpeed;
            agent.SetDestination(dest);
            Play(WalkAnim);

            /* 도착 or 타임아웃 대기 */
            yield return WaitUntilArrivedOr(3f, () => state != State.Idle);

            Play(IdleAnim);
            yield return BreakableWait(Random.Range(0.5f, 2f), State.Idle);
        }
    }

    protected virtual IEnumerator Detect()
    {
        const float detectMax = 4f;              // ▶ 4 초 한도
        Play(WalkAnim);

        agent.speed = data.detectSpeed;
        agent.SetDestination(detectedPos);

        float t = 0f;
        while (state == State.Detect)
        {
            // 시야 확보 → 바로 Combat/Flee
            if (SeePlayer(data.sightDistance))
            {
                ChangeState(data.isaggressive ? State.Combat : State.Flee);
                yield break;
            }

            /* 2) 4 초 경과 or 목적지 도달 */
            bool reached = ReachedDestination();
            t += Time.deltaTime;

            if (t >= detectMax || reached)
            {
                bool nearSpawner = spawner &&
                                   Vector2.Distance(transform.position,
                                                    spawner.position)
                                   <= data.nearSpawnerDist;

                ChangeState(nearSpawner ? State.Idle : State.Return);
                yield break;
            }

            yield return null;
        }
    }
    protected virtual IEnumerator Combat()
    {
        Play(RunAnim);
        float lostTimer = 0f;

        while (state == State.Combat)
        {
            if (!player) { ChangeState(State.Return); yield break; }
            if (TooFarFromSpawner()) { isfastReturn = true; ChangeState(State.Return); yield break; }

            agent.speed = data.combatSpeed;
            agent.SetDestination(player.position);

            float dist = Vector2.Distance(transform.position, player.position);
            bool see = CanSeePlayer(player, data.lostDistance);

            if (!see || dist > data.lostDistance)
            {
                lostTimer += Time.deltaTime;
                if (lostTimer >= 1f) { ChangeState(State.Return); yield break; }
            }
            else lostTimer = 0f;

            /* 공격 */
            if (dist <= data.attackRange)
            {
                Play(AttackAnim);
                yield return new WaitForSeconds(data.attackCooldown);
                Debug.Log($"{data.monsterName} → {data.attackPower} 데미지");
                Play(RunAnim);
            }
            yield return null;
        }
    }

    protected virtual IEnumerator Flee() { yield break; }
 
    protected virtual IEnumerator Return()
    {
        Play(WalkAnim);
        if (isfastReturn)
        {
            agent.speed = data.fleeSpeed;
        }
        else
        {
            agent.speed = data.detectSpeed;
        }

        float checkInterval = 4f;
        float timer = 0f;

        while (state == State.Return)
        {
            timer += Time.deltaTime; //간격을 두고 플레이어 체크
            if (timer >= checkInterval)
            {
                timer = 0f;
                if (SeePlayer(data.sightDistance))
                {
                    ChangeState(data.isaggressive ? State.Combat : State.Flee);
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

    protected virtual IEnumerator Killed()
    {
        Play(DieAnim);
        yield return new WaitForSeconds(2f);
        HealthBarManager.Instance?.Unregister(this);
        Destroy(gameObject);
    }
    protected virtual IEnumerator Escaped() { yield break; }


    #endregion

    #region 공통 메서드
    
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
        //var cur = anim.GetCurrentAnimatorStateInfo(0);
        //if (!cur.IsName(name)) anim.Play(name, 0, 0f);
    }

    protected IEnumerator WaitUntilReached()
    {
        while (agent.remainingDistance > agent.stoppingDistance) yield return null;
    }

    public virtual void TakeDamage(float dmg)
    {
        hp -= dmg;
        HealthBarManager.Instance?.UpdateBar(this, GetHPRatio());
        Debug.Log("현재 몬스터 체력" + hp);
        if (hp <= 0 && state != State.Killed) ChangeState(State.Killed);
    }
    public float GetCurrentHP() => hp;             // 절대값
    public float GetHPRatio() => hp / data.maxHp;


    private void SensePlayer()
    {
        if (!player) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // Detect : 소리 + 내 hearRange
        if (state is not (State.Combat or State.Flee) &&
            dist <= data.hearRange + GetNoise())
        {
            detectedPos = player.position;
            ChangeState(State.Detect);
        }

        // Combat / Flee
        if (dist <= data.hearRange &&
            CanSeePlayer(player, data.sightDistance))
        {
            ChangeState(data.isaggressive ? State.Combat : State.Flee);
        }

        // 원거리 벗어남→Return
        if (state is not State.Return &&
            dist > data.maxSpawnerDist &&
            spawner)
        {
            ChangeState(State.Return);
        }
    }
    private void EnsurePlayer()
    {
        if (player) return;
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go) player = go.transform;
    }
    private float GetNoise()
    {
        var psr = player.GetComponent<PlayerSoundRange>();
        return psr ? psr.NoiseRadius : 0f;
    }
    private bool ReachedDestination()
        => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

    private IEnumerator WaitUntilArrivedOr(float timeout, System.Func<bool> breakCond)
    {
        float t = 0f;
        while (!ReachedDestination() && t < timeout && !breakCond())
        {
            t += Time.deltaTime;
            yield return null;
        }
    }

    /*
    //트리거 (소리·플레이어)
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
    */
    #endregion
}

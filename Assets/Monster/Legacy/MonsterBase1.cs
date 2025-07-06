using System.Collections;
using System.Collections.Generic;
using UnityEditor.Connect;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public abstract class MonsterBase1 : MonoBehaviour
{
    [SerializeField] protected MonsterData data;
    [SerializeField] protected Transform spawner;
    [SerializeField] protected LayerMask obstacleMask;

    protected GameObject player;
    protected NavMeshAgent agent;
    protected Animator anim;
    protected SpriteRenderer sr;
    protected float hp;
    protected Transform playertrans;
    private PlayerSoundRange psr;
    protected enum State { Idle, Detect, Combat, Flee, Return, Escaped, Killed }
    protected State state;
    Coroutine stateRoutine;
    protected Vector3 detectedPos;   // Detect 시 목표 지점
    protected bool isfastReturn = false; // 스포너로부터 멀어져서 일어나는 귀환이면 빠르게 귀환
    float senseTimer = 0f; // SensePlayer 간격 만들기용 변수

    protected virtual string IdleAnim => "Idle";
    protected virtual string WalkAnim => "Walk";
    protected virtual string RunAnim => "Run";
    protected virtual string AttackAnim => "Attack";
    protected virtual string DieAnim => "Die";

    #region Unity Lifecycle
    protected virtual void Awake()
    {
        psr = player ? player.GetComponent<PlayerSoundRange>() : null;
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            Debug.LogError("No Player!");
        playertrans = player.GetComponent<Transform>();
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        hp = data.maxHp;
        sr = GetComponent<SpriteRenderer>();
        SetStateColor(State.Idle);
        agent.stoppingDistance = data.stoppingDistance;
        agent.updateRotation = false;  
        agent.updateUpAxis = false;

        if (data.animator) anim.runtimeAnimatorController = data.animator;
    }
    protected virtual void Update()
    {
        if (state is State.Killed or State.Escaped) return;

        senseTimer += Time.deltaTime;
        if (senseTimer >= 1f)        // 1 초가 넘었을 때만
        {
            senseTimer -= 1f;        // 남은 초과분 보존
            SensePlayer();
        }
    }

    protected virtual void Start()
    {
        StartState(State.Idle);      // 처음 상태
        if (HealthBarManager.Instance)
            HealthBarManager.Instance.Register(this);
    }

    void StartState(State s)
    {
        if (stateRoutine != null) StopCoroutine(stateRoutine);
        state = s;
        stateRoutine = StartCoroutine(s.ToString());
    }
    bool SeePlayer(float maxDist) => player && CanSeePlayer(playertrans, maxDist);

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
        SetStateColor(s);  

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

    protected virtual IEnumerator Idle()
    {
        Play(IdleAnim);
        isfastReturn = false;
        while (state == State.Idle)
        {
            if (TooFarFromSpawner()) { ChangeState(State.Return); yield break; }
            agent.ResetPath();
            yield return BreakableWait(1f, State.Idle);
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
        const float NOISE_TIMEOUT = 2f;     // 최근 소리가 2 초 이상 업데이트 안되면 포기
        const float DEST_TIMEOUT = 2f;     // 같은 목표로 2 초 이상 이동하면 포기
        const float TARGET_EPS = 0.05f;  // 목적지 변경 허용 오차

        Play(WalkAnim);
        agent.speed = data.detectSpeed;

        float lostNoiseTimer = 0f;          // 들리지 않은 누적 시간
        float travelTimer = 0f;          // 현재 목적지까지 달린 시간

        Vector3 curTarget = detectedPos;
        agent.SetDestination(curTarget);
        if (TooFarFromSpawner()) { isfastReturn = true; ChangeState(State.Return); yield break; }
        while (state == State.Detect)
        {
            // 아직 ‘듣고’ 있는가?
            bool heardNow = CanHearPlayer(data.hearRange);

            // 보이면 즉시 Combat/Flee
            if (SeePlayer(data.sightDistance))
            {
                ChangeState(data.isaggressive ? State.Combat : State.Flee);
                yield break;
            }

            // 들리고 있다면 목표 좌표 계속 갱신
            if (heardNow)
            {
                lostNoiseTimer = 0f;                        // 리셋
                if (Vector2.Distance(curTarget, playertrans.position) > TARGET_EPS)
                {
                    curTarget = playertrans.position;   // 최신 위치
                    agent.SetDestination(curTarget);
                    travelTimer = 0f;                     // 새 목표이므로 리셋
                }
            }
            else
            {
                lostNoiseTimer += Time.deltaTime;
            }

            // 경로 계산 실패 시 즉시 포기
            if (!agent.pathPending &&
                agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                BreakToIdleOrReturn();   // 험지·막다른길 등
                yield break;
            }

            // 목적 도착·타임아웃·소음 사라짐 판정
            travelTimer += Time.deltaTime;
            if (ReachedDestination() ||
                travelTimer >= DEST_TIMEOUT ||
                lostNoiseTimer >= NOISE_TIMEOUT)
            {
                BreakToIdleOrReturn();
                yield break;
            }

            yield return null;
        }

        void BreakToIdleOrReturn()
        {
            bool nearSpawner = spawner &&
                               Vector2.Distance(transform.position, spawner.position)
                               <= data.nearSpawnerDist;
            ChangeState(nearSpawner ? State.Idle : State.Return);
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
            agent.SetDestination(playertrans.position);

            float dist = Vector2.Distance(transform.position, playertrans.position);
            bool see = CanSeePlayer(playertrans, data.lostDistance);

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

        RaycastHit2D hit = Physics2D.Raycast(start, dir, dist, ~0);
        return hit.collider && hit.collider.CompareTag("Player");
    }
    void SensePlayer()
    {
        if (!player) return;

        float dist = Vector2.Distance(transform.position, playertrans.position);
        bool heard = data.canHearNoise && CanHearPlayer(data.hearRange);
        bool seen = dist <= data.sightDistance &&
                     CanSeePlayer(playertrans, data.sightDistance);

        // Detect : ‘소리’ + 내 hearRange 
        if (heard)
        {
            detectedPos = playertrans.position;        // 가장 최신 위치 저장

            if (data.hearToCombat)          // “소리 = 전투” 타입
            {
                ChangeState(data.isaggressive ? State.Combat : State.Flee);
            }
            else
            {
                if (state is not (State.Detect or State.Combat or State.Flee))
                    ChangeState(State.Detect);          // Idle/Return → Detect
            }
        }
        //  Combat / Flee : 시야 탐지로 상태 진입
        if (seen)
        {
            ChangeState(data.isaggressive ? State.Combat : State.Flee);
        }

        // 먼 거리 벗어나면 Return
        if (state is not State.Return &&
            dist > data.maxSpawnerDist &&
            spawner)
        {
            ChangeState(State.Return);
        }
    }

    private float GetNoise()
    {
        return PlayerSoundRange.Instance ? PlayerSoundRange.Instance.NoiseRadius : 0f;
    }
    // 소리 감쇠 보정치 계산 
    int CountObstaclesBetween(Vector2 from, Vector2 to)
    {
        Vector2 dir = (to - from).normalized;
        float dist = Vector2.Distance(from, to);

        // obstacleMask에 포함되는 모든 충돌체를 조사
        RaycastHit2D[] hits = Physics2D.RaycastAll(from, dir, dist, obstacleMask);
        return hits.Length;          // 벽이 n개면 n 반환
    }

    // 소리를 들을 수 있는지?
    bool CanHearPlayer(float baseRange)
    {
        if (!player) return false;

        float dist = Vector2.Distance(transform.position, playertrans.position);

        // 벽 개수 × 감쇠치만큼 거리를 늘려 “실질 거리”로 환산
        int walls = CountObstaclesBetween(transform.position, playertrans.position);
        float effective = dist + walls * data.soundObstaclePenalty;

        return effective <= baseRange + GetNoise();
    }
    void SetStateColor(State s)
    {
        if (!sr) return;

        sr.color = s switch
        {
            State.Detect => Color.yellow,
            State.Combat => Color.red,
            _ => Color.white
        };
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
    protected bool TooFarFromSpawner()
    {
        if (!spawner) return false;
        return Vector2.Distance(transform.position, spawner.position) >= data.maxSpawnerDist;
    }
    #endregion
}

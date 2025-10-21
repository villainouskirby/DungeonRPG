using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData",
                 menuName = "Mobs/Monster Data",
                 order = 0)]
public class MonsterData : ScriptableObject
{
    [Header("공통 스탯")]
    public string monsterName = "Normal";
    public float maxHp = 100f;
    public float attackPower = 5f;   // 1타 데미지
    public float attackCooldown = 1.5f; // 타격 간격
    public float attackRange = 1.2f; // 실제 타격 거리

    [Header("이동 / 시야")]
    public bool canMove = true; // 터렛처럼 정지형이면 false
    public float hearRange = 3f;   // 청각 반경(고정)
    public float sightDistance = 6f;   // 시야 거리
    public float sightAngle = 90f;
    public float lostDistance = 8f;   // 시야 벗어나면 Combat 해제
    public float wanderRadius = 4f;   // Idle 산책 범위

    public float detectSpeed = 1.5f; // Detect 추적 속도
    public float combatSpeed = 3f;   // Combat 추적 속도
    public float fleeSpeed = 3.5f; // Flee 도주 속도
    public float fleeDashSpeed = 4.0f;
    public float stoppingDistance = 1f;   // NavMeshAgent.stoppingDistance

    [Header("소리 탐지 로직")]
    public float soundObstaclePenalty = 2f;   // 벽 1개당 거리 보정치(또는 dB 감쇠)

    [Header("몬스터 상태 전이 설정")]
    public float aggroHoldSeconds = 2f;        // 전투 진입까지 연속 유지 시간
    public float disengageHoldSeconds = 2f;    // 전투 이탈까지 연속 미유지 시간

    [Header("전투/도망/따라가기 전환 공통 지연")]
    [Tooltip("Combat/Flee/Trace 로 전환하기 전에 잠깐 대기")]
    public float preTransitionDelay = 0.3f;

    [Header("추적 모드(Trace)")]
    public bool istracing = false;
    [Tooltip("Trace 이동 속도 (비워두면 detectSpeed 사용)")]
    public float traceSpeed = 0f;

    [Tooltip("플레이어와 유지하고 싶은 목표 거리")]
    public float traceDesiredDistance = 4f;
    [Tooltip("이 거리보다 가까우면 살짝 벌어짐(정지 밴드 하한)")]
    public float traceNearDistance = 2.5f;
    [Tooltip("이 거리보다 멀면 다가감(정지 밴드 상한)")]
    public float traceFarDistance = 5.5f;

    [Header("상태 전이 지연(Inspector 조절)")]
    [Tooltip("플레이어 감지 후 Detect 상태로 들어가기 전에 요구되는 연속 유지 시간")]
    public float detectGateDelay = 0.25f;
    [Tooltip("플레이어 감지 후 Detect -> 플레이어 파악 상태로 들어가기 전에 요구되는 연속 유지 시간")]
    public float detecttoSeePlayerGateDelay = 0.5f;

    [Header("몬스터 피격 시 스턴 지속시간")]
    public float defaultHitStunSeconds = 0.3f;

    [Tooltip("스포너 최대 거리 초과 상태가 연속 유지되어 Return으로 돌아가기까지의 지연")]
    public float returnGateDelay = 0.75f;

    [Header("도망 로직, 이 거리 이상 몬스터가 플레이어랑 멀어지려 함")]
    public float fleeNearDistance = 15f;

    [Header("감지/플레이어 발각 상태 스프라이트")]
    public Sprite questionSprite;              // 감지 중(물음표)
    public Sprite exclamationSprite;           // 전투 진입(느낌표)
    public Color questionStartColor = Color.white;
    public Color questionEndColor = Color.red;


    [Header("스포너")]
    public float maxSpawnerDist = 15f;  // 이 거리 이상이면 Return
    public float nearSpawnerDist = 2f; // 이 거리만큼 스포너 근접 시 Idle 복귀

    [Header("적대적/비적대적 몬스터")]
    public bool isaggressive = true;
    [Header("지각 능력")]
    public bool canHearNoise = true;   // 청각 보유 여부
    public bool hearToCombat = false;  // 소리만 들어도 바로 전투
    public enum MonsterCategory { Hound, Turret, Miner, Beetle, Cleaner, Titan }
    [Header("행동 타입")]
    public MonsterCategory category = MonsterCategory.Hound;

    [Header("이 몬스터가 사용할 공격 패턴들 (순서대로 BT Selector)")]
    [Tooltip("이 이내면 공격 우선")]
    public float attackEnterDistance = 2.2f;
    [Tooltip("이 밖이면 이동 우선(히스테리시스)")]
    public float attackExitDistance = 3.0f;

    [Header("접근/ 이동 행동 리프 각각 구분해서 넣기")]
    public AttackBehaviourSO[] combatMoveBehaviours;   // 접근/오빗 등
    public AttackBehaviourSO[] combatAttackBehaviours; // 차지/근접공격 등
    public float[] moveWeights = { 60, 25, 15 }; // 이거 지우면 안됨

    [Tooltip("도망(또는 특수) 패턴들")]
    public FleeBehaviourSO[] fleeBehaviours;

    [Tooltip("특수 행동(채굴·충전·패링 등)")]
    public SpecialBehaviourSO[] specialBehaviours;

    [Header("관심 오브젝트")]
    [Tooltip("몬스터가 감지·추적할 태그들")]
    public string[] interestTags;
    //[Header("애니메이터")]
    public RuntimeAnimatorController animator;

}
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
    public float stoppingDistance = 1f;   // NavMeshAgent.stoppingDistance

    [Header("소리 탐지 로직")]
    public float soundObstaclePenalty = 2f;   // 벽 1개당 거리 보정치(또는 dB 감쇠)

    [Header("몬스터 상태 전이 설정")]
    public float aggroHoldSeconds = 2f;        // 전투 진입까지 연속 유지 시간
    public float disengageHoldSeconds = 2f;    // 전투 이탈까지 연속 미유지 시간

    [Header("상태 전이 지연(Inspector 조절)")]
    [Tooltip("플레이어 감지 후 Detect 상태로 들어가기 전에 요구되는 연속 유지 시간")]
    public float detectGateDelay = 0.25f;

    [Tooltip("스포너 최대 거리 초과 상태가 연속 유지되어 Return으로 돌아가기까지의 지연")]
    public float returnGateDelay = 0.75f;


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
    public enum MonsterCategory { Hound, Turret, Miner, Beetle, Cleaner }
    [Header("행동 타입")]
    public MonsterCategory category = MonsterCategory.Hound;

    [Tooltip("이 몬스터가 사용할 공격 패턴들 (순서대로 BT Selector)")]
    public AttackBehaviourSO[] attackBehaviours;

    [Tooltip("도망(또는 특수) 패턴들")]
    public FleeBehaviourSO[] fleeBehaviours;

    [Tooltip("특수 행동(채굴·충전·패링 등)")]
    public SpecialBehaviourSO[] specialBehaviours;

    [Header("관심 오브젝트")]
    [Tooltip("몬스터가 감지·추적할 태그들")]
    public string[] interestTags;
    //[Header("애니메이터")]
    public RuntimeAnimatorController animator;

    [Header("드랍 & EXP ")]
    public int expReward = 10;
    //public LootTable lootTable;
}
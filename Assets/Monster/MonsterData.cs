using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData",
                 menuName = "Mobs/Monster Data",
                 order = 0)]
public class MonsterData : ScriptableObject
{
    [Header("공통 스탯")]
    public string monsterName = "Normal";
    public float maxHp = 100f;
    public float attackPower = 2f;

    [Header("이동 / 시야")]
    public float detectSpeed = 1f;
    public float combatSpeed = 3f;
    public float FleeSpeed = 3f;
    public float stoppingDistance = 1f;
    public float sightDistance = 6f;
    public float lostDistance = 5f;

    [Header("스포너")]
    public float maxSpawnerDist = 15f;
    public float nearSpawnerDist = 2f;

    [Header("적대적/비적대적 몬스터")]
    public bool isagrresive = true;
    //[Header("애니메이터")]
    public RuntimeAnimatorController animator;
}
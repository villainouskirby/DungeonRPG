using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData",
                 menuName = "Mobs/Monster Data",
                 order = 0)]
public class MonsterData : ScriptableObject
{
    [Header("공통 스탯")]
    public string monsterName = "Normal";
    public float maxHp = 10f;
    public float attackPower = 2f;

    [Header("이동 / 시야")]
    public float detectSpeed = 1f;
    public float combatSpeed = 3f;
    public float stoppingDistance = 1f;
    public float lostDistance = 5f;

    [Header("스포너")]
    public float maxSpawnerDist = 15f;
    public float nearSpawnerDist = 2f;

    //[Header("애니메이터")]
    //public RuntimeAnimatorController animator;
}
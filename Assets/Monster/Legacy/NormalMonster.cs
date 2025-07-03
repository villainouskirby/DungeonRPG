using UnityEngine;

// 애니메이션 이름이 MonsterBase 기본값과 다르다면 재정의.
// 별도 로직(원거리 공격, 특수 패턴)이 필요하면 해당 코루틴만 override.
public class NormalMonster : MonsterBase1
{
    //애니메이션 이름 커스터마이즈
    protected override string IdleAnim => "IdleNormal";
    protected override string WalkAnim => "WalkFWD";
    protected override string RunAnim => "AttackRun";
    protected override string AttackAnim => "Attack01";
    protected override string DieAnim => "Die";
}
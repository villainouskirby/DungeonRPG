using System.Collections;
using UnityEngine;

public interface IMonsterState
{
    void Enter();      // 최초 1회
    void Tick();       // 매 프레임
    void Exit();       // 탈출 시 1회
}
public interface IMonsterBehaviour
{
    /// ① 지금 이 패턴을 실행할 수 있는가? (조건식)
    bool CanRun(MonsterContext ctx);          // 기본값: true

    /// ② 패턴 본체 – 반드시 yield 반환
    IEnumerator Execute(MonsterContext ctx);

    /// ③ 중도 취소 시 후처리(선택)
    void OnInterrupt(MonsterContext ctx);
}
public abstract class AttackBehaviourSO : ScriptableObject, IMonsterBehaviour
{
    public virtual bool CanRun(MonsterContext ctx) => true;
    public abstract IEnumerator Execute(MonsterContext ctx);
    public virtual void OnInterrupt(MonsterContext ctx) { }
}
public abstract class FleeBehaviourSO : ScriptableObject, IMonsterBehaviour
{
    public virtual bool CanRun(MonsterContext ctx) => true;
    public abstract IEnumerator Execute(MonsterContext ctx);
    public virtual void OnInterrupt(MonsterContext ctx) { }
}
public abstract class SpecialBehaviourSO : ScriptableObject, IMonsterBehaviour
{
    public virtual bool CanRun(MonsterContext ctx) => true;
    public abstract IEnumerator Execute(MonsterContext ctx);
    public virtual void OnInterrupt(MonsterContext ctx) { }
}

public sealed class MonsterStateMachine
{
    public IMonsterState Current { get; private set; }

    public void ChangeState(IMonsterState next)
    {
        Current?.Exit();
        Current = next;
        Current?.Enter();
    }

    public void Tick() => Current?.Tick();
}
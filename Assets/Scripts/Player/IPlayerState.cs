public interface IPlayerState
{
    void Enter();
    void Update();
    void Exit();
}

public interface IPlayerChangeState
{
    void ChangeState(IPlayerState newState);
}

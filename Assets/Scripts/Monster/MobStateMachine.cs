public class MobStateMachine
{
    private IMobState curState;
    private IMobState previousState;

    public void ChangeState(IMobState newState)
    {
        if (curState != null)
        {
            previousState = curState;
            curState.Exit();
        }


        curState = newState;
        curState.Enter();
    }
    public void RestorePreviousState()
    {
        if (previousState != null)
        {
            ChangeState(previousState);
        }
    }
    public IMobState GetCurrentState() => curState;
    public IMobState GetPreviousState() => previousState;

    public void Update()
    {
        if (curState != null)
            curState.Update();
    }
}
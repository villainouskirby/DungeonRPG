using Events;
using System;
using EventArgs = Events.EventArgs;

[System.Serializable]
public abstract class Mission
{
    public Action OnMissionClearChanged;

    public string ID;
    public int MaxProgress;
    public int Progress;
    public bool IsMissionCleared = false;

    public abstract void Init(string questID);
    public abstract string GetExplanation();

    /// <summary> 진행상황 이벤트에 등록 </summary>
    public abstract void RegisterProcess();

    /// <summary> 진행상황 이벤트에 등록해제 </summary>
    public abstract void UnRegisterProcess();

    /// <summary> 진행상황 업데이트 </summary>
    public abstract void UpdateProgress(EventArgs eventArgs);
    public virtual bool CheckIsMissionCleared()
    {
        bool isClear = Progress >= MaxProgress;

        if (isClear != IsMissionCleared)
        {
            IsMissionCleared = isClear;
            OnMissionClearChanged?.Invoke();
        }

        return isClear;
    }
}

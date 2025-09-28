using Events;

[System.Serializable]
public abstract class Mission
{
    public string ID;
    public int MaxProgress;
    public int Progress;

    public bool IsMissionCleared => Progress >= MaxProgress;

    public abstract void Init(string questID);

    /// <summary> 진행상황 이벤트에 등록 </summary>
    public abstract void RegisterProcess();

    /// <summary> 진행상황 이벤트에 등록해제 </summary>
    public abstract void UnRegisterProcess();

    /// <summary> 진행상황 업데이트 </summary>
    public abstract void UpdateProgress(EventArgs eventArgs);
}

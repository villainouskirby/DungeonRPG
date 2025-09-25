using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    public bool ActiveSelf => gameObject.activeSelf;
    
    protected bool _isActvieOnStart = false;

    protected virtual void Awake()
    {
        InitBase();

        gameObject.SetActive(_isActvieOnStart);
    }

    /// <summary> UIName 초기화 </summary>
    protected abstract void InitBase();

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
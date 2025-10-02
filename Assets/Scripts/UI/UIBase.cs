using System;
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

    protected virtual void OnDisable()
    {
        UIPopUpHandler.Instance.CloseUI(this);
    }

    /// <summary> UIName 초기화 </summary>
    protected virtual void InitBase() { }
    
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CheckChange : MonoBehaviour
{
    private SrInfo _srInfo;
    private Action<SrInfo> _action;

    public void Init(SrInfo srInfo, Action<SrInfo> action)
    {
        _srInfo = srInfo;
        _action = action;
    }

    private void Update()
    {
        if (_srInfo == null)
            return;

        _action.Invoke(_srInfo);
    }
}
#endif
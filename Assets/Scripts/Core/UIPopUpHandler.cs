using System;
using System.Collections.Generic;
using UnityEngine;
using Core;

public class UIPopUpHandler
    : Singleton<UIPopUpHandler>, IManager
{
    private Dictionary<Type, UIBase> _uiDict = new(); // 등록된 UI들 inspector에서 참조말고 여기서 불러오는 식으로 다 바꿔야 할듯
    private UIBase _openUI;

    private void Update()
    {
        if (Input.GetKey(KeyCode.Tab))
        {
            OpenUI<Inventory>();
        }

        if (Input.GetKey(KeyCode.Alpha1))
        {
            OpenUI<Quest>();
        }

        if (Input.GetKey(KeyCode.Alpha2))
        {
            // OpenUI<Map>();
        }

        if (Input.GetKey(KeyCode.Alpha3))
        {
            // OpenUI<Document>();
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            CloseUI();
        }
    }

    public void RegisterUI<T>(T ui) where T : UIBase
    {
        _uiDict[typeof(T)] = ui;
    }

    public void UnRegisterUI<T>(T ui) where T : UIBase
    {
        _uiDict.Remove(typeof(T));
    }
    
    /// <returns> 해당 UI 클래스 </returns>
    public T GetScript<T>() where T : UIBase
    {
        if (!_uiDict.TryGetValue(typeof(T), out var ui)) return null;
        if (ui == null)
        {
            _uiDict.Remove(typeof(T));
            return null;
        }

        return ui as T;
    }

    /// <summary>
    /// UI 활성화
    /// <para/> 한번에 하나만 띄우게 함
    /// </summary>
    /// <returns> 해당 UI 클래스 </returns>
    public T OpenUI<T>() where T : UIBase
    {
        UIBase ui;

        if (_openUI != null && _openUI.ActiveSelf) return null;
        if ((ui = GetScript<T>()) == null) return null;

        ui.SetActive(true);
        _openUI = ui;

        return ui as T;
    }

    public void CloseUI()
    {
        if (_openUI == null) return;

        _openUI.SetActive(false);
        _openUI = null;
    }

    public void Initialize()
    {
    }
}

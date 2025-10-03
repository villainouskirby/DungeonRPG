using System;
using System.Collections.Generic;
using UnityEngine;
using Core;

public class UIPopUpHandler : Singleton<UIPopUpHandler>, IManager
{
    public bool IsUIOpen => _openUIs.Count > 0;

    private Dictionary<Type, UIBase> _uiDict = new(); // 등록된 UI들 inspector에서 참조말고 여기서 불러오는 식으로 다 바꿔야 할듯
    private List<UIBase> _openUIs = new();

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleUI<Inventory>();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleUI<Quest>();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // OpenUI<Map>();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // OpenUI<Document>();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseLastUI();
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
    /// UI 토글
    /// <para/> 한번에 하나만 띄우게 함
    /// </summary>
    /// <returns> 해당 UI 클래스 </returns>
    public T ToggleUI<T>() where T : UIBase
    {
        foreach (var ui in _openUIs) // 본인이 열려 있으면 닫기
        {
            if (ui.GetType() == typeof(T))
            {
                CloseUI(ui);
                return ui as T;
            }
        }

        if (_openUIs.Count > 0) return null; // 본인이 열려 있지 않으나 다른 UI 켜져 있으면 miss

        return OpenUI<T>();
    }

    /// <summary>
    /// UI 열기
    /// </summary>
    /// <returns> 해당 UI 클래스 </returns>
    public T OpenUI<T>() where T : UIBase
    {
        UIBase ui;

        if ((ui = GetScript<T>()) == null || ui.ActiveSelf) return null;

        ui.SetActive(true);
        _openUIs.Add(ui);

        return ui as T;
    }

    public T CloseAllAndOpenUI<T>() where T : UIBase
    {
        while (_openUIs.Count > 0)
        {
            CloseLastUI();
        }

        return OpenUI<T>();
    }

    public void CloseUI<T>(T ui) where T : UIBase
    {
        ui.SetActive(false);
        _openUIs.Remove(ui);
    }

    public void CloseLastUI()
    {
        if (_openUIs.Count == 0) return;

        int idx = _openUIs.Count - 1;
        _openUIs[idx].SetActive(false);
    }

    public void Initialize()
    {
        GetScript<Inventory>().GiveInitEquipment();
    }
}

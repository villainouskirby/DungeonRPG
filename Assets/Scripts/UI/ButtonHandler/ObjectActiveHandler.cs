using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Button))]
public class ObjectActiveHandler : MonoBehaviour
{
    [SerializeField] private GameObject _object;

    public Action<ObjectActiveHandler> OnClicked;
    public Action<ObjectActiveHandler> OnActive;
    public Action<ObjectActiveHandler> OnInactive;

    public void SetActive(bool active)
    {
        if (_object == null) return;

        _object.SetActive(active);
        InvokeEvents(active);
    }

    public void ToggleActive()
    {
        if (_object == null) return;

        _object.SetActive(!_object.activeSelf);
        InvokeEvents(_object.activeSelf);
    }

    private void InvokeEvents(bool active)
    {
        OnClicked?.Invoke(this);

        if (active)
        {
            OnActive?.Invoke(this);
        }
        else
        {
            OnInactive?.Invoke(this);
        }
    }

    public void OpenPanelForce()
    {
        if (_object == null) return;

        _object.SetActive(true);
    }

    public void ClosePanelForce()
    {
        if (_object == null) return;

        _object.SetActive(false);
    }
}

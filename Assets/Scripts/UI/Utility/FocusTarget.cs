using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class FocusTarget : MonoBehaviour, IPointerClickHandler
{
    public Action OnClicked;
    public string ID => _id;

    [SerializeField] private string _id;
    
    private bool _isRegistered = false;

    void Start()
    {
        Register();
    }

    public void Register()
    {
        if (_isRegistered) return;

        if (!TryGetComponent(out RectTransform rect))
        {
            Debug.LogError("UI 아님");
            return;
        }

        if (string.IsNullOrEmpty(_id))
        {
            _id = gameObject.name;
        }

        UIPopUpHandler.Instance.GetScript<UIFocus>().Register(_id, rect);

        _isRegistered = true;
    }

    private void OnDestroy()
    {
        UIPopUpHandler.Instance?.GetScript<UIFocus>()?.Unregister(_id);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClicked?.Invoke();
        OnClicked = null;
    }
}

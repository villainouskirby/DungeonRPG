using System;
using Unity.VisualScripting;
using UnityEngine;

public class HandleableButtons : MonoBehaviour
{
    [Header("기본 인덱스 값")]
    [SerializeField] private int _idx = 0;

    private ButtonSpriteHandler[] _handlers;
    private float _prevAxis = 0;

    private void Awake()
    {
        int cnt = transform.childCount;
        _handlers = new ButtonSpriteHandler[cnt];

        for (int i = 0; i < cnt; i++)
        {
            if (!transform.GetChild(i).TryGetComponent<ButtonSpriteHandler>(out var handler))
            {
                handler.AddComponent<ButtonSpriteHandler>();
                handler.Init();
            }

            _handlers[i] = handler;
        }

        if (_idx < 0 || _idx >= _handlers.Length)
        {
            _idx = 0;
        }
    }

    private void Start()
    {
        for (int i = 0; i < _handlers.Length; i++)
        {
            _handlers[i].SetNormalSprite();
        }

        _handlers[_idx].SetEnterSprite();
    }

    private void OnEnable()
    {
        foreach (var handler in _handlers)
        {
            handler.OnEnter += UpdateButtons;
        }
    }

    private void OnDisable()
    {
        foreach (var handler in _handlers)
        {
            handler.OnEnter -= UpdateButtons;
        }
    }

    // 기본적으로 위에서 아래로 카운팅
    private void Update()
    {
        float axis = Input.GetAxisRaw("Vertical");

        if (axis == _prevAxis) return;

        if (axis != 0)
        {
            _handlers[_idx].SetNormalSprite();

            if (axis < 0)
            {
                _idx = (_idx + 1) % _handlers.Length;
            }
            else if (axis > 0)
            {
                _idx = (_idx - 1 + _handlers.Length) % _handlers.Length;
            }

            _handlers[_idx].SetEnterSprite();
        }

        _prevAxis = axis;
    }

    private void UpdateButtons(ButtonSpriteHandler handler)
    {
        int idx = Array.IndexOf(_handlers, handler);

        if (idx == _idx) return;

        _handlers[_idx].SetNormalSprite();
        _idx = idx;
    }
}

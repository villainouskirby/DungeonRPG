using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemSlotUI : ItemSlotUI
{
    [SerializeField] private Image _slotBackground;
    [SerializeField] private TextMeshProUGUI _tierText;

    [Header("Slider")]
    [SerializeField] private RectTransform _viewport;
    [SerializeField] private RectTransform _detailArea;

    [SerializeField] private Sprite _detailOffSprite;
    [SerializeField] private Sprite _detailOnSprite;

    [SerializeField] private float _slideTime = 0.1f;

    private bool _isDetailOn = false;

    private float _viewportWidth;
    private float _viewportHeight;

    private CancellationTokenSource cts;

    private void Start()
    {
        _viewportWidth = _detailArea.sizeDelta.x;
    }

    public void SetItemDetail(string tier)
    {
        _tierText.text = tier;
    }

    [ContextMenu("Slide")]
    public void ToggleDetail()
    {
        _viewportHeight = _detailArea.sizeDelta.y + 4; // 나중에 init으로 빼고 instantiate 한 후에 호출하는 식으로 하면 해결 되려나 // start에 하니 vertical layout 연산 잔에 이루어지는 듯 해 height가 0으로 표기됨

        cts?.Cancel();
        cts?.Dispose();

        if (!_isDetailOn)
        {
            _slotBackground.sprite = _detailOnSprite;
        }

        SlideDetail(_isDetailOn).Forget();
        _isDetailOn = !_isDetailOn;
    }

    private async UniTaskVoid SlideDetail(bool isOn)
    {
        cts = new CancellationTokenSource();

        float startTime = Time.time;

        float startHeight = _viewport.sizeDelta.y;
        float targetHeight;

        if (isOn)
        {
            targetHeight = 0;
        }
        else
        {
            targetHeight = _viewportHeight;
        }

        float t;
        try
        {
            while ((t = (Time.time - startTime) / _slideTime) < 1)
            {
                _viewport.sizeDelta = new Vector2(_viewportWidth, Mathf.Lerp(startHeight, targetHeight, t));

                await UniTask.NextFrame(cts.Token);
            }

            _viewport.sizeDelta = new Vector2(_viewportWidth, targetHeight);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (isOn)
        {
            _slotBackground.sprite = _detailOffSprite;
        }
    }
}

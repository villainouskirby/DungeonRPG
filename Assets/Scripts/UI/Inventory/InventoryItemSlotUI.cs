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

    [SerializeField] private float _slideSpeed = 10;
    [SerializeField] private float _slideInterval = 0.1f;

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
        cts?.Cancel();
        cts?.Dispose();

        if (!_isDetailOn)
        {
            _slotBackground.sprite = _detailOnSprite;
        }

        _viewportHeight = _detailArea.sizeDelta.y;
        SlideDetail(_isDetailOn).Forget();
        _isDetailOn = !_isDetailOn;
    }

    private async UniTaskVoid SlideDetail(bool isOn)
    {
        cts = new CancellationTokenSource();

        float speed;
        float targetHeight;

        if (isOn)
        {
            speed = -_slideSpeed;
            targetHeight = 0;
        }
        else
        {
            speed = _slideSpeed;
            targetHeight = _viewportHeight;
        }

        try
        {
            while (true)
            {
                _viewport.sizeDelta = _viewport.sizeDelta + Vector2.up * speed;

                float height = _viewport.sizeDelta.y;
                if (height < 0 || height > _viewportHeight) break;

                await UniTask.WaitForSeconds(_slideInterval ,cancellationToken: cts.Token);
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

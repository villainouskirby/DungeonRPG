using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemGetPopUpSlotUI : SlotUI
{
    public delegate void PopUpClose();
    public event PopUpClose OnPopUpClose;

    [Tooltip("아이템 이름")]
    [SerializeField] private TextMeshProUGUI _nameText;

    [Tooltip("아이템 개수")]
    [SerializeField] private TextMeshProUGUI _amountText;

    private RectTransform _rect;
    private Image _image;

    private bool _isOpen = false;


    private void OnDisable()
    {
        OnPopUpClose = null;
    }

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
    }

    public void SetItemInfo(Sprite sprite, string name, int amount)
    {
        SetItemInfo(sprite);
        _nameText.text = name;
        _amountText.text = "X" + amount.ToString();
    }

    public async UniTask PopUpSlot(Vector2 destPos, float targetTime, CancellationToken token)
    {
        Vector2 startPos = destPos + Vector2.right * _rect.sizeDelta.x;
        _rect.position = startPos;

        Color color = _image.color;
        color.a = 1;
        _image.color = color;
        gameObject.SetActive(true);
        _isOpen = true;

        float startTime = Time.time;
        float dTime;

        while ((dTime = (Time.time - startTime) / targetTime) < 1)
        {
            _rect.position = Vector2.Lerp(startPos, destPos, dTime);

            await UniTask.NextFrame(cancellationToken: token);
        }

        _rect.position = destPos;

        WaitBeforeClose(token).Forget();
    }

    private async UniTask WaitBeforeClose(CancellationToken token)
    {
        float startTime = Time.time;

        while (Time.time - startTime < 10)
        {
            if (!_isOpen) return;

            await UniTask.NextFrame(cancellationToken: token);
        }

        ClosePopUp(token).Forget();
    }

    public async UniTask ClosePopUp(CancellationToken token)
    {
        _isOpen = false;

        Vector2 startPos = _rect.position;
        Vector2 destPos = startPos + Vector2.up * _rect.sizeDelta.y;

        float startTime = Time.time;
        float dTime;

        while ((dTime = (Time.time - startTime) / 0.1f) < 1)
        {
            _rect.position = Vector2.Lerp(startPos, destPos, dTime);
            Color color = _image.color;
            color.a = 1 - dTime;
            _image.color = color;

            await UniTask.NextFrame(cancellationToken: token);
        }

        OnPopUpClose?.Invoke();
        gameObject.SetActive(false);
    }
}

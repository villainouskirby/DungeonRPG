using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemSlotUI : ItemSlotUI
{
    [SerializeField] private Image _slotBackground;
    [SerializeField] private GameObject _equippedImage;
    [SerializeField] private GameObject[] _detailAbilityObjects;
    [SerializeField] private Image[] _detailImages;
    [SerializeField] private TextMeshProUGUI[] _detailAbilityTexts;
    [SerializeField] private TextMeshProUGUI _tierText;
    [SerializeField] private TextMeshProUGUI _explanationText;

    [Header("Detail Sprites")]
    [SerializeField] private Sprite _colectIcon;
    [SerializeField] private Sprite _hpIcon;
    [SerializeField] private Sprite _powerIcon;
    [SerializeField] private Sprite _speedIcon;
    [SerializeField] private Sprite _staminaIcon;

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

    private CancellationTokenSource _cts;

    private void Start()
    {
        _viewportWidth = _detailArea.sizeDelta.x;
    }

    public void SetEquipped(bool isEquipped)
    {
        _equippedImage.SetActive(isEquipped);
    }

    public void SetItemDetail(ItemData data)
    {
        _tierText.text = data.Info.rank.ToString();
        _explanationText.text = data.Info.Explanation;

        int activatedDetailCnt = 0;

        switch (data)
        {
            case WeaponItemData:
                _detailImages[0].sprite = _powerIcon;
                _detailAbilityTexts[0].text = (data as WeaponItemData).WeaponInfo.atk.ToString();
                activatedDetailCnt = 1;

                break;

            case SubWeaponItemData:
                _detailImages[0].sprite = _colectIcon;
                _detailAbilityTexts[0].text = (data as SubWeaponItemData).SubWeaponInfo.count.ToString();
                activatedDetailCnt = 1;

                break;

            case ArmorItemData:
                Item_Info_Armor armorInfo = (data as ArmorItemData).ArmorInfo;

                _detailImages[0].sprite = _hpIcon;
                _detailImages[1].sprite = _staminaIcon;
                _detailImages[2].sprite = _speedIcon;

                _detailAbilityTexts[0].text = armorInfo.hp.ToString();
                _detailAbilityTexts[1].text = armorInfo.stamina.ToString();
                _detailAbilityTexts[2].text = armorInfo.speed.ToString();
                activatedDetailCnt = 2;

                break;

            case BackpackItemData:
                Item_Info_Backpack backpackInfo = (data as BackpackItemData).BackpackInfo;

                _detailImages[0].sprite = _speedIcon;
                _detailAbilityTexts[0].text = backpackInfo.speed.ToString();

                break;

        }


        for (int i = activatedDetailCnt; i < _detailAbilityObjects.Length; i++)
        {
            _detailAbilityObjects[i].SetActive(false);
        }
    }

    [ContextMenu("Slide")]
    public void ToggleDetail()
    {
        _cts?.Cancel();
        _cts?.Dispose();

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
        _cts = new CancellationTokenSource();

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

                await UniTask.WaitForSeconds(_slideInterval ,cancellationToken: _cts.Token);
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

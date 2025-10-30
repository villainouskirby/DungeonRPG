using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tutorial
{
    public class InteractUI : UIBase
    {
        public enum InteractType
        {
            Click,
            F
        }

        [SerializeField] private float _yOffset = 30;
        [SerializeField] private Sprite _clickIcon;
        [SerializeField] private Sprite _fIcon;

        private Image _image;
        private RectTransform _rect;
        private Dictionary<InteractType, Sprite> _spritesDict = new();

        private int _subscribeNum = 0;
        private SpriteRenderer _targetRenderer;
        private Transform _targetTransform;

        protected override void InitBase()
        {
            _image = GetComponent<Image>();
            _rect = GetComponent<RectTransform>();
            UIPopUpHandler.Instance.RegisterUI(this);

            _spritesDict[InteractType.Click] = _clickIcon;
            _spritesDict[InteractType.F] = _fIcon;

            gameObject.SetActive(false);
        }

        public void OpenInteractPopUp(InteractType type, Transform transform)
        {
            OpenInteractPopUp(type, transform, _yOffset);
        }

        public void OpenInteractPopUp(InteractType type, Transform transform, float yOffset)
        {
            _targetRenderer = transform.GetComponent<SpriteRenderer>();
            _targetTransform = transform;

            if (_subscribeNum++ == 0)
            {
                UpdatePopUp(type, yOffset).Forget();
                gameObject.SetActive(true);
            }
        }

        public async UniTaskVoid UpdatePopUp(InteractType type, float yOffset)
        {
            Sprite sprite = _spritesDict[type];
            _image.sprite = sprite;
            _rect.sizeDelta = sprite.rect.size;

            while (_subscribeNum > 0)
            {
                Vector2 pos = new Vector2(_targetTransform.position.x, _targetRenderer.bounds.max.y);
                pos = RectTransformUtility.WorldToScreenPoint(Camera.main, pos);
                pos += Vector2.up * yOffset;

                _rect.anchoredPosition = pos;

                await UniTask.NextFrame();
            }
        }

        public void CloseInteractPopUp()
        {
            if (--_subscribeNum <= 0)
            {
                _subscribeNum = 0;
                gameObject.SetActive(false);
            }
        }
    }
}
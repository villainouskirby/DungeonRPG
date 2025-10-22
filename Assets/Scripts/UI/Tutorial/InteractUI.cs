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
            SpriteRenderer spriteRenderer = transform.GetComponent<SpriteRenderer>();
            Vector2 pos = new Vector2(transform.position.x, spriteRenderer.bounds.max.y);
            pos = RectTransformUtility.WorldToScreenPoint(Camera.main, pos);

            OpenInteractPopUp(type, pos);
        }

        public void OpenInteractPopUp(InteractType type, Vector2 pos)
        {
            pos += Vector2.up * _yOffset;

            Sprite sprite = _spritesDict[type];
            _image.sprite = sprite;
            _rect.sizeDelta = sprite.rect.size;
            _rect.anchoredPosition = pos;

            gameObject.SetActive(true);
        }

        public void CloseInteractPopUp()
        {
            gameObject.SetActive(false);
        }
    }
}
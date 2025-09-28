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

        [SerializeField] private Sprite _clickIcon;
        [SerializeField] private Sprite _fIcon;

        private Image _image;
        private RectTransform _rect;
        private Dictionary<InteractType, Sprite> _spritesDict = new();

        protected override void InitBase()
        {
            _image = GetComponent<Image>();
            UIPopUpHandler.Instance.RegisterUI(this);

            _spritesDict[InteractType.Click] = _clickIcon;
            _spritesDict[InteractType.F] = _fIcon;

            gameObject.SetActive(false);
        }

        public void OpenInteractPopUp(InteractType type)
        {
            Sprite sprite = _spritesDict[type];
            _image.sprite = sprite;
            _rect.sizeDelta = sprite.rect.size * 100;

            gameObject.SetActive(true);
        }

        public void CloseInteractPopUp()
        {
            gameObject.SetActive(false);
        }
    }
}
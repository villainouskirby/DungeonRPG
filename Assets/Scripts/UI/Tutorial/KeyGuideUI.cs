using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tutorial
{
    public class KeyGuideUI : UIBase
    {
        public enum GuideType
        {
            Wasd,
            Click,
            Dodge,
            Crouch,
            Sprint,
            Bag,
            Quest,
            Map,
            Tab
        }

        [SerializeField] private Sprite _wasdSprite;
        [SerializeField] private Sprite _clickSprite;
        [SerializeField] private Sprite _dodgeSprite;
        [SerializeField] private Sprite _crouchSprite;
        [SerializeField] private Sprite _sprintSprite;
        [SerializeField] private Sprite _bagSprite;
        [SerializeField] private Sprite _questSprite;
        [SerializeField] private Sprite _mapSprite;
        [SerializeField] private Sprite _tabSprite;

        public GuideType CurrentType { get; private set; }

        private bool _isTutorialFinished = false;

        private Image _tutorialImage;
        private Dictionary<GuideType, Sprite> _spritesDict = new();

        protected override void InitBase()
        {
            if (_isTutorialFinished) // ㅏ 이거 어케 하지 튜토리얼 같은거 기획 더 나오고 해야할거같은데
            {
                Destroy(gameObject);
                return;
            }

            _tutorialImage = GetComponent<Image>();
            UIPopUpHandler.Instance.RegisterUI(this);

            _spritesDict[GuideType.Wasd] = _wasdSprite;
            _spritesDict[GuideType.Click] = _clickSprite;
            _spritesDict[GuideType.Dodge] = _dodgeSprite;
            _spritesDict[GuideType.Crouch] = _crouchSprite;
            _spritesDict[GuideType.Sprint] = _sprintSprite;
            _spritesDict[GuideType.Bag] = _bagSprite;
            _spritesDict[GuideType.Quest] = _questSprite;
            _spritesDict[GuideType.Map] = _mapSprite;
            _spritesDict[GuideType.Tab] = _tabSprite;

            gameObject.SetActive(false);
        }

        public void OpenTutorial(string typeName)
        {
            if (Enum.TryParse(typeName, out GuideType type))
            {
                OpenTutorial(type);
            }
        }

        public void OpenTutorial(GuideType type)
        {
            CurrentType = type;
            _tutorialImage.sprite = _spritesDict[type];
            gameObject.SetActive(true);
        }

        public void CloseTutorial()
        {
            gameObject.SetActive(false);
        }
    }
}

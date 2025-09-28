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
        }

        [SerializeField] private Sprite _wasdSprite;
        [SerializeField] private Sprite _clickSprite;
        [SerializeField] private Sprite _dodgeSprite;
        [SerializeField] private Sprite _crouchSprite;
        [SerializeField] private Sprite _sprintSprite;

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

            gameObject.SetActive(false);
        }

        public void OpenTutorial(GuideType type)
        {
            _tutorialImage.sprite = _spritesDict[type];
            gameObject.SetActive(true);
        }

        public void CloseTutorial(GuideType type)
        {
            gameObject.SetActive(false);
        }
    }
}

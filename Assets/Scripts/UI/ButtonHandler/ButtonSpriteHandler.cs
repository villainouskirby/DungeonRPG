using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ButtonSpriteHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private bool _isResetOnRealease = true;
    [SerializeField] private bool _isSetSizeToSprite = true;
    [SerializeField] private ButtonSpriteHandler _innerSpriteHandler;

    [Header("Sprites")]
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Sprite _pressedSprite;
    [SerializeField] private Sprite _disabledSprite;

    private Image _image;
    private RectTransform _rect;
    private bool _isFixed = false;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _image.sprite = _defaultSprite;

        _rect = GetComponent<RectTransform>();

        Init();
    }

    public void Init()
    {
        Sprite defaultSprite = GetComponent<Sprite>();

        if (_defaultSprite == null)
        {
            _defaultSprite = defaultSprite;
        }

        if (_pressedSprite == null)
        {
            _pressedSprite = defaultSprite;
        }

        if (_disabledSprite == null)
        {
            _disabledSprite = defaultSprite;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isFixed) return;

        SetPressedSprite();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_isFixed) return;

        if (_isResetOnRealease ||
            eventData.pointerCurrentRaycast.gameObject != gameObject)
        {
            SetNormalSprite();
        }
        else
        {
            _isFixed = true;
        }
    }

    public void SetNormalSprite()
    {
        _isFixed = false;

        if (_defaultSprite == null) return;

        if (_image == null)
        {
            _image = GetComponent<Image>();
        }

        _image.sprite = _defaultSprite;
        ChangeSize(_defaultSprite);

        _innerSpriteHandler?.SetNormalSprite();
    }

    public void SetPressedSprite()
    {
        if (_pressedSprite == null) return;

        if (_image == null)
        {
            _image = GetComponent<Image>();
        }

        _image.sprite = _pressedSprite;
        ChangeSize(_pressedSprite);

        _innerSpriteHandler?.SetPressedSprite();
    }

    public void SetDisabledSprite()
    {
        if (_disabledSprite == null) return;

        if (_image == null)
        {
            _image = GetComponent<Image>();
        }

        _image.sprite = _disabledSprite;
        ChangeSize(_disabledSprite);

        _innerSpriteHandler?.SetDisabledSprite();
    }

    private void ChangeSize(Sprite sprite)
    {
        if (!_isSetSizeToSprite) return;

        _rect.sizeDelta = sprite.bounds.size * 100;
    }

    #region 이벤트 가로챔 방지

    public void OnBeginDrag(PointerEventData eventData) => eventData.Use();
    public void OnDrag(PointerEventData eventData) => eventData.Use();
    public void OnEndDrag(PointerEventData eventData) => eventData.Use();

    #endregion
}
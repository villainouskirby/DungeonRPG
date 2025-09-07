using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ButtonSpriteHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private bool _isResetOnRealease = true;

    [Header("Sprites")]
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Sprite _pressedSprite;
    [SerializeField] private Sprite _disabledSprite;

    private Image _image;
    private bool _isFixed = false;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _image.sprite = _defaultSprite;

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

        if (_isResetOnRealease
            || eventData.pointerCurrentRaycast.gameObject != gameObject)
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
    }

    public void SetPressedSprite()
    {
        if (_pressedSprite == null) return;

        if (_image == null)
        {
            _image = GetComponent<Image>();
        }

        _image.sprite = _pressedSprite;
    }

    public void SetDisabledSprite()
    {
        if (_disabledSprite == null) return;

        if (_image == null)
        {
            _image = GetComponent<Image>();
        }

        _image.sprite = _disabledSprite;
    }
}
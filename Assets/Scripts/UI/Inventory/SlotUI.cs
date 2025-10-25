using UnityEngine;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
    [Tooltip("아이템 이미지")]
    [SerializeField] protected Image _image;

    private void Awake()
    {
        if (_image == null) _image = GetComponent<Image>();
        if (_image.sprite == null)
        {
            ChangeAlpha(0);
        }
    }

    public void SetItemInfo(Sprite imageSprite)
    {
        _image.sprite = imageSprite;

        if (imageSprite == null)
        {
            ChangeAlpha(0);
        }
        else
        {
            ChangeAlpha(1);
        }
    }

    public void ChangeAlpha(float a)
    {
        Color color = _image.color;
        color.a = a;
        _image.color = color;
    }
}

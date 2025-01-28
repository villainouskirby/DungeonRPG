using UnityEngine;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
    [Tooltip("아이템 이미지")]
    [SerializeField] private Image _image;


    public void SetItemInfo(Sprite imageSprite)
    {
        _image.sprite = imageSprite;
    }
}

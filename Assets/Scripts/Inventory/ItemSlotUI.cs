using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [Tooltip("아이템 이미지")]
    [SerializeField] private Image _image;

    [Tooltip("아이템 이름")]
    [SerializeField] private TextMeshProUGUI _nameText;

    [Tooltip("아이템 개수")]
    [SerializeField] private TextMeshProUGUI _amountText;

    [Tooltip("아이템 개당 무게")]
    [SerializeField] private TextMeshProUGUI _weightText;

    [Tooltip("아이템 총 무게")]
    [SerializeField] private TextMeshProUGUI _weightSumText;

    private int _itemWeight;
    

    public void SetItemInfo(Sprite imageSprite, string name, int Weight)
    {
        if (imageSprite != null)
        {
            _image.sprite = imageSprite;
            _nameText.text = name;
            _itemWeight = Weight;
            _weightText.text = Weight.ToString();
            gameObject.SetActive(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetItemAmount(int amount)
    {
        _amountText.text = "X" + amount.ToString();
        _weightSumText.text = (amount * _itemWeight).ToString();
    }
}

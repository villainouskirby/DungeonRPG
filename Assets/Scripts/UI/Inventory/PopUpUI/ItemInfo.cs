using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private Image icon;

    public void SetInfo(ItemData data)
    {
        nameText.text = data.Name;
        //tooltipText.text = data.Tooltip;
        icon.sprite = data.IconSprite;
    }
}

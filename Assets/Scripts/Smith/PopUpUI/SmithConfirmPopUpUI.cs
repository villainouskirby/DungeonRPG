using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmithConfirmPopUpUI : MonoBehaviour
{
    [SerializeField] private Image _itemImage;
    [SerializeField] private TextMeshProUGUI _itemText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    [Header("Smith PopUpUI")]
    [SerializeField] private SmithPopUpUI _smithPopUpUI;

    [Header("Equip Check PopUpUI")]
    [SerializeField] private GameObject _equipCheckPopUpUI;

    private void Awake()
    {
        _confirmButton.onClick.AddListener(ConfirmCraft);
        _cancelButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    private void ConfirmCraft()
    {
        _smithPopUpUI.AddCraftedItemToInventory();
        _equipCheckPopUpUI.SetActive(true);
    }

    public void OpenConfirmPopUpUI(Sprite sprite, string name)
    {
        _itemImage.sprite = sprite;
        _itemText.text = name;
        gameObject.SetActive(true);
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellSelectedItemPopUpUI : MonoBehaviour
{
    [SerializeField] private ShopUI _shopUI;

    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _confirmButton;

    public void SetInfo(int cost)
    {
        _goldText.text = cost.ToString();
        gameObject.SetActive(true);
    }

    private void Awake()
    {
        _cancelButton.onClick.AddListener(() => gameObject.SetActive(false));
        _confirmButton.onClick.AddListener(_shopUI.SellSelectedSlot);
        _confirmButton.onClick.AddListener(() => gameObject.SetActive(false));
    }
}

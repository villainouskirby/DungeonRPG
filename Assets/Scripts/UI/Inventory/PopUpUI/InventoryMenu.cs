using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private InventoryUI _inventoryUI;

    [SerializeField] private Button _setToQuickSlotButton;
    [SerializeField] private Button _useButton;
    [SerializeField] private Button _removeButton;

    [SerializeField] private RemovePopUpUI _removePopUpUI;

    private bool _isOnPointer = false;
    private bool _isPointerDown = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isOnPointer = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isOnPointer = false;
    }

    void Update()
    {
        OnClick();
    }

    private void OnDisable()
    {
        _isOnPointer = false;
        _isPointerDown = false;
    }

    private void OnClick()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (!_isPointerDown)
            {
                gameObject.SetActive(false);
            }
            _isPointerDown = false;
        }
        if (Input.GetMouseButtonDown(0) && _isOnPointer)
        {
            _isPointerDown = true;
        }
    }

    public void InitButton(int index)
    {
        ItemData data = _inventoryUI.GetItemData(index);
        _setToQuickSlotButton.interactable = data is PotionItemData;
        _useButton.interactable = _inventoryUI.CheckItemUsable(index);
        _removeButton.interactable = true;

        _setToQuickSlotButton.onClick.AddListener(() =>
        {
            _inventoryUI.SetItemToQuickSlot(index);
            CloseUI();
        });

        _useButton.onClick.AddListener(() =>
        {
            _inventoryUI.UseItem(index);
            CloseUI();
        });

        _removeButton.onClick.AddListener(() =>
        {
            _removePopUpUI.SetItemData(index);
            CloseUI();
        });

    }

    private void CloseUI()
    {
        _setToQuickSlotButton.onClick.RemoveAllListeners();
        _useButton.onClick.RemoveAllListeners();
        _removeButton.onClick.RemoveAllListeners();
        gameObject.SetActive(false);
    }
}

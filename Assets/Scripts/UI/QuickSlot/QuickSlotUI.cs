using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickSlotUI : SlotInteractHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private QuickSlot _quickSlot;
    [SerializeField] private QuickSlotInGameUI _quickSlotInGameUI;

    [SerializeField] private Transform _slotsParent;
    [SerializeField] private Button _confirmButton;

    private List<SlotUI> _quickSlots = new List<SlotUI>();

    private int _selectedIndex = -1;
    private Transform _selectedSlot;
    private Vector3 _selectedSlotPosition;

    public void InitQuickSlotUI()
    {
        foreach (Transform child in _slotsParent)
        {
            _quickSlots.Add(child.GetComponent<SlotUI>());
        }

        _confirmButton.onClick.AddListener(SetQuickSlot);
    }

    public void SetQuickSlot()
    {
        _quickSlotInGameUI.SetQuickSlotUI();
        //gameObject.SetActive(false);
    }

    public void SetSlotImage(int index)
    {
        _quickSlots[index].SetItemInfo(_quickSlot.GetItemSprite(index));
    }

    public void RemoveSlot(int index)
    {
        _quickSlots[index].SetItemInfo(null);
        _quickSlot.RemoveSlot(index);
    }

    public Sprite GetItemSprite(int index)
    {
        return _quickSlot.GetItemSprite(index);
    }

    protected int GetItemSlotIndex(SlotUI slot)
    {
        return _quickSlots.IndexOf(slot);
    }

    #region Pointer Event

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_pointedSlot == null) return;

        _selectedIndex = GetItemSlotIndex(_pointedSlot);

        if (_quickSlot.GetItemSprite(_selectedIndex) == null)
        {
            _selectedIndex = -1;
            return;
        }

        _selectedSlot = _pointedSlot.transform;
        _selectedSlotPosition = _selectedSlot.position;
        _selectedSlot.GetComponent<Image>().raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_selectedIndex == -1) return;

        _selectedSlot.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_selectedIndex == -1) return;

        _quickSlot.SwapSlots(_selectedIndex, GetItemSlotIndex(_pointedSlot));

        // UI 배치 정상화
        _selectedSlot.position = _selectedSlotPosition;
        _selectedSlot.GetComponent<Image>().raycastTarget = true;
        _selectedIndex = -1;
        _selectedSlot = null;
        _selectedSlotPosition = Vector3.zero;
    }

    public override void OnRightClick()
    {

    }

    public override void OnLeftClick()
    {
    }

    public override void OnDoubleClick()
    {
    }

    public override void OnPointerIn()
    {
    }

    public override void OnPointerOut()
    {
    }

    #endregion
}

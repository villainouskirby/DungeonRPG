using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickSlotUI : SlotInteractHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private QuickSlot _quickSlot;

    [SerializeField] private Transform _slotsParent;
    [SerializeField] private Button _confirmButton;

    private List<Transform> _slotTransforms = new();
    private List<SlotUI> _quickSlots = new();
    private List<GameObject> _lockPanels = new();

    private int _selectedIndex = -1;
    private Transform _selectedSlot;
    private Vector3 _selectedSlotPosition;

    public void InitQuickSlotUI()
    {
        foreach (Transform child in _slotsParent)
        {
            _slotTransforms.Add(child);
            _quickSlots.Add(child.GetChild(0).GetComponent<SlotUI>());
            _lockPanels.Add(child.GetChild(1).gameObject);
        }

        for (int i = 0; i < _quickSlots.Count; i++)
        {
            SetSlotImage(i);
        }

        _confirmButton.onClick.AddListener(SetQuickSlot);
        _confirmButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    public void InitLockPanels(int slotCounts)
    {
        for (int i = 0; i < _lockPanels.Count; i++)
        {
            _lockPanels[i].SetActive(i >= slotCounts);
        }
    }

    public void SetQuickSlot()
    {
        UIPopUpHandler.Instance.GetScript<QuickSlotInGameUI>().SetQuickSlotUI();
    }

    public void SetSlotImage(int index)
    {
        Sprite sprite = _quickSlot.GetItemSprite(index);

        if (_quickSlot.GetItem(index) != null)
        {
            _quickSlots[index].SetItemInfo(sprite);
            _quickSlots[index].ChangeAlpha(1);

        }
        else
        {
            _quickSlots[index].ChangeAlpha(0);
        }
    }

    public void RemoveSlot(int index)
    {
        _quickSlots[index].ChangeAlpha(0);
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

        if (_quickSlot.GetItem(_selectedIndex) == null)
        {
            _selectedIndex = -1;
            return;
        }

        _slotTransforms[_selectedIndex].SetAsLastSibling();
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

        // UI 배치 정상화
        _selectedSlot.position = _selectedSlotPosition;
        _selectedSlot.GetComponent<Image>().raycastTarget = true;

        int droppedSlotIndex = GetItemSlotIndex(_pointedSlot);
        if (droppedSlotIndex == -1 || droppedSlotIndex >= _quickSlot.SlotCount) return;

        _quickSlot.SwapSlots(_selectedIndex, droppedSlotIndex);

        // 변수 초기화
        _selectedIndex = -1;
        _selectedSlot = null;
        _selectedSlotPosition = Vector3.zero;
    }

    public override void OnRightClick()
    {
        _quickSlot.UnRegisterSlot(GetItemSlotIndex(_pointedSlot));
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

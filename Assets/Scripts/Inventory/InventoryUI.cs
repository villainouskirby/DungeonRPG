using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour, IPointerClickHandler, IPointerMoveHandler, IPointerExitHandler
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] private InventoryPopUpUI _inventoryPopUpUI;

    [SerializeField] private Transform _invectoryContent;
    [SerializeField] private GameObject _itemSlotPrefab;

    [SerializeField] private TextMeshProUGUI _weightText;

    #region Interact Member Variables

    private GraphicRaycaster _raycaster;
    private List<RaycastResult> _rrList;

    /// <summary> 마우스가 올라가 있는 슬롯 </summary>
    private ItemSlotUI _pointedSlot;

    // 더블클릭 감지용 시간 저장
    private float clickTime = 0;

    #endregion

    private List<ItemSlotUI> _itemSlots = new List<ItemSlotUI>();

    /// <summary> 새 슬롯 추가 </summary>
    private void CreateSlot()
    {
        ItemSlotUI slotUI;
        GameObject newSlot = Instantiate(_itemSlotPrefab, _invectoryContent); // TODO => 임시로 새로 생길때마다 동적생성으로 해놨지만 나중에 Pool을 만들어 쓰는게 더 나을지도
        if ((slotUI = newSlot.GetComponent<ItemSlotUI>()) == null) slotUI = newSlot.AddComponent<ItemSlotUI>();
        _itemSlots.Add(slotUI);
    }

    /// <summary> 슬롯 기본 정보 등록 </summary>
    public void SetItemSlot(int index, ItemData data)
    {
        // 새로 추가된 경우
        if (_itemSlots.Count == index)
        {
            // 슬롯 생성
            CreateSlot();

            // 기본 정보 등록
            _itemSlots[index].SetItemInfo(data.IconSprite, data.Name, data.Weight);
        }
    }

    /// <summary> 아이템 수량 텍스트 수정 </summary>
    public void SetItemAmountText(int index, int amount = 1)
    {
        _itemSlots[index].SetItemAmount(amount);
    }

    /// <summary> 중량 텍스트 수정 </summary>
    public void UpdateWeightText(int currentCapacity, int maxCapacity)
    {
        // 텍스트 세팅
        _weightText.text = currentCapacity.ToString() + " / " + maxCapacity.ToString();

        // 현제 중량에 따른 색 변경
        Color color;
        if (currentCapacity >= 0.9 * maxCapacity)
        {
            color = Color.red;
        }
        else if (currentCapacity >= 0.8 * maxCapacity)
        {
            color = Color.yellow;
        }
        else
        {
            color = Color.black;
        }
        _weightText.color = color;
    }

    /// <summary> 중량 초과 알림 팝업 띄우기 </summary>
    public void OpenExcessPopUp() => _inventoryPopUpUI.OpenExcessPopUP();

    /// <summary> 슬롯 제거 </summary>
    public void RemoveSlot(int index)
    {
        Destroy(_itemSlots[index].gameObject); // TODO => 위에서 말한 Instantiate와 마찬가지
        _itemSlots.RemoveAt(index);
    }

    /// <summary> CountableItem을 특정 개수만큼 버리기 </summary>
    public void RemoveItem(int index, int amount)
    {
        _inventory.RemoveItem(index, amount);
    }

    /// <summary> 해당 슬롯의 모든 아이템 제거 </summary>
    public void RemoveItem(int index)
    {
        RemoveSlot(index);
        _inventory.RemoveItem(index);
    }

    /// <summary> 해당 슬롯의 데이터 불러오기 </summary>
    public ItemData GetItemData(int index)
    {
        return _inventory.GetItemData(index);
    }

    /// <returns> 해당 슬롯의 아이템 개수 </returns>
    public int GetItemAmount(int index)
    {
        return _inventory.GetItemAmount(index);
    }

    /// <returns> 해당 슬롯의 인덱스 값 </returns>
    public int GetItemSlotIndex(ItemSlotUI slot)
    {
        return _itemSlots.IndexOf(slot);
    }
    
    private T RaycastAndGetFirstComponent<T>(PointerEventData eventData) where T : Component
    {
        _rrList.Clear();
        _raycaster.Raycast(eventData, _rrList);

        if (_rrList.Count == 0) return null;

        return _rrList[0].gameObject.GetComponent<T>();
    }

    private void Awake()
    {
        _raycaster = GetGraphicRaycasterFromParent();
        _rrList = new List<RaycastResult>();
    }

    private GraphicRaycaster GetGraphicRaycasterFromParent()
    {
        Transform parent = transform.parent;

        while (parent != null)
        {
            GraphicRaycaster raycaster = parent.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                return raycaster;
            }
            parent = parent.parent;
        }

        return null; // 끝까지 탐색했지만 없으면 null 반환
    }

    #region Pointer Event

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_pointedSlot != null)
        {
            // 더블클릭 확인
            if (Input.GetMouseButtonUp(0))
            {
                if (Time.time - clickTime < 0.3f)
                {
                    // 더블 클릭 => 아이템 사용
                    Debug.Log("더블 클릭");
                    clickTime = -1;
                }
                else
                {
                    clickTime = Time.time;
                }
            }
            // 우클릭 확인
            else if (Input.GetMouseButtonUp(1))
            {
                _inventoryPopUpUI.OpenMenu(GetItemSlotIndex(_pointedSlot));
                _inventoryPopUpUI.CloseInfo();
            }
        }

    }

    public void OnPointerMove(PointerEventData eventData)
    {
        _pointedSlot = RaycastAndGetFirstComponent<ItemSlotUI>(eventData);

        if (_pointedSlot != null)
        {
            _inventoryPopUpUI.OpenInfo(GetItemSlotIndex(_pointedSlot));
        }
        else
        {
            _inventoryPopUpUI.CloseInfo();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _inventoryPopUpUI.CloseInfo();
    }

    #endregion
}

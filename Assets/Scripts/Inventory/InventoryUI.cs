using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory _inventory;

    [SerializeField] private Transform _invectoryContent;
    [SerializeField] private GameObject _itemSlotPrefab;

    [SerializeField] private TextMeshProUGUI weightText;
    
    private List<ItemSlotUI> _itemSlots = new List<ItemSlotUI>();

    private void CreateSlot()
    {
        ItemSlotUI slotUI;
        GameObject newSlot = Instantiate(_itemSlotPrefab, _invectoryContent); // TODO => 임시로 새로 생길때마다 동적생성으로 해놨지만 나중에 Pool을 만들어 쓰는게 더 나을지도
        if ((slotUI = newSlot.GetComponent<ItemSlotUI>()) == null) slotUI = newSlot.AddComponent<ItemSlotUI>();
        _itemSlots.Add(slotUI);
    }

    public void UpdateWeightText(int currentCapacity, int maxCapacity)
    {
        // 텍스트 세팅
        weightText.text = currentCapacity.ToString() + " / " + maxCapacity.ToString();

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
        weightText.color = color;
    }

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

    public void SetItemAmountText(int index, int amount = 1)
    {
        _itemSlots[index].SetItemAmount(amount);
    }

    public void RemoveSlot(int index)
    {
        Destroy(_itemSlots[index].gameObject); // TODO => 위에서 말한 Instantiate와 마찬가지
        _itemSlots.RemoveAt(index);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class QuickSlot : MonoBehaviour
{
    [SerializeField] private QuickSlotUI _quickSlotUI;

    [SerializeField] private ItemsSO _quickSlotList;

    private List<Item> _quickSlot => _quickSlotList.Items;

    private void Awake()
    {
        if (_quickSlot.Count == 0) _quickSlotList.Items = new List<Item>(new Item[5]);
        _quickSlotUI.InitQuickSlot();
    }

   
    private void OnEnable()
    {
        for (int i = 0; i < _quickSlot.Count; i++)
        {
            _quickSlotUI.SetSlotImage(i);
        }
    }

    public bool AddToSlot(Item item)
    {
        for (int i = 0; i < _quickSlot.Count; i++)
        {
            if (_quickSlot[i] == null)
            {
                _quickSlot[i] = item;
                _quickSlotUI.SetSlotImage(i);

                return true;
            }
        }

        // 슬롯 가득 참
        return false;
    }

    public void RemoveSlot(int index)
    {
        _quickSlot[index] = null;
    }

    public Sprite GetItemSprite(int index)
    {
        return (_quickSlot.Count > index && index >= 0 && _quickSlot[index] != null) ? _quickSlot[index].Data.IconSprite : null;
    }

    public void SwapSlots(int idx1, int idx2)
    {
        Item tempItem = _quickSlot[idx1];
        _quickSlot[idx1] = _quickSlot[idx2];
        _quickSlot[idx2] = tempItem;

        _quickSlotUI.SetSlotImage(idx1);
        _quickSlotUI.SetSlotImage(idx2);
    }
}
////임시
///* ▼ 인스펙터에 노출될 스크립트 오브젝트 배열 */
//[SerializeField] private List<ItemData> _defaultItemDatas = new List<ItemData>(5);
////임시

////임시
//private void Start()          // 여기서 스크립트오브젝트 → Item 변환
//{
//    for (int i = 0; i < _defaultItemDatas.Count && i < _quickSlot.Count; i++)
//    {
//        if (_defaultItemDatas[i] == null) continue;

//        _quickSlot[i] = _defaultItemDatas[i].Createitem();   // 인스턴스 생성
//        _quickSlotUI.SetSlotImage(i);                        // 아이콘 갱신
//    }
//}
////임시
using System.Collections.Generic;
using UnityEngine;
using ItemDataExtensions;
using DBUtility;
using static ItemDataExtensions.ItemDataExtension;
using System;
using UnityEngine.AddressableAssets;

[System.Serializable]
public abstract class ItemData
{
    public string SID => _info.id;
    public string Name => _info.name;
    public Sprite IconSprite => _iconSprite;

    public Item_Info_Item Info => _info;
    public Dictionary<ItemDataExtension.Name, ItemDataExtension> Extensions => _extensions;

    [SerializeReference] private Item_Info_Item _info;
    [SerializeReference] private Dictionary<ItemDataExtension.Name, ItemDataExtension> _extensions = new();

    [SerializeField] private Sprite _iconSprite;

    public ItemData(Item_Info_Item info)
    {
        _info = info;

        try
        {
            _iconSprite = Addressables.LoadAssetAsync<Sprite>("ItemSprites/" + info.id).WaitForCompletion();
        }
        catch (Exception ex)
        {
            Debug.LogError($"이미지 로드 실패: {ex.Message}");
            _iconSprite = null;
        }
        
        if (_info.throwable)
        {
            _extensions[ItemDataExtension.Name.Throwable] = new ThrowableItemDataExtension(SID);
        }
    }

    public ItemData()
    {

    }

    /// <summary> 타입에 맞는 새로운 아이템 생성 </summary>
    public abstract Item Createitem();
}

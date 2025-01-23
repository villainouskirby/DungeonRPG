using UnityEngine;

public class InventoryPopUpUI : MonoBehaviour
{
    [SerializeField] private InventoryUI _inventoryUI;

    [SerializeField] private GameObject _infoPopUp;
    [SerializeField] private GameObject _menuPopUp;

    private ItemInfo _info;
    private InventoryMenu _menu;

    private void Awake()
    {
        _info = _infoPopUp.GetComponent<ItemInfo>();
        _menu = _menuPopUp.GetComponent<InventoryMenu>();

        if (_info == null) _info = _infoPopUp.AddComponent<ItemInfo>();
        if (_menu == null) _menu = _menuPopUp.AddComponent<InventoryMenu>();
    }

    public void OpenInfo(int index)
    {
        _info.SetInfo(_inventoryUI.GetItemData(index));
        if (!_menuPopUp.activeSelf)
        {
            _infoPopUp.transform.position = Input.mousePosition;
            _infoPopUp.SetActive(true);
        }
    }

    public void OpenMenu(int index)
    {
        _menuPopUp.SetActive(false);
        _menuPopUp.transform.position = Input.mousePosition;
        _menu.InitButton(_inventoryUI.GetItemData(index), index);
        _menuPopUp.SetActive(true);
    }

    public void CloseInfo() => _infoPopUp.SetActive(false);

    public void CloseMenu() => _menuPopUp.SetActive(false);
    
}

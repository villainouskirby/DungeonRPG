using UnityEngine;

public class InventoryPopUpUI : ItemPopUpUI
{
    [SerializeField] private InventoryUI _inventoryUI;
    [SerializeField] private ExcessPopUpUI _excessPopUpUI;
    [SerializeField] private GameObject _menuPopUp;

    private InventoryMenu _menu;

    protected override void Awake()
    {
        base.Awake();

        if (_menuPopUp == null) return;

        _menu = _menuPopUp.GetComponent<InventoryMenu>();
        if (_menu == null) _menu = _menuPopUp.AddComponent<InventoryMenu>();
    }

    public void OpenInfo(int index)
    {
        ItemData data = _inventoryUI.GetItemData(index);
        if (data == null)
        {
            CloseInfo();
            return;
        }

        if (_menuPopUp == null || !_menuPopUp.activeSelf)
        {
            base.OpenInfo(data);
        }
    }

    public void OpenMenu(int index)
    {
        _menuPopUp.SetActive(false);
        _menuPopUp.transform.position = Input.mousePosition;
        _menu.InitButton(index);
        _menuPopUp.SetActive(true);
    }

    public void OpenExcessPopUP() => _excessPopUpUI.OpenExcessPopUp();
}

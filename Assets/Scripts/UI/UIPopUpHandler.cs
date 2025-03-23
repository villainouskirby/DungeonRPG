using UnityEngine;

public class UIPopUpHandler : MonoBehaviour
{
    [SerializeField] private GameObject _inventory;
    [SerializeField] private GameObject _shop;
    [SerializeField] private GameObject _storage;
    [SerializeField] private GameObject _quest;
    [SerializeField] private GameObject _smith;

    private Inventory _inventoryScript;
    private Shop _shopScript;
    private Storage _storageScript;

    private GameObject _openUI;

    private static UIPopUpHandler _instance;
    public static UIPopUpHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.Log("인스턴스가 존재하지 않음");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        _inventoryScript = _inventory.GetComponent<Inventory>();
        _shopScript = _shop.GetComponent<Shop>();
        _storageScript = _storage.GetComponent<Storage>();

        InitAllUI();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            if (_openUI == null) return;
            
            _openUI.SetActive(false);
            _openUI = null;
        }
    }

    private void InitAllUI()
    {
        _inventory.SetActive(true);
        _shop.SetActive(true);
        _storage.SetActive(true);
        _quest.SetActive(true);
        _smith.SetActive(true);

        _inventory.SetActive(false);
        _shop.SetActive(false);
        _storage.SetActive(false);
        _quest.SetActive(false);
        _smith.SetActive(false);
    }

    private void OpenUI(GameObject ui)
    {
        if (_openUI != null) return;

        ui.SetActive(true);
        _openUI = ui;
    }

    /// <summary> 인벤토리 열기 </summary>
    public void OpenInventory() // TODO => 매번 호출할때마다 초기화시키는건 비효율적인데 뭔가 개선방안이 필요할듯
    {
        _inventoryScript.InitInventory();
        OpenUI(_inventory);
    }

    /// <summary> 상점 열기 </summary>
    public void OpenShop()
    {
        _shopScript.InitInvenToShop();
        OpenUI(_shop);
    }

    /// <summary> 창고 열기 </summary>
    public void OpenStorage()
    {
        _storageScript.InitInventory();
        OpenUI(_storage);
    }

    /// <summary> 퀘스트 게시판 열기 </summary>
    public void OpenQuest() => OpenUI(_quest);

    /// <summary> 대장장이 UI 열기</summary>
    public void OpenSmith() => OpenUI(_smith);
}

using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    // 임시로 이렇게 만들고 UI 관련해서 좀더 생각해서 정리해야겠음
    [SerializeField] private Button _inventoryButton;
    [SerializeField] private Button _mapButton;
    [SerializeField] private Button _questButton;
    [SerializeField] private Button _documentButton;

    private void Start()
    {
        _inventoryButton.onClick.AddListener(UIPopUpHandler.Instance.OpenInventory);
        //_mapButton.onClick.AddListener(UIPopUpHandler.Instance.open); // 맵 코드 가져와야함
        _questButton.onClick.AddListener(UIPopUpHandler.Instance.OpenQuest);
    }
}

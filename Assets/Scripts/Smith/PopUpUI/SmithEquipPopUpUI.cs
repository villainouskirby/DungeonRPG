using UnityEngine;
using UnityEngine.UI;

public class SmithEquipPopUpUI : MonoBehaviour
{
    [SerializeField] private SmithPopUpUI _smithPopUpUI;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    private void Start()
    {
        _confirmButton.onClick.AddListener(() => _smithPopUpUI.EquipItem());
        _confirmButton.onClick.AddListener(() => gameObject.SetActive(false));
        _cancelButton.onClick.AddListener(() => gameObject.SetActive(false));
    }
}

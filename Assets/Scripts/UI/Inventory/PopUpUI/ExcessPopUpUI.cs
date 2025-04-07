using UnityEngine;
using UnityEngine.UI;

public class ExcessPopUpUI : MonoBehaviour
{
    [SerializeField] private Button _closeButton;

    void Start()
    {
        _closeButton.onClick.AddListener(() => gameObject.SetActive(false));   
    }

    public void OpenExcessPopUp() => gameObject.SetActive(true);
}

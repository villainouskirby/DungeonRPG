using UnityEngine;
using UnityEngine.UI;

public class ExcessPopUpUI : UIBase
{
    [SerializeField] private Button _closeButton;

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
    }

    void Start()
    {
        _closeButton.onClick.AddListener(() => gameObject.SetActive(false));   
    }

    public void OpenExcessPopUp() => gameObject.SetActive(true);
}

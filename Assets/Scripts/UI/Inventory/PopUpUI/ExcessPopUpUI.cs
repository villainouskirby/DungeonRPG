using UnityEngine;
using UnityEngine.UI;

public class ExcessPopUpUI : UIBase
{
    [SerializeField] private Button _closeButton;

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
    }

    protected override void Start()
    {
        base.Start();
        _closeButton.onClick.AddListener(() => gameObject.SetActive(false));   
    }

    public void OpenExcessPopUp() => gameObject.SetActive(true);
}

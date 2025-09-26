public class InteractUI : UIBase
{
    public void OpenInteractPopUp()
    {


        gameObject.SetActive(true);
    }

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
    }
}
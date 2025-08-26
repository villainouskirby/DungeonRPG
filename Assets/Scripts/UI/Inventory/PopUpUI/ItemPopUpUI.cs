using UnityEngine;

public class ItemPopUpUI : MonoBehaviour // 삭제나 통합 예정
{
    [SerializeField] protected GameObject _infoPopUp;

    protected ItemInfo _info;

    protected virtual void Awake()
    {
        //_info = _infoPopUp.GetComponent<ItemInfo>();

        //if (_info == null) _info = _infoPopUp.AddComponent<ItemInfo>();
    }
    
    public void OpenInfo(ItemData data)
    {
        //_info.SetInfo(data);
        //_infoPopUp.transform.position = Input.mousePosition;
        //_infoPopUp.SetActive(true);
    }

    public void CloseInfo() { }//_infoPopUp.SetActive(false);
}

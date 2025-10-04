using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerFarming : MonoBehaviour
{
    public static PlayerFarming Instance { get { return _instance; } }
    private static PlayerFarming _instance;

    private List<GameObject> _rangedResourceNodeObj = new();
    private List<DropItem> _rangedDropItem = new();


    [Header("Outline Color Settings")]
    public Color AbleColor;
    public Color DisableColor;



    [Header("Targeting Settings")]
    public GameObject IconObj;
    public LayerMask ResourceNodeLayerMask;
    [HideInInspector]
    public FarmIconFunc FarmIcon;
    public Vector3 TargetingObjPosCorrect = new(0, 0.2f);

    [HideInInspector]
    public DropItem TargetDropItem;
    [HideInInspector]
    public GameObject TargetObj;
    [HideInInspector]
    public ResourceNodeBase TargetResourceNode;
    [HideInInspector]
    public OutlineGenerator TargetResourceNodeOutline;
    public bool IsMouseSelect;
    public bool IsTargeting;
    public bool IsFarming;

    [Header("Player Settings")]
    public int Level;
    public KeyCode FarmingKey = KeyCode.F;
    public KeyCode DropItemKey = KeyCode.U;

    [Header("Farm Settings")]
    public int MinLevelDifference = -3;
    public GameObject FarmGageBarObj;
    [HideInInspector]
    public FarmGageBarFunc FarmGageBar;

    private Camera _mainCamera;


    private void Start()
    {
        _instance = this;
        _rangedResourceNodeObj = new();
        TargetResourceNode = null;
        TargetObj = null;
        IsMouseSelect = false;
        IsTargeting = false;
        IsFarming = false;
        FarmGageBar = Instantiate(FarmGageBarObj, transform, false).GetComponent<FarmGageBarFunc>();
        _mainCamera = Camera.main;
    }

    private float   _minDistance;
    private int     _targetIndex;
    private void FixedUpdate()
    {
        if (!IsTargeting)
            return;
        if (IsFarming)
            return;

        if (IsMouseSelect)
        {
            return;
        }

        _minDistance = 9999999;
        _targetIndex = 0;

        for (int i = 0; i < _rangedResourceNodeObj.Count; i++)
        {
            GameObject farmObj = _rangedResourceNodeObj[i];
            float distance = Mathf.Pow(farmObj.transform.position.x - transform.position.x, 2)
                             + Mathf.Pow(farmObj.transform.position.y - transform.position.y, 2);
            if (_minDistance >= distance)
            {
                _minDistance = distance;
                _targetIndex = i;
            }
        }

        if (_targetIndex < _rangedResourceNodeObj.Count && TargetObj != _rangedResourceNodeObj[_targetIndex])
        {
            SelectFarm(_rangedResourceNodeObj[_targetIndex]);
        }
    }

    public void AddDropItem(DropItem item)
    {
        _rangedDropItem.Add(item);
        TargetDropItem = item;
    }

    public void RemoveDropItem(DropItem item)
    {
        _rangedDropItem.Remove(item);
        if (TargetDropItem == item)
        {
            if (_rangedDropItem.Count > 0)
                TargetDropItem = _rangedDropItem[0];
            else
                TargetDropItem = null;
        }        
    }

    private Color GetOutlineColor()
    {
        return CheckFarmable() ? AbleColor : DisableColor;
    }

    private void SelectFarm(GameObject target)
    {
        //if (TargetResourceNodeOutline != null)
            //TargetResourceNodeOutline.OffOutline();

        TargetObj = target;
        TargetResourceNode = TargetObj.GetComponent<ResourceNodeBase>();
        TargetResourceNodeOutline = TargetObj.transform.GetChild(1).GetComponent<OutlineGenerator>();
        TargetResourceNodeOutline.OnOutline(GetOutlineColor());

        if (FarmIcon == null)
            FarmIcon = Instantiate(IconObj, TargetObj.transform.position + TargetingObjPosCorrect, Quaternion.identity).GetComponent<FarmIconFunc>();

        FarmIcon.gameObject.SetActive(true);
        FarmIcon.transform.position = TargetObj.transform.position + TargetingObjPosCorrect;
        FarmIcon.SetIcon(CheckFarmable());
    }

    private void Update()
    {
        if (IsFarming)
        {
            _time += Time.deltaTime;
            FarmGageBar.SetGage(1f - _time / _requiredTime);

            // 시간이 지나면 자동 채집? 일단 추가
            if (_time >= _requiredTime)
            {
                IsFarming = false;
                SuccessFarm();
            }
        }

        if (Input.GetKeyDown(DropItemKey))
        {
            if (TargetDropItem != null)
                TargetDropItem.Get();
        }

        if (!IsTargeting)
            return;

        Vector3 pos = Input.mousePosition;
        pos.z = transform.position.z - _mainCamera.transform.position.z;
        Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(pos);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, ResourceNodeLayerMask);
        if (hit.collider != null && hit.collider.gameObject != TargetObj)
        {
            GameObject firstFarm = hit.collider.gameObject;
            if (_rangedResourceNodeObj.Contains(firstFarm))
            {
                IsMouseSelect = true;
                SelectFarm(firstFarm);
            }
        }
        else
        {
            IsMouseSelect = false;
        }

        if (Input.GetKeyDown(FarmingKey))
        {
            StartFarm();
        }
        if (Input.GetKeyUp(FarmingKey))
        {
            EndFarm();
        }
    }

    private float _time;
    private float _requiredTime;

    void StartFarm()
    {
        if (CheckFarmable())
        {
            IsFarming = true;
            FarmGageBar.gameObject.SetActive(true);
            _time = 0;
            _requiredTime = 1f; // 임시로 제작
        }
    }

    private bool CheckFarmable()
    {
        int levelDifference = Level - TargetResourceNode.Info.Resistance;
        return levelDifference >= MinLevelDifference;
    }


    void EndFarm()
    {
        if (!IsFarming)
            return;

        if (IsFarmSuccess())
        {
            SuccessFarm();
            // 채집 성공
        }
        else
        {
            IsFarming = false;
            _time = 0;
            FarmGageBar.gameObject.SetActive(false);
            // 채집 실패 - 사유 시간 끝나기전에 손을 땜.
            Debug.Log("채집 실패 - 시간 부족");
        }
    }

    private void SuccessFarm()
    {
        IsFarming = false;
        _time = 0;
        FarmGageBar.gameObject.SetActive(false);

        // 채집 성공 - 임시 코드
        TargetResourceNode.GetItem(ClearFarm);
    }

    private void ClearFarm()
    {
        _rangedResourceNodeObj.Remove(TargetObj);
        SpawnerPool.Instance.ResourceNodePool.Return(TargetResourceNode);

        Debug.Log("채집 성공!");
        ResetFarm();
    }

    private bool IsFarmSuccess()
    {
        return _time >= _requiredTime;
    }

    private void ResetFarm()
    {
        _time = 0;
        _requiredTime = 0;
        IsFarming = false;
        FarmGageBar.gameObject.SetActive(false);
        TargetObj = null;
        TargetResourceNode = null;
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("FarmRange"))
        {
            _rangedResourceNodeObj.Add(collision.transform.parent.gameObject);

            if (_rangedResourceNodeObj.Count >= 1)
                IsTargeting = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("FarmRange"))
        {
            _rangedResourceNodeObj.Remove(collision.transform.parent.gameObject);
            if (collision.transform.parent.gameObject == TargetObj)
            {
                //TargetResourceNodeOutline.OffOutline();
                TargetObj = null;
                TargetResourceNode = null;
                //TargetResourceNodeOutline = null;
                if (IsFarming)
                {
                    Debug.Log("채집 실패 - 범위를 벗어남");
                    ResetFarm();
                }
            }
            if (_rangedResourceNodeObj.Count <= 0)
            {
                IsTargeting = false;
                IsFarming = false;
                TargetObj = null;
                TargetResourceNode = null;
                //TargetResourceNodeOutline = null;
                FarmIcon.gameObject.SetActive(false);
            }
        }
    }
}

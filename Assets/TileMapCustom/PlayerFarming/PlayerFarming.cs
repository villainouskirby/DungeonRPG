using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerFarming : MonoBehaviour
{
    public static PlayerFarming Instance { get { return _instance; } }
    private static PlayerFarming _instance;

    private List<GameObject> _rangedFarmObj = new();


    [Header("Outline Color Settings")]
    public Color AbleColor;
    public Color DisableColor;



    [Header("Targeting Settings")]
    public GameObject IconObj;
    public LayerMask FarmLayerMask;
    [HideInInspector]
    public FarmIconFunc FarmIcon;
    public Vector3 TargetingObjPosCorrect = new(0, 0.2f);

    [HideInInspector]
    public GameObject TargetObj;
    [HideInInspector]
    public FarmableBase TargetFarm;
    [HideInInspector]
    public OutlineGenerator TargetFarmOutline;
    public bool IsMouseSelect;
    public bool IsTargeting;
    public bool IsFarming;

    [Header("Player Settings")]
    public int Level;
    public KeyCode FarmingKey = KeyCode.F;

    [Header("Farm Settings")]
    public int MinLevelDifference = -3;
    public GameObject FarmGageBarObj;
    [HideInInspector]
    public FarmGageBarFunc FarmGageBar;

    private Camera _mainCamera;


    private void Start()
    {
        _rangedFarmObj = new();
        TargetFarm = null;
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

        for (int i = 0; i < _rangedFarmObj.Count; i++)
        {
            GameObject farmObj = _rangedFarmObj[i];
            float distance = Mathf.Pow(farmObj.transform.position.x - transform.position.x, 2)
                             + Mathf.Pow(farmObj.transform.position.y - transform.position.y, 2);
            if (_minDistance >= distance)
            {
                _minDistance = distance;
                _targetIndex = i;
            }
        }

        if (TargetObj != _rangedFarmObj[_targetIndex])
        {
            SelectFarm(_rangedFarmObj[_targetIndex]);
        }
    }

    private Color GetOutlineColor()
    {
        return CheckFarmable() ? AbleColor : DisableColor;
    }

    private void SelectFarm(GameObject target)
    {
        if (TargetFarmOutline != null)
            TargetFarmOutline.OffOutline();

        TargetObj = target;
        TargetFarm = TargetObj.GetComponent<FarmableBase>();
        TargetFarmOutline = TargetObj.transform.GetChild(1).GetComponent<OutlineGenerator>();
        TargetFarmOutline.OnOutline(GetOutlineColor());

        if (FarmIcon == null)
            FarmIcon = Instantiate(IconObj, TargetObj.transform.position + TargetingObjPosCorrect, Quaternion.identity).GetComponent<FarmIconFunc>();

        FarmIcon.gameObject.SetActive(true);
        FarmIcon.transform.position = TargetObj.transform.position + TargetingObjPosCorrect;
        FarmIcon.SetIcon(TargetFarm.Type, CheckFarmable());
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

        if (!IsTargeting)
            return;

        Vector3 pos = Input.mousePosition;
        pos.z = transform.position.z - _mainCamera.transform.position.z;
        Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(pos);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, FarmLayerMask);
        if (hit.collider != null && hit.collider.gameObject != TargetObj)
        {
            GameObject firstFarm = hit.collider.gameObject;
            if (_rangedFarmObj.Contains(firstFarm))
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
        int levelDifference = Level - TargetFarm.Level;
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

        Debug.Log("실행됨");
        // 채집 성공 - 임시 코드
        UIPopUpHandler.Instance.InventoryScript.AddItem(TargetFarm.DropItemList[0]);
        _rangedFarmObj.Remove(TargetObj);
        switch (TargetFarm.Type)
        {
            case FarmEnum.Plant:
                SpawnerPool.Instance.PlantPool.Release(TargetFarm.PlantType, TargetObj);
                break;
            case FarmEnum.Mineral:
                SpawnerPool.Instance.MineralPool.Release(TargetFarm.MineralType, TargetObj);
                break;
        }

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
        TargetFarm = null;
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("FarmRange"))
        {
            _rangedFarmObj.Add(collision.transform.parent.gameObject);

            if (_rangedFarmObj.Count >= 1)
                IsTargeting = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("FarmRange"))
        {
            _rangedFarmObj.Remove(collision.transform.parent.gameObject);
            if (collision.transform.parent.gameObject == TargetObj)
            {
                TargetFarmOutline.OffOutline();
                TargetObj = null;
                TargetFarm = null;
                TargetFarmOutline = null;
                if (IsFarming)
                {
                    Debug.Log("채집 실패 - 범위를 벗어남");
                    ResetFarm();
                }
            }
            if (_rangedFarmObj.Count <= 0)
            {
                IsTargeting = false;
                IsFarming = false;
                TargetObj = null;
                TargetFarm = null;
                TargetFarmOutline = null;
                FarmIcon.gameObject.SetActive(false);
            }
        }
    }
}

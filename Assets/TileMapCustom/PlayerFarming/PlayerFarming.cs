using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


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
    //public OutlineGenerator TargetResourceNodeOutline;
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
    [SerializeField] private float baseRequiredTime = 1f;
    public float GetRequiredTime() => Mathf.Max(0.05f, baseRequiredTime);

    bool IsMonsterTarget() => TargetObj != null && TargetObj.CompareTag("Monster");

    private void Start()
    {
        FarmingKey = KeyCode.F;
        DropItemKey = KeyCode.F;
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
        if (!IsMonsterTarget())
        {
            TargetResourceNode = TargetObj.GetComponent<ResourceNodeBase>();
            // 방어적 접근
            var t = TargetObj.transform;
        }
        else
        {
            // 몬스터일 땐 자원 참조/아웃라인 비움
            TargetResourceNode = null;
        }

        if (FarmIcon == null)
            FarmIcon = Instantiate(IconObj, TargetObj.transform.position + TargetingObjPosCorrect, Quaternion.identity).GetComponent<FarmIconFunc>();

        FarmIcon.gameObject.SetActive(true);
        FarmIcon.transform.position = TargetObj.transform.position + TargetingObjPosCorrect;
        FarmIcon.SetIcon(CheckFarmable());
    }

    private void Update()
    {
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
                SelectFarm(firstFarm);
            }
        }
    }
    private float _time;


    private bool CheckFarmable()
    {
        if (IsMonsterTarget())
        {
            // 페이드(소멸) 중이면 상호작용 금지
            if (MonsterKilledState.IsDespawning(TargetObj)) return false;
            if (MonsterKilledState.HarvestsLeft(TargetObj) <= 0) return false;
            // 난이도 필요 시 Monster 측 데이터로 바꿔도 됨. 우선 0으로.
            int resistance = 0;
            int levelDifferencemon = Level - resistance;
            return levelDifferencemon >= MinLevelDifference;
        }

        // 자원일 때 NPE 방지
        if (TargetResourceNode == null || TargetResourceNode.Info == null) return false;
        int levelDifference = Level - TargetResourceNode.Info.Resistance;
        return levelDifference >= MinLevelDifference;
    }

    public void SuccessFarm()
    {
        IsFarming = false;
        _time = 0;
        FarmGageBar.gameObject.SetActive(false);

        if (IsMonsterTarget())
        {
            // 아이템 지급. 0이 되면 MonsterKilledState가 페이드 시작.
            MonsterKilledState.OnFarmHarvest(TargetObj);

            int left = MonsterKilledState.HarvestsLeft(TargetObj);

            if (left <= 0 || MonsterKilledState.IsDespawning(TargetObj))
            {
                _rangedResourceNodeObj.Remove(TargetObj);
                ResetFarm();
                Debug.Log("몬스터 파밍 완료");
            }
            else
            {
                Debug.Log($"몬스터 파밍 1회 완료. 남은 횟수: {left}");
                FarmIcon?.SetIcon(CheckFarmable());
            }
            return;
        }

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


    private void ResetFarm()
    {
        FarmGageBar.gameObject.SetActive(false);
        TargetObj = null;
        TargetResourceNode = null;
    }

    public bool CanFarmNow()
    {
        if (!IsTargeting || TargetObj == null) return false;

        if (IsMonsterTarget())
        {
            if (MonsterKilledState.IsDespawning(TargetObj)) return false;
            if (MonsterKilledState.HarvestsLeft(TargetObj) <= 0) return false;
            int resistance = 0; // 필요하면 몬스터 데이터로
            return (Level - resistance) >= MinLevelDifference;
        }

        if (TargetResourceNode == null || TargetResourceNode.Info == null) return false;
        return (Level - TargetResourceNode.Info.Resistance) >= MinLevelDifference;
    }
    public void BeginFarmVisuals()
    {
        if (FarmGageBar != null)
        {
            FarmGageBar.gameObject.SetActive(true);
            FarmGageBar.SetGage(1f);
        }

        // 아이콘/아웃라인도 최신 가능여부 색으로
        FarmIcon?.SetIcon(CanFarmNow());
    }

    // 상태머신에서 호출: 취소/실패
    public void CancelFarmVisuals()
    {
        if (FarmGageBar != null) FarmGageBar.gameObject.SetActive(false);
    }

    // 상태머신에서 호출: 프레임마다 게이지만 업데이트(0~1)
    public void UpdateGage(float remain, float total)
    {
        if (FarmGageBar != null && total > 0f)
            FarmGageBar.SetGage(Mathf.Clamp01(1f - (total - remain) / total));
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("FarmRange"))
        {
            GameObject parentGo = collision.transform.parent ? collision.transform.parent.gameObject : collision.gameObject;
            if (!_rangedResourceNodeObj.Contains(parentGo))
                _rangedResourceNodeObj.Add(parentGo);

            if (_rangedResourceNodeObj.Count >= 1)
                IsTargeting = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("FarmRange"))
        {
            GameObject parentGo = collision.transform.parent ? collision.transform.parent.gameObject : collision.gameObject;

            _rangedResourceNodeObj.Remove(parentGo);
            if (parentGo == TargetObj)
            {
                TargetObj = null;
                TargetResourceNode = null;

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
                FarmIcon?.gameObject.SetActive(false);
            }
        }
    }
}

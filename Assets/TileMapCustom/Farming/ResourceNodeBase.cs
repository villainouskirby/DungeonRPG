
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[RequireComponent(typeof(PolygonCollider2D))]
public class ResourceNodeBase : MonoBehaviour
{

    public ResourceNode_Info_ResourceNode Info;
    public ResourceNode_Info_ResourceNode_DropTable DropTable;
    public List<ItemData> DropItemList;

    [HideInInspector]
    public Vector2Int TilePos;

    // 채집물 피격 채집 관련
    public float CurrentHp;
    public bool[] DropAble;
    public float DropHpCut;

    // 채집물 채력 관련
    public GameObject HpBar;
    public FarmHpBarFunc HpBarFunc;
    public float HpBarRemainTime = 2;
    public float HpBarCloseTime = 3;
    public bool HpBarShowing;
    public Coroutine _hpCloseC;
    public static GameObject HpBarPrefab;

    private MaterialPropertyBlock _block;
    private SpriteRenderer _renderer;
    private bool _isCheckVisible;

    private void Awake()
    {
        Init();
    }

    public void Set(ResourceNode_Info_ResourceNode info)
    {
        Info = info;
        DropAble = new bool[Info.Gathering_count];
        Array.Fill(DropAble, true);
        CurrentHp = Info.Hp;
        DropHpCut = CurrentHp / (float)Info.Gathering_count;
    }


    // 기본값을 Init한다. 최초 1회
    public void Init()
    {
        _renderer = GetComponent<SpriteRenderer>();
        HpBar = Instantiate(HpBarPrefab, transform, false);
        HpBarFunc = HpBar.GetComponent<FarmHpBarFunc>();
    } 

    public void ShowHpBar()
    {
        HpBar.SetActive(true);
        HpBarFunc.SetAlpha(1);
        HpBarFunc.SetGage(CurrentHp / Info.Hp);
    }

    public IEnumerator ClosedHpBar(float remainTime ,float closeTime)
    {
        yield return new WaitForSeconds(remainTime);

        float currentTime = 0;
        while (currentTime <= closeTime)
        {
            yield return null;
            currentTime += Time.deltaTime;
            HpBarFunc.SetAlpha((closeTime - currentTime) / closeTime);
            HpBarFunc.SetGage(CurrentHp / Info.Hp);
        }
        HpBar.SetActive(false);
    }

    public void Damage(float damage)
    {
        if (!Info.isDestructible)
        {
            // 파괴 불가능한 채집물 임으로 이펙트를 따로 표시할 것
            return;
        }

        CurrentHp -= damage;
        if (CurrentHp < 0)
            CurrentHp = 0;

        ShowHpBar();
        if (_hpCloseC != null)
            StopCoroutine(_hpCloseC);
        _hpCloseC = StartCoroutine(ClosedHpBar(HpBarRemainTime ,HpBarCloseTime));

        for (int i = 0; i < Info.Gathering_count; i++)
        {
            if (DropAble[i])
            {
                if (CurrentHp <= Info.Hp - (i + 1) * DropHpCut)
                {
                    DropAble[i] = false;
                    DropItem(i);
                }
            }
            else
                continue;
        }

        if(CurrentHp <= 0)
        {
            SpawnerPool.Instance.ResourceNodePool.Return(this);
        }
    }

    public void DropItem(int num)
    {
        DropItem dropItem = DropItemPool.Instance.Get(DropItemList[0]);
        dropItem.gameObject.transform.position = transform.position + new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 0);
        dropItem.gameObject.SetActive(true);
    }

    public void GetItem(Action farmEnd)
    {
        for (int i = 0; i < Info.Gathering_count; i++)
        {
            if (DropAble[i])
            {
                DropAble[i] = false;
                // 임시 코드 나중에 드랍테이블과 연동 JJJJ
                UIPopUpHandler.Instance.InventoryScript.AddItem(DropItemList[0]);

                CurrentHp = Info.Hp - (i + 1) * DropHpCut;
                ShowHpBar();
                if (_hpCloseC != null)
                    StopCoroutine(_hpCloseC);
                _hpCloseC = StartCoroutine(ClosedHpBar(HpBarRemainTime, HpBarCloseTime));

                if (i == Info.Gathering_count - 1)
                    farmEnd.Invoke();

                break;
            }
            else
                continue;
        }
    }

    public void FarmReset()
    {
        _isCheckVisible = false;
        TilePos = new(0, 0);
        CurrentHp = Info.Hp;
        // 수확이 되거나 해서 돌아간 경우.
        // 초기화 관련 로직이 들어갈듯?
    }

    // 로직상 활성화 비활성화 되는 순간은
    // 스폰/디스폰 될때 말고는 없다. 따라서 초기화 Init은 여기에 추가한다.
    // 만약 로직이 바뀐다면 수정할 것

    private void OnDisable()
    {
        FarmReset();
    }

    // 시아에 처음 보인 시점
    private void OnEnable()
    {
        // 기본값을 전부 세팅해준다.
        TilePos = new(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        _isCheckVisible = true;
    }
}

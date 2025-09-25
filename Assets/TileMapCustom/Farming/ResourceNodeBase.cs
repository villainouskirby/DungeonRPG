
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

[RequireComponent(typeof(PolygonCollider2D))]
public class ResourceNodeBase : MonoBehaviour
{
    public static SpriteAtlas SpriteAtlas;

    public ResourceNode_Info_ResourceNode Info;

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

    private MaterialPropertyBlock _block;
    private SpriteRenderer _sr;
    private PolygonCollider2D _poly;
    private bool _isCheckVisible;
    private EllipseVisualizer _ev;
    private Vector3 _oriPos;


    public Action EndAction;

    private void Awake()
    {
        Init();
    }
    public Dictionary<string, Item_Info_Potion> PotionDic;

    public ResourceNodeSaveData Save()
    {
        ResourceNodeSaveData data = new();
        data.Pos = transform.position;
        data.CurrentHp = CurrentHp;
        data.DropAble = DropAble;
        data.DropHpCut = DropHpCut;

        return data;
    }

    public void Load(ResourceNodeSaveData data)
    {
        transform.position = data.Pos;
        CurrentHp = data.CurrentHp;
        DropAble = data.DropAble;
        DropHpCut = data.DropHpCut;
        _oriPos = data.Pos;
    }

    public void Set(ResourceNode_Info_ResourceNode info)
    {
        Info = info;
        DropAble = new bool[Info.Gathering_count];
        Array.Fill(DropAble, true);
        CurrentHp = Info.Hp;
        HpBar.SetActive(false);
        DropHpCut = CurrentHp / (float)Info.Gathering_count;
        _sr.sprite = SpriteAtlas.GetSprite(Info.ResourceNode_sprite);

        if (_sr.sprite != null)
        {
            int shapeCount = _sr.sprite.GetPhysicsShapeCount();
            _poly.pathCount = shapeCount;

            for (int i = 0; i < shapeCount; i++)
            {
                var shape = new List<Vector2>();
                _sr.sprite.GetPhysicsShape(i, shape);
                _poly.SetPath(i, shape.ToArray());
            }
        }
    }


    // 기본값을 Init한다. 최초 1회
    public void Init()
    {
        HpBarFunc = HpBar.GetComponent<FarmHpBarFunc>();
        _sr = GetComponent<SpriteRenderer>();
        _poly = GetComponent<PolygonCollider2D>();
        _ev = GetComponent<EllipseVisualizer>();
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

    public void Shake(float time, float power)
    {
        transform.DOKill();
        transform.position = _oriPos;
        transform.DOShakePosition(time, power);
    }

    public void Damage(float damage)
    {
        if (!Info.isDestructible)
        {
            // 파괴 불가능한 채집물 임으로 이펙트를 따로 표시할 것
            return;
        }

        Shake(0.4f, 0.1f);

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
        (ResourceItemData data, int amount) item = DropTableUtil.GetDropItemFromTable(Info.DT_destroy);

        DropItem dropItem = DropItemPool.Instance.Get(item.data);
        dropItem.Set(item.data, item.amount, transform.position, EllipseVisualizer.GetRandomPos(_ev));
        dropItem.gameObject.SetActive(true);
    }

    public void GetItem(Action farmEnd)
    {
        for (int i = 0; i < Info.Gathering_count; i++)
        {
            if (DropAble[i])
            {
                DropAble[i] = false;

                CurrentHp = Info.Hp - (i + 1) * DropHpCut;
                ShowHpBar();
                if (_hpCloseC != null)
                    StopCoroutine(_hpCloseC);
                _hpCloseC = StartCoroutine(ClosedHpBar(HpBarRemainTime, HpBarCloseTime));

                if (i == Info.Gathering_count - 1)
                {
                    var item = DropTableUtil.GetDropItemFromTable(Info.DT_lastInteraction);
                    UIPopUpHandler.Instance.GetUI<Inventory>().AddItem(item.data, item.amount);
                    farmEnd.Invoke();
                }
                else
                {
                    var item = DropTableUtil.GetDropItemFromTable(Info.DT_interaction);
                    UIPopUpHandler.Instance.GetUI<Inventory>().AddItem(item.data, item.amount);
                }

                break;
            }
            else
                continue;
        }
    }

    public void FarmReset()
    {
        transform.DOKill();
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
        EndAction?.Invoke();
        FarmReset();
    }

    // 시아에 처음 보인 시점
    private void OnEnable()
    {
        // 기본값을 전부 세팅해준다.
        TilePos = new(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        _isCheckVisible = true;
        _oriPos = transform.position;
    }
}

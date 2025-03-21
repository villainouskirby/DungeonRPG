using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public abstract class FarmableBase : MonoBehaviour
{
    public FarmEnum Type { get { return _type; } }
    public PlantEnum PlantType { get { return _plantType; } }
    public MineralEnum MineralType { get { return _mineralType; } }
    protected FarmEnum _type;
    protected PlantEnum _plantType;
    protected MineralEnum _mineralType;

    public int Level;
    public List<ItemData> DropItemList;

    [HideInInspector]
    public Vector2Int TilePos;

    private MaterialPropertyBlock _block;
    private SpriteRenderer _renderer;
    private bool _isCheckVisible;
    private float _strength;

    private void Awake()
    {
        Init();
    }

    private void FixedUpdate()
    {
        if (!_isCheckVisible)
            return;

        _strength = MapManager.Instance.FOVCaster.IsInFOV(TilePos);

        if (0 < _strength)
        {
            _block.SetFloat("_IsDisable", 0f);
            _block.SetFloat("_BlurCorrect", _strength);
            _renderer.SetPropertyBlock(_block);
        }
        else
        {
            Debug.Log(_strength);
            _block.SetFloat("_IsDisable", 1f);
            _block.SetFloat("_BlurCorrect", 1f);
            _renderer.SetPropertyBlock(_block);
        }
    }

    // 기본값을 Init한다. 최초 1회
    public virtual void Init()
    {
        _block = new();
        _renderer = GetComponent<SpriteRenderer>();
    }   

    public void FarmReset()
    {
        _isCheckVisible = false;
        TilePos = new(0, 0);
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
        TilePos = new(Mathf.FloorToInt(transform.position.x / MapManager.Instance.TileSize), Mathf.FloorToInt(transform.position.y / MapManager.Instance.TileSize));
        _block.SetFloat("_IsDisable", 0f);
        _block.SetFloat("_BlurCorrect", 1f);
        _renderer.SetPropertyBlock(_block);
        _isCheckVisible = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

[System.Serializable]
[RequireComponent(typeof(CircleCollider2D))]
public class DropItem : MonoBehaviour
{
    [Header("Effect Settings")]
    public float CurveHeight = 1f;
    public float Duration = 1f;
    public float Point1 = 0.25f;
    public float Point2 = 0.75f;
    public AnimationCurve TCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    public TMP_Text Text;
    private CircleCollider2D _collider;
    private SpriteRenderer _spriteRenderer;
    private ItemData _itemData;
    private int _amount;
    private Vector3 _startPos;
    private Vector3 _endPos;
    public ItemData Data => _itemData;
    public int Amount => _amount;
    private void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetSprite(Sprite sprite) => _spriteRenderer.sprite = sprite;

    public void Set(ItemData itemData, int amount, Vector3 startPos, Vector3 endPos)
    {
        _itemData = itemData;
        _amount = amount;
        _startPos = startPos;
        _endPos = endPos;

        switch (_amount)
        {
            case 0:
                Text.text = "";
                break;
            case 1:
                Text.text = "";
                break;
            default:
                Text.text = $"x{_amount.ToString()}";
                break;
        }
        StartCoroutine(DropEffect());
    }

    public void Get()
    {
        StopAllCoroutines();
        UIPopUpHandler.Instance.InventoryScript.AddItem(_itemData, _amount);
        DropItemPool.Instance.Return(this);
    }

    public void ResetItem()
    {
        _itemData = null;
        _spriteRenderer.sprite = null;
        _amount = 0;
    }

    public IEnumerator DropEffect()
    {
        Vector3 dir = _endPos - _startPos;
        Vector3 perp = Vector3.Cross(dir.normalized, Vector3.forward).normalized;
        if (_endPos.x >= _startPos.x)
            perp = -perp;

        BezierCurve curve = new(
            _startPos,
            _startPos + dir * Point1 + perp * CurveHeight,
            _startPos + dir * Point2 + perp * CurveHeight,
            _endPos);

        float time = 0;

        while (time < Duration)
        {
            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / Duration);
            float tProgress = TCurve.Evaluate(progress);
            transform.position = curve.GetPoint(tProgress);
            yield return null;
        }

        transform.position = _endPos;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            PlayerFarming.Instance.AddDropItem(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            PlayerFarming.Instance.RemoveDropItem(this);
    }
}

public class BezierCurve
{
    private Vector3 p0, p1, p2, p3;

    public BezierCurve(Vector3 start, Vector3 control1, Vector3 control2, Vector3 end)
    {
        p0 = start;
        p1 = control1;
        p2 = control2;
        p3 = end;
    }

    public Vector3 GetPoint(float t)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        return uuu * p0
             + 3f * uu * t * p1
             + 3f * u * tt * p2
             + ttt * p3;
    }
}

using System.Collections.Generic;
using UnityEngine;

public class MonsterStomach : MonoBehaviour
{
    [System.Serializable]
    public struct Entry
    {
        public ItemData item;  // 아이템 SO
        public int amount;
    }

    [Header("Optional: 던질 궤적")]
    [SerializeField] private EllipseVisualizer ellipse;     // 없으면 자동으로 GetComponent 시도
    [SerializeField] private float fallbackRadius = 1.2f;   // EV 없을 때 반경

    private readonly Stack<Entry> _stack = new();

    void Awake()
    {
        if (!ellipse) ellipse = GetComponent<EllipseVisualizer>();
    }

    public bool HasItems => _stack.Count > 0;
    public int Count => _stack.Count;

    public void Swallow(ItemData item, int amount = 1)
    {
        //if (item = null) return;
        _stack.Push(new Entry { item = item, amount = Mathf.Max(1, amount) });
    }

    public void VomitAll()
    {
        while (_stack.Count > 0)
            VomitOneInternal(_stack.Pop());
    }

    public void VomitOne()
    {
        if (_stack.Count > 0)
            VomitOneInternal(_stack.Pop());
    }

    private void VomitOneInternal(Entry e)
    {
        Vector3 start = transform.position;
        Vector3 end = ellipse
            ? EllipseVisualizer.GetRandomPos(ellipse)
            : start + (Vector3)(Random.insideUnitCircle.normalized * fallbackRadius);

        var drop = DropItemPool.Instance.Get(e.item);
        drop.Set(e.item, e.amount, start, end);
        drop.gameObject.SetActive(true);
    }

    public void Clear() => _stack.Clear();
}
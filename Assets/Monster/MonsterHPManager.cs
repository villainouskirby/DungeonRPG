using System.Collections.Generic;
using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    public static HealthBarManager Instance { get; private set; }

    [SerializeField] private RectTransform canvasRoot; // Screen‑Space‑Camera Canvas
    [SerializeField] private HealthBarUI prefab;

    private readonly Dictionary<MonsterBase1, HealthBarUI> map = new();

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Register(MonsterBase1 monster)
    {
        if (map.ContainsKey(monster)) return;

        HealthBarUI ui = Instantiate(prefab, canvasRoot);
        ui.Init(monster.transform, Vector3.up * 0.8f); // 머리 위 0.8 unit
        map.Add(monster, ui);
    }

    public void UpdateBar(MonsterBase1 m, float ratio)
    {
        if (map.TryGetValue(m, out var ui))
            ui.SetRatio(ratio);
    }

    public void Unregister(MonsterBase1 m)
    {
        if (!map.TryGetValue(m, out var ui)) return;
        Destroy(ui.gameObject);
        map.Remove(m);
    }
}
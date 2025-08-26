using System.Collections.Generic;
using UnityEngine;

public sealed class ThrowNoiseManager : MonoBehaviour
{
    public static ThrowNoiseManager Instance { get; private set; }

    class Noise
    {
        public Vector3 pos;
        public float radius;
        public float expireAt;
    }

    readonly List<Noise> _noises = new();

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        float now = Time.time;
        for (int i = _noises.Count - 1; i >= 0; i--)
            if (_noises[i].expireAt <= now) _noises.RemoveAt(i);
    }

    /// <summary> 소리 등록: pos에서 radius만큼, ttl초 동안 유효 </summary>
    public void RegisterNoise(Vector3 pos, float radius, float ttl = 3f)
    {
        _noises.Add(new Noise { pos = pos, radius = Mathf.Max(0f, radius), expireAt = Time.time + Mathf.Max(0.1f, ttl) });
    }

    /// <summary>
    /// 청자(from) 기준으로 maxDetectDist 이내에 존재하는 소리 중 가장 가까운 하나를 반환.
    /// 소리 반경(radius)까지 포함해 “도달 가능한 목적지”로 간주.
    /// </summary>
    public bool TryGetNearestNoise(Vector3 from, float maxDetectDist, out Vector3 noisePos)
    {
        noisePos = default;
        if (_noises.Count == 0) return false;

        float best = float.PositiveInfinity;
        bool found = false;

        foreach (var n in _noises)
        {
            float d = Vector3.Distance(from, n.pos);
            // Detect 범위 + 소리 반경을 합쳐 “유효 범위”로 판단
            if (d <= maxDetectDist + n.radius && d < best)
            {
                best = d;
                noisePos = n.pos;
                found = true;
            }
        }
        return found;
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DropResolved
{
    public string itemId;
    public int count;
}

[Serializable]
public struct DropEntry
{
    public string itemId;
    public int chance;     // 0~100 (%). 합이 100을 넘어도 '독립 판정'이므로 OK
    public int minCount;   // 포함
    public int maxCount;   // 포함
}

public static class DropTableResolver
{
    /// <summary>
    /// "ID/확률/최소/최대~ID/확률/최소/최대" 형식을 엔트리 리스트로 파싱
    /// 빈 칸/잘못된 형식은 스킵
    /// </summary>
    public static List<DropEntry> Parse(string dropTableInfo)
    {
        var result = new List<DropEntry>();
        if (string.IsNullOrWhiteSpace(dropTableInfo)) return result;

        string[] entries = dropTableInfo.Split('~', StringSplitOptions.RemoveEmptyEntries);
        foreach (var e in entries)
        {
            var parts = e.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
            {
                Debug.LogWarning($"[DropTableResolver] malformed entry: '{e}'");
                continue;
            }

            var entry = new DropEntry
            {
                itemId = parts[0].Trim(),
                chance = ToInt(parts[1]),
                minCount = ToInt(parts[2]),
                maxCount = ToInt(parts[3]),
            };

            if (entry.maxCount < entry.minCount)
                (entry.minCount, entry.maxCount) = (entry.maxCount, entry.minCount);

            result.Add(entry);
        }
        return result;
    }

    /// <summary>
    /// 각 엔트리를 독립 확률로 판정하여 실제 드랍 결과를 생성
    /// </summary>
    public static List<DropResolved> RollDrops(string dropTableInfo, System.Random rng = null)
    {
        var parsed = Parse(dropTableInfo);
        var outList = new List<DropResolved>();
        if (parsed.Count == 0) return outList;

        // UnityEngine.Random 사용 시: rng == null
        foreach (var e in parsed)
        {
            bool success = RollPercent(e.chance, rng);
            if (!success) continue;

            int count = RangeInclusive(e.minCount, e.maxCount, rng);
            if (count <= 0) continue;

            outList.Add(new DropResolved { itemId = e.itemId, count = count });
        }
        return outList;
    }

    static int ToInt(string s) => int.TryParse(s?.Trim(), out var v) ? v : 0;

    static bool RollPercent(int percent, System.Random rng)
    {
        percent = Mathf.Clamp(percent, 0, 100);
        if (rng != null)
            return rng.NextDouble() * 100.0 < percent;
        else
            return UnityEngine.Random.value * 100f < percent;
    }

    static int RangeInclusive(int min, int max, System.Random rng)
    {
        if (min > max) (min, max) = (max, min);
        if (rng != null)
            return rng.Next(min, max + 1); // upper bound exclusive
        else
            return UnityEngine.Random.Range(min, max + 1); // upper bound inclusive용 +1
    }
}

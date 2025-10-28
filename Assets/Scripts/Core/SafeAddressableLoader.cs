using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public static class SafeAddressableLoader
{
    // 키 검증 결과 캐싱
    private static readonly Dictionary<string, bool> _checkedKeys = new();

    /// <summary>
    /// 비동기 로드
    /// </summary>
    public static async UniTask<T> LoadAsync<T>(string address) where T : UnityEngine.Object
    {
        if (!await CheckKeyAsync(address)) return null;

        try
        {
            var handle = Addressables.LoadAssetAsync<T>(address);
            var asset = await handle.ToUniTask();

            return asset;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SafeAddressable] 비동기 로드 실패: {address}\n{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 동기 로드
    /// </summary>
    public static T LoadSync<T>(string address) where T : UnityEngine.Object
    {
        if (!CheckKeySync(address)) return null;

        try
        {
            var handle = Addressables.LoadAssetAsync<T>(address);
            var asset = handle.WaitForCompletion();

            return asset;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SafeAddressable] 동기 로드 실패: {address}\n{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 비동기 키 검사
    /// </summary>
    private static async UniTask<bool> CheckKeyAsync(string address)
    {
        if (_checkedKeys.TryGetValue(address, out var exists)) return exists;

        try
        {
            var handle = Addressables.LoadResourceLocationsAsync(address);
            var locs = await handle.ToUniTask();
            exists = locs != null && locs.Count > 0;
        }
        catch
        {
            exists = false;
        }

        _checkedKeys[address] = exists;

        if (!exists)
        {
            Debug.LogWarning($"[SafeAddressable] 존재하지 않는 Addressable 키: {address}");
        }

        return exists;
    }

    /// <summary>
    /// 동기 키 검사
    /// </summary>
    private static bool CheckKeySync(string address)
    {
        if (_checkedKeys.TryGetValue(address, out var exists)) return exists;

        try
        {
            var handle = Addressables.LoadResourceLocationsAsync(address);
            var locs = handle.WaitForCompletion();
            exists = locs != null && locs.Count > 0;
        }
        catch
        {
            exists = false;
        }

        _checkedKeys[address] = exists;

        if (!exists)
        {
            Debug.LogWarning($"[SafeAddressable] 존재하지 않는 Addressable 키: {address}");
        }

        return exists;
    }
}
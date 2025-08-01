using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public static class OptionManager
{
    public static Option Option;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>("OptionPrefab");

        handle.WaitForCompletion();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject obj = handle.Result;
            GameObject.Instantiate(obj);
            GameObject.DontDestroyOnLoad(obj);

            Option = obj.GetComponent<Option>();
        }
    }
}

public class OptionData
{
    public ScreenSize ScreenSize;
    public ScreenMode ScreenMode;
    public AudioSourceData BGM;
    public AudioSourceData SFX;
    public InteractionType InteractionType;
    public Dictionary<KeyMapping, KeyCode> KeyMapping;
    public float ShakePower;
    //Etc
}

public class AudioSourceData
{
    public float Value;
    public bool Mute;
}
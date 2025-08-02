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
            GameObject objPrefab = handle.Result;
            GameObject obj = GameObject.Instantiate(objPrefab);
            GameObject.DontDestroyOnLoad(obj);

            Option = obj.GetComponent<Option>();

            obj.SetActive(false);
        }
    }

    public static void Open()
    {
        Option.gameObject.SetActive(true);
    }

    public static void Close()
    {
        Option.gameObject.SetActive(false);
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
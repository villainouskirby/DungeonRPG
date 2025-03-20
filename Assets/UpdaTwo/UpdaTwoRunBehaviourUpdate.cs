using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using static UnityEngine.PlayerLoop.Update;

public static class UpdaTwoRunBehaviourUpdate
{
    private static readonly HashSet<MonoBehaviour> _updateObjects = new();
    private static readonly Dictionary<Type, Action<MonoBehaviour>[]> _cachedUpdateMethods = new();

    [RuntimeInitializeOnLoadMethod]
    private static void Init()
    {
        RegisterSceneMonoBehaviours();

        var defaultSystems = PlayerLoop.GetDefaultPlayerLoop();
        var myUpdateSystem = new PlayerLoopSystem
        {
            subSystemList = null,
            updateDelegate = OnUpdate,
            type = typeof(UpdaTwoRunBehaviourUpdate)
        };

        var loopWithUpdate = AddSystemAfter<Update, ScriptRunBehaviourUpdate>(in defaultSystems, myUpdateSystem);
        PlayerLoop.SetPlayerLoop(loopWithUpdate);

        Application.quitting += ClearAll; // 게임 종료 시 정리
    }

    private static void OnUpdate()
    {
        for (int i = 0; i < 3; i++) // 0: Update1, 1: Update2, 2: Update3
        {
            foreach (var obj in _updateObjects)
            {
                if (obj == null) continue;

                if (_cachedUpdateMethods.TryGetValue(obj.GetType(), out var updateActions))
                {
                    updateActions[i]?.Invoke(obj);
                }
            }
        }
    }

    /// <summary>
    /// 현재 씬에서 모든 MonoBehaviour 등록
    /// </summary>
    private static void RegisterSceneMonoBehaviours()
    {
        foreach (var mono in UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true))
        {
            RegisterMonoBehaviour(mono);
        }
    }

    /// <summary>
    /// MonoBehaviour를 등록하고 자동 감지
    /// </summary>
    public static void RegisterMonoBehaviour(MonoBehaviour obj)
    {
        if (obj == null || _updateObjects.Contains(obj)) return;

        _updateObjects.Add(obj);
        CacheUpdateMethods(obj.GetType());

        // 자동 감지 컴포넌트 추가
        if (obj.gameObject.GetComponent<MonoLifecycleDetector>() == null)
        {
            obj.gameObject.AddComponent<MonoLifecycleDetector>();
        }
    }

    /// <summary>
    /// MonoBehaviour를 해제
    /// </summary>
    public static void UnregisterMonoBehaviour(MonoBehaviour obj)
    {
        _updateObjects.Remove(obj);
    }

    /// <summary>
    /// Update1, Update2, Update3 메서드를 캐싱하여 성능 향상
    /// </summary>
    private static void CacheUpdateMethods(Type type)
    {
        if (_cachedUpdateMethods.ContainsKey(type)) return;

        Action<MonoBehaviour>[] methodArray = new Action<MonoBehaviour>[3];
        string[] methodNames = { "Update1", "Update2", "Update3" };

        for (int i = 0; i < methodNames.Length; i++)
        {
            MethodInfo method = type.GetMethod(methodNames[i], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method != null)
            {
                methodArray[i] = (obj) => method.Invoke(obj, null);
            }
        }

        _cachedUpdateMethods[type] = methodArray;
    }

    private static PlayerLoopSystem AddSystemAfter<TParent, TTarget>(in PlayerLoopSystem loopSystem, PlayerLoopSystem systemToAdd)
    where TParent : struct
    where TTarget : struct
    {
        for (int i = 0; i < loopSystem.subSystemList.Length; i++)
        {
            var parentLoopSystem = loopSystem.subSystemList[i];
            if (parentLoopSystem.type == typeof(TParent))
            {
                PlayerLoopSystem newPlayerLoop = new()
                {
                    loopConditionFunction = parentLoopSystem.loopConditionFunction,
                    type = parentLoopSystem.type,
                    updateDelegate = parentLoopSystem.updateDelegate,
                    updateFunction = parentLoopSystem.updateFunction
                };

                List<PlayerLoopSystem> newSubSystemList = new(parentLoopSystem.subSystemList);
                for (int j = 0; j < newSubSystemList.Count; j++)
                {
                    if (newSubSystemList[j].type == typeof(TTarget))
                    {
                        newSubSystemList.Insert(j + 1, systemToAdd);
                        break;
                    }
                }

                newPlayerLoop.subSystemList = newSubSystemList.ToArray();
                loopSystem.subSystemList[i] = newPlayerLoop;
            }
        }
        return loopSystem;
    }

    private static void ClearAll()
    {
        _updateObjects.Clear();
        _cachedUpdateMethods.Clear();
    }
}

/// <summary>
/// 자동으로 MonoBehaviour의 생성/삭제를 감지하는 컴포넌트 (숨김 처리됨)
/// </summary>
public class MonoLifecycleDetector : MonoBehaviour
{
    private void Awake()
    {
        // 현재 컴포넌트만 Inspector에서 숨김 (GameObject는 유지)
        this.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave;
    }

    private void OnEnable()
    {
        UpdaTwoRunBehaviourUpdate.RegisterMonoBehaviour(this);
    }

    private void OnDisable()
    {
        UpdaTwoRunBehaviourUpdate.UnregisterMonoBehaviour(this);
    }
}

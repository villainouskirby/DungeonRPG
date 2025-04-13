using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using static UnityEngine.PlayerLoop.Update;


public static class TimeSystemUpdate
{
    public static float        Time;
    public static int          Day { get { return (int)(Time * InGameTimeScale / 86400); } }
    public static bool         IsPlaying = true;
    public static float        TimeScale = 1;
    public static float        InGameTimeScale = 60;

    public static void Stop()
    {
        TimeScale = 0;
    }

    public static void Start()
    {
        TimeScale = 1;
    }

    [RuntimeInitializeOnLoadMethod]
    private static void Init()
    {
        var defaultSystems = PlayerLoop.GetDefaultPlayerLoop();
        var myUpdateSystem = new PlayerLoopSystem
        {
            subSystemList = null,
            updateDelegate = OnUpdate,
            type = typeof(TimeSystemUpdate)
        };

        var loopWithUpdate = AddSystemAfter<Update, ScriptRunBehaviourUpdate>(in defaultSystems, myUpdateSystem);
        PlayerLoop.SetPlayerLoop(loopWithUpdate);
    }

    private static void OnUpdate()
    {
        Time += TimeScale * UnityEngine.Time.deltaTime;
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
}

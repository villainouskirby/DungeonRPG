using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    static GameManager s_inst;
    public static GameManager Inst
    {
        get
        {
            if (s_inst == null)
            {
                s_inst = new GameManager();
            }
            return s_inst;
        }
    }

    InputManager _input = new InputManager();
    SoundManager _sound = new SoundManager();
    SaveLoadManager _saveLoad = new SaveLoadManager();


    public static InputManager Input { get { return Inst._input; } }

    public static SoundManager Sound { get { return Inst._sound; } }
    public static SaveLoadManager SaveLoad { get { return Inst._saveLoad; } }

    private void Awake()
    {
        Init();
    }
    static void Init()
    {
        if (s_inst == null)
        {
            GameObject go = GameObject.Find("@GameManager");
            if (go == null)
            {
                go = new GameObject { name = "@GameManager" };
                go.AddComponent<GameManager>();
            }

            DontDestroyOnLoad(go);
            s_inst = go.GetComponent<GameManager>();

        }
    }

    private void Update()
    {
        _input.OnUpdate();
    }
}
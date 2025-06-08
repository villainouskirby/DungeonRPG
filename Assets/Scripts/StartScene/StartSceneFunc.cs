using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneFunc : MonoBehaviour
{
    public GameObject OptionUI;

    public void StartGame()
    {
        SceneManager.LoadScene("testDohyun");
    }

    public void OpenOption()
    {
        OptionUI.SetActive(true);
    }

    public void CloseOption()
    {
        OptionUI.SetActive(false);
    }

    public void GameExit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

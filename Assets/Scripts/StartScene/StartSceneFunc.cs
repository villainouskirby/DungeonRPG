using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartSceneFunc : MonoBehaviour
{
    public GameObject ContinueCheckUI;
    public SaveFileManager SaveFileManager;
    public Button ContinueBtn;
    public Color BtnDisableColor;

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        var auto1SlotData = SaveManager.SaveFileData.ManualSaveSlot[(int)SaveSlotIndex.Auto1];
        if (auto1SlotData.Exist)
        {
            ContinueBtn.interactable = true;
            ContinueBtn.transform.GetChild(0).GetComponent<TMP_Text>().color = Color.black;
        }
        else
        {
            ContinueBtn.interactable = false;
            ContinueBtn.transform.GetChild(0).GetComponent<TMP_Text>().color = BtnDisableColor;
        }
    }

    public void ContinueGame()
    {
        SaveManager.SaveSlotIndex = SaveSlotIndex.Auto1;
        StartGame();
    }

    public void OpenSaveSlotUI()
    {
        SaveFileManager.Open();
    }

    public void NewGame()
    {
        if (SaveManager.SaveFileData.ManualSaveSlot[(int)SaveSlotIndex.Auto1].Exist)
        {
            ContinueCheckUI.SetActive(true);
        }
        else
        {
            StartNewGame();
        }
    }

    public void StartNewGame()
    {
        SaveManager.NewSlot(SaveSlotIndex.Auto1, "Auto1");
        SaveManager.SaveSlotIndex = SaveSlotIndex.Auto1;
        StartGame();
    }

    public void StartGame()
    {
        TileMapMaster.IsLoad = false;
        SceneManager.LoadScene("testDohyun");
    }
    
    public void OpenOption()
    {
        OptionManager.Open();
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

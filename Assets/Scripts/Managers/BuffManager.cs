using Cainos.PixelArtTopDown_Basic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffManager : MonoBehaviour
{
    public static BuffManager instance;
    private void Awake()
    {
        instance = this;
    }
    public GameObject buffPrefab;

    public void CreateBuff(string type, float per, float du, Sprite icon)
    {
        GameObject go = Instantiate(buffPrefab, transform);
        go.GetComponent<BaseBuff>().Init(type, per, du);
        go.GetComponent<Image>().sprite = icon;
    }
}

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
        // 여기 부분은 Instantiate 보단 오브젝트 풀링 사용하시면 좋을거같아요
        // 나중에 버프 좀 많아지면 살짝 문제 생길수도있어서
        GameObject go = Instantiate(buffPrefab, transform);
        go.GetComponent<BaseBuff>().Init(type, per, du);
        go.GetComponent<Image>().sprite = icon;
    }
}

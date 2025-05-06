using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


public class PotionManager : MonoBehaviour
{
    public static PotionManager instance;

    [Header("버프 아이콘 프리팹")]
    public GameObject buffPrefab;

    private void Awake()
    {
        instance = this;
    }

    private bool isDrinking = false;

    public async UniTask<bool> GetPotionID(ItemData data)
    {
        if (isDrinking) return false;
        isDrinking = true;

        PotionItemData piData = data as PotionItemData;
        
        if (!await Drink()) return false;

        if (piData.ID <= 10)
        {
            CreateBuff(piData.ID, piData.Percentage, piData.Duration, piData.IconSprite);
        }
        else if (piData.ID <= 20) 
        {
            PlayerData.instance.HPValueChange(piData.Healamount);
        }

        isDrinking = false;

        return true;
    }

    private async UniTask<bool> Drink()
    {
        float startTime = Time.time;

        while (Time.time - startTime < 2)
        {
            // 피격감지 => 피격시 false 반환

            await UniTask.NextFrame();
        }

        return true;
    }

    /// <summary>
    /// 버프 아이콘 생성
    /// </summary>
    public void CreateBuff(int buffID, float percentage, float duration, Sprite icon)
    {
        BuffType type = buffID switch
        {
            1 => BuffType.AttackUp,
            2 => BuffType.AttackDown,
            _ => BuffType.AttackUp
        };
        GameObject go = Instantiate(buffPrefab, transform);
        go.GetComponent<Image>().sprite = icon;
        BaseBuff buffImage = go.GetComponent<BaseBuff>();
        buffImage.Init(type, percentage, duration);
    }
}


/*
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
*/
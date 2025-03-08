using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseBuff : MonoBehaviour
{
    //버프 아이콘 인스펙터에 붙일 것
    public string type;
    public float percentage;
    public float duration;
    public float currenttime;
    public Image icon;
    public void Awake()
    {
        icon = GetComponent<Image>();
    }
    public void Init(string type, float per, float du)
    {
        this.type = type;
        percentage = per;
        duration = du;
        currenttime = duration;
        icon.fillAmount = 1; // 버프 표기 방식에 따라 바꿀 수 있음
        Execute();
    }
    public void Execute()
    {
        PlayerData.instance.onBuff.Add(this);
        PlayerData.instance.ChooseBuff(type);
        StartCoroutine(Activation());
    }

    IEnumerator Activation()
    {
        while (currenttime > 0)
        {
            currenttime -= 0.1f;
            icon.fillAmount = currenttime/duration;
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
        icon.fillAmount = 0;
        currenttime = 0;
        DeActivation();
    }
    public void DeActivation()
    {
        PlayerData.instance.onBuff.Remove(this);
        PlayerData.instance.ChooseBuff(type);
        Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

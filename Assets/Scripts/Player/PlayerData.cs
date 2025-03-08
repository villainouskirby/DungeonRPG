using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;
    private void Awake()
    {
        instance = this;
    }
    public Player player;
    public float originalAtk = 100;
    public float originalSpeed = 5;
    void Start()
    {
        player.Atk = originalAtk;
        player.Speed = originalSpeed;
    }

    public List<BaseBuff> onBuff = new List<BaseBuff>();

    public float BuffChange(string type, float origin)
    {
        if (onBuff.Count > 0)
        {
            float temp = 0;
            for (int i = 0; i < onBuff.Count; i++) 
            {
                if (onBuff[i].type.Equals(type))
                    temp += origin * onBuff[i].percentage;
            }
            return origin + temp;
        }
        else
        {
            return origin;
        }
    }
    public void ChooseBuff(string type) 
    { 
        switch(type)
        {
            case "Atk":
                player.Atk = BuffChange(type, originalAtk); break;
            case "Speed":
                player.Speed = BuffChange(type, originalSpeed); break;
        }
    }
}

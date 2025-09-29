using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stair : MonoBehaviour
{
    public int StartHeight;
    public int EndHeight;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag != "Player")
            return;
        TileMapMaster.Instance.Player.tag = "Stair";
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag != "Player")
            return;
        TileMapMaster.Instance.Player.tag = "Player";
    }
}

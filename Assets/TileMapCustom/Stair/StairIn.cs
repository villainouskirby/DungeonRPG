using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairIn : MonoBehaviour
{
    public Stair Stair;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag != "Player")
            return;
        if (!Stair.IsEntry)
            return;
        Stair.IsIn = true;
        HeightManager.Instance.AutoHeight = false;
        TileMapMaster.Instance.Player.GetComponent<PlayerController>().StartMoveCorrect(Stair.MoveCorrect, Stair.Speed, Stair.StairType);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag != "Player")
            return;
        Stair.IsIn = false;
        HeightManager.Instance.AutoHeight = true;
        TileMapMaster.Instance.Player.GetComponent<PlayerController>().EndMoveCorrect();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairEntry : MonoBehaviour
{
    public Stair Stair;
    public int Index;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag != "Player")
            return;
        float playerHeight = HeightManager.Instance.PlayerHeight;
        switch(Index)
        {
            case 1:
                if (Mathf.Abs(playerHeight - Stair.Entry1Height) > 0.5f)
                    return;
                break;
            case 2:
                if (Mathf.Abs(playerHeight - Stair.Entry2Height) > 0.5f)
                    return;
                break;
        }

        Stair.IsEntry = true;
        Stair.StairOutLine1.gameObject.SetActive(true);
        Stair.StairOutLine2.gameObject.SetActive(true);
        TileMapMaster.Instance.Player.layer = LayerMask.NameToLayer("Stair");
    }

private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag != "Player")
            return;
        if (!Stair.IsIn)
        {
            Stair.IsEntry = false;
            Stair.StairOutLine1.gameObject.SetActive(false);
            Stair.StairOutLine2.gameObject.SetActive(false);
            TileMapMaster.Instance.Player.layer = LayerMask.NameToLayer("Player");
        }
    }
}

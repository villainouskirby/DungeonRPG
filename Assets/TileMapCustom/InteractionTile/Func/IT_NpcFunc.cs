using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class IT_NpcFunc : MonoBehaviour
{
    private BoxCollider2D _collider;
    private NPC _npcName;
    private bool _isActive;
    public SpriteRenderer SR;

    public void Init(IT_NpcObj npcObj)
    {
        _collider = GetComponent<BoxCollider2D>();
        _npcName = npcObj.NpcName;
        _isActive = false;
        SR.sortingLayerName = npcObj.LayerName;
        SR.sortingOrder = npcObj.LayerIndex;

        switch (_npcName) // 대충 UI Open하는 코드 추가 작성
        {
            case NPC.A:
                gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                break;
            case NPC.B:
                gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
                break;
            case NPC.C:
                gameObject.GetComponent<SpriteRenderer>().color = Color.green;
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            _isActive = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            _isActive = false;
        }
    }


    // 임시 코드 Interaction 전용 관리 Manager 하나 만들어야할듯
    public void Update()
    {
        if (!_isActive)
            return;

        if (Input.GetKeyDown(KeyCode.O))
        {
            switch (_npcName) // 대충 UI Open하는 코드 추가 작성
            {
                case NPC.A:
                    UIPopUpHandler.Instance.OpenSmith();
                    break;
                case NPC.B:
                    UIPopUpHandler.Instance.OpenShop();
                    break;
                case NPC.C:
                    UIPopUpHandler.Instance.OpenQuest();
                    break;
            }
        }
    }
}

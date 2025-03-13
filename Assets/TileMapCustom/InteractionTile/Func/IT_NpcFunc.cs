using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class IT_NpcFunc : MonoBehaviour
{
    private BoxCollider2D _collider;
    private string _npcName;
    private bool _isActive;

    public void Init(IT_NpcObj npcObj)
    {
        _collider = GetComponent<BoxCollider2D>();
        _npcName = npcObj.NpcName;
        _isActive = false;
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

        if (Input.GetKeyDown(KeyCode.E))
        {
            switch (_npcName) // 대충 UI Open하는 코드 추가 작성
            {
                case "A":
                    
                    break;
                case "B":

                    break;
                case "C":

                    break;
            }
        }
    }
}

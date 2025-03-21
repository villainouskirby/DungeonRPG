using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class IT_EntryFunc : MonoBehaviour
{
    private BoxCollider2D _collider;
    private MapEnum _mapType;
    private bool _isActive;

    public void Init(IT_EntryObj entryObj)
    {
        _collider = GetComponent<BoxCollider2D>();
        _mapType = entryObj.MapType;
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
            TileMapMaster.Instance.StartTileMap(_mapType);
    }
}

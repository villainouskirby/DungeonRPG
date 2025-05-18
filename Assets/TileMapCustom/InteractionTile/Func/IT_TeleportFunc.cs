using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class IT_TeleportFunc : MonoBehaviour
{
    private BoxCollider2D _collider;
    private Vector3 _targetPos;
    private bool _isActive;
    private GameObject _player;
    public SpriteRenderer SR;

    public void Init(IT_TeleportObj teleportObj)
    {
        _collider = GetComponent<BoxCollider2D>();
        _targetPos = teleportObj.TargetPos;
        _isActive = false;
        SR.sortingLayerName = teleportObj.LayerName;
        SR.sortingOrder = teleportObj.LayerIndex;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            _isActive = true;
            _player = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            _isActive = false;
            _player = null;
        }
    }


    public void Update()
    {
        if (!_isActive)
            return;

        if (Input.GetKeyDown(KeyCode.O))
        {
            _player.transform.position = _targetPos;
        }
    }
}

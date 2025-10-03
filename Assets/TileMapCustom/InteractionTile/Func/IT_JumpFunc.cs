using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class IT_JumpFunc : MonoBehaviour
{
    private BoxCollider2D _collider;
    private int _targetLayer;
    private int _targetGround;
    private int _targetHeight;
    private bool _isActive;
    private GameObject _player;
    public SpriteRenderer SR;

    public void Init(IT_JumpObj jumpObj)
    {
        _collider = GetComponent<BoxCollider2D>();
        _targetLayer = jumpObj.TargetLayer;
        _targetGround = jumpObj.TargetGround;
        _targetHeight = jumpObj.TargetHeight;
        _isActive = false;
        SR.sortingLayerName = jumpObj.LayerName;
        SR.sortingOrder = jumpObj.LayerIndex;
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
            _player.GetComponent<PlayerController>().StartDrop(_targetLayer, _targetGround, _targetHeight);
        }
    }
}

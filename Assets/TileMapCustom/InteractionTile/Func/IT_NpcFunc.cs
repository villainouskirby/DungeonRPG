using System.Collections;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class IT_NpcFunc : MonoBehaviour
{
    public NPC NpcName => _npcName;

    private BoxCollider2D _collider;
    private NPC _npcName;
    private bool _isActive;
    public SpriteRenderer SR;

    [SerializeField] private Sprite _merchantSprite;
    [SerializeField] private Sprite _blacksmithSprite;
    [SerializeField] private Sprite _priestSprite;

    public void Init(IT_NpcObj npcObj)
    {
        _collider = GetComponent<BoxCollider2D>();
        _collider.size = npcObj.ColliderBounds;
        _collider.offset = npcObj.ColliderOffset;
        _npcName = npcObj.NpcName;
        _isActive = false;
        SR.sortingLayerName = npcObj.LayerName;
        SR.sortingOrder = npcObj.LayerIndex;

        switch (_npcName)
        {
            case NPC.merchant:
                SR.sprite = _merchantSprite;
                break;

            case NPC.smith:
                SR.sprite = _blacksmithSprite;
                break;

            case NPC.priest:
                SR.sprite = _priestSprite;
                break;

            default:
                SR.sprite = _merchantSprite;
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            _isActive = true;
            UIPopUpHandler.Instance.GetScript<InteractUI>().OpenInteractPopUp(InteractUI.InteractType.F, transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            _isActive = false;
            UIPopUpHandler.Instance.GetScript<InteractUI>().CloseInteractPopUp();
        }
    }


    // 임시 코드 Interaction 전용 관리 Manager 하나 만들어야할듯
    public void Update()
    {
        if (!_isActive)
            return;

        if (PotionManager.Instance != null && PotionManager.Instance.IsDrinking)
            return;

        if (!UIPopUpHandler.Instance.IsUIOpen && Input.GetKeyDown(KeyCode.F))
        {
            switch (_npcName) // 대충 UI Open하는 코드 추가 작성
            {
                case NPC.smith:
                    UIPopUpHandler.Instance.GetScript<Smith>().StartTalk();
                    break;
                case NPC.merchant:
                    UIPopUpHandler.Instance.GetScript<Shop>().StartTalk();
                    break;
                case NPC.C:
                    UIPopUpHandler.Instance.GetScript<Quest>();
                    break;
                case NPC.priest:
                    UIPopUpHandler.Instance.GetScript<Priest>().StartTalk();
                    break;
            }
        }
    }
}

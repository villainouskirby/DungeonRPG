using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIFocus : UIBase
{
    [SerializeField] private MaskRaycast _maskRaycast;

    private Queue<RectTransform> _focusQueue = new();
    private Dictionary<string, RectTransform> _focusTargetDict = new();

    public bool IsFocusing { get; private set; } = false;

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
        gameObject.SetActive(false);
    }

    public void Register(string key, RectTransform target)
    {
        _focusTargetDict[key] = target;
    }

    public void Unregister(string key)
    {
        _focusTargetDict.Remove(key);
    }

    public void EnqueueFocusEvent(string key, int type = 0)
    {
        RectTransform target;

        if (type == 0)
        {
            _focusTargetDict.TryGetValue(key, out target);
        }
        else
        {
            var inventory = UIPopUpHandler.Instance.GetScript<Inventory>();
            var slot = inventory.GetItemSlotUI(key);

            if (slot == null) return;

            target = slot.GetComponent<RectTransform>();
            
            if (inventory.GetTabType() != InventoryUI.TabType.All)
            {
                EnqueueFocusEvent("AllSlot");
            }
        }

        if (target != null)
        {
            _focusQueue.Enqueue(target);
        }

        StartFocus().Forget();
    }

    public async UniTaskVoid StartFocus()
    {
        if (IsFocusing) return;

        IsFocusing = true;

        while (_focusQueue.Count > 0)
        {
            RectTransform rectTransform = _focusQueue.Peek();
            GameObject go = rectTransform.gameObject;
            bool isClicked = false;

            KeyCode keyCode = KeyCode.None;

            if (rectTransform.TryGetComponent(out KeyGuideUI comp))
            {
                switch (comp.CurrentType)
                {
                    case KeyGuideUI.GuideType.Bag:
                    case KeyGuideUI.GuideType.Tab:
                        keyCode = KeyCode.Tab;
                        break;

                    case KeyGuideUI.GuideType.Quest:
                        keyCode = KeyCode.Alpha1;
                        break;
                }
            }
            else if (rectTransform.TryGetComponent(out InventoryItemSlotUI slot))
            {
                rectTransform = rectTransform.GetChild(0).GetComponent<RectTransform>();
                isClicked = true; // 내부 interactHandler에서 클릭 여부 걸러짐
            }
            else
            {
                rectTransform.GetComponent<FocusTarget>().OnClicked += () => isClicked = true;
            }

            Rect rect = new Rect();
            rect.size = rectTransform.sizeDelta;

            await UniTask.NextFrame();

            rect.position = rectTransform.TransformPoint(rectTransform.rect.center);

            _maskRaycast.SetRaycastRect(rect);
            gameObject.SetActive(true);

            while (true)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    await UniTask.NextFrame();

                    if (isClicked && EventSystem.current.currentSelectedGameObject == go)
                    {
                        break;
                    }
                }

                if (keyCode != KeyCode.None && Input.GetKeyDown(keyCode))
                {
                    switch (keyCode)
                    {
                        case KeyCode.Tab:
                            UIPopUpHandler.Instance.OpenUI<Inventory>();
                            break;

                        case KeyCode.Alpha1:
                            UIPopUpHandler.Instance.OpenUI<Quest>();
                            break;
                    }

                    break;
                }

                await UniTask.Yield();
            }

            _focusQueue.Dequeue();
            gameObject.SetActive(false);
        }

        IsFocusing = false;
    }
}

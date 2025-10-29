using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIFocus : UIBase
{
    public struct FocusInfo
    {
        public RectTransform Rect;
        public int Type;
        public string Text;
        public Vector2 TextPos;
    }

    [SerializeField] private GameObject _clickBlocker;
    [SerializeField] private MaskRaycast _maskRaycast;
    [SerializeField] private RectTransform _textBGRect;
    [SerializeField] private RectTransform _textRect;
    [SerializeField] private RectTransform _highlightRect;
    [SerializeField] private TextMeshProUGUI _tmp;
    [SerializeField] private float _shadeAlpha = 0.5f;

    [Header("Anim Time")]
    [SerializeField] private float _shadowDuration = 0.5f;
    [SerializeField] private float _highlightFadeDuration = 0.1f;
    [SerializeField] private float _highlightScaleDuration = 0.3f;

    private Queue<FocusInfo> _focusQueue = new();
    private Dictionary<string, RectTransform> _focusTargetDict = new();

    private MaskableGraphic _shadowImage;
    private Image _highlightImage;

    public bool IsFocusing { get; private set; } = false;

    protected override void InitBase()
    {
        _shadowImage = GetComponent<MaskableGraphic>();
        _highlightImage = _highlightRect.GetComponent<Image>();

        Material newMaterial = Instantiate(_highlightImage.material);
        _highlightImage.material = newMaterial;

        UIPopUpHandler.Instance.RegisterUI(this);
        _highlightRect.gameObject.SetActive(false);
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

    public void EnqueueFocusEvent(int type, string focusID, string tutorialText, Vector2 textPos)
    {
        RectTransform target;

        if (type == 1)
        {
            var inventory = UIPopUpHandler.Instance.GetScript<Inventory>();
            var slot = inventory.GetItemSlotUI(focusID);

            if (slot == null) return;

            target = slot.GetComponent<RectTransform>();

            if (inventory.GetTabType() != InventoryUI.TabType.All)
            {
                FocusInfo info = new();
                info.Rect = _focusTargetDict["AllSlot"];
                info.Type = 0;

                _focusQueue.Enqueue(info);
            }
        }
        else
        {
            _focusTargetDict.TryGetValue(focusID, out target);
        }

        if (target != null)
        {
            FocusInfo info = new();
            info.Rect = target;
            info.Type = type;
            info.Text = tutorialText;
            info.TextPos = textPos;

            _focusQueue.Enqueue(info);
        }

        StartFocus().Forget();
    }

    public async UniTaskVoid StartFocus()
    {
        if (IsFocusing) return;

        IsFocusing = true;

        do
        {
            _clickBlocker.SetActive(true);

            FocusInfo focusInfo = _focusQueue.Dequeue();
            RectTransform rectTransform = focusInfo.Rect;
            GameObject go = rectTransform.gameObject;
            bool isClicked = false;

            KeyCode keyCode = KeyCode.None;

            if (rectTransform.TryGetComponent(out KeyGuideUI guideUI))
            {
                switch (guideUI.CurrentType)
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
            else if (rectTransform.parent.TryGetComponent(out Palette palette))
            {
                switch (rectTransform.name)
                {
                    case "Bag":
                        keyCode = KeyCode.Tab;
                        break;

                    case "Quest":
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

            Rect rect = new();
            rect.size = rectTransform.sizeDelta;

            await UniTask.DelayFrame(3);

            Vector3 pos = rectTransform.TransformPoint(rectTransform.rect.center);
            rect.position = pos;
            _maskRaycast.SetRaycastRect(rect);

            Vector3 scale = new Vector3((focusInfo.TextPos.x > 0) ? 1 : -1, 1, 1);
            _textBGRect.localScale = scale;
            _textRect.localScale = scale;
            _textBGRect.position = focusInfo.TextPos + rect.position;

            _tmp.text = focusInfo.Text;

            gameObject.SetActive(true);
            UniTask fadeTask = Fade(_shadowImage, 0, _shadeAlpha, _shadowDuration);

            if (keyCode == KeyCode.None || focusInfo.Type == 2)
            {
                await HighLight(pos); ;
            }

            await fadeTask;

            _clickBlocker.SetActive(false);

            while (true)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    await UniTask.NextFrame();

                    if (focusInfo.Type == 2 || 
                       (isClicked && EventSystem.current.currentSelectedGameObject == go))
                    {
                        break;
                    }
                }

                if (keyCode != KeyCode.None && Input.GetKeyDown(keyCode))
                {
                    UIPopUpHandler.Instance.CloseUI<KeyGuideUI>();

                    switch (keyCode)
                    {
                        case KeyCode.Tab:
                            UIPopUpHandler.Instance.CloseAllAndOpenUI<Inventory>();
                            break;

                        case KeyCode.Alpha1:
                            UIPopUpHandler.Instance.CloseAllAndOpenUI<Quest>();
                            break;
                    }

                    break;
                }

                await UniTask.Yield();
            }

            _highlightRect.gameObject.SetActive(false);
            gameObject.SetActive(false);
        } while (_focusQueue.Count > 0);

        IsFocusing = false;
    }

    public async UniTask Fade(MaskableGraphic targetImage, float startAlpha, float targetAlpha, float duration, bool useMaterialColor = false)
    {
        Material material = targetImage.materialForRendering;
        Color color;

        if (useMaterialColor && material)
        {
            color = material.color;
        }
        else
        {
            color = targetImage.color;
        }

        float startTime = Time.time;
        float t;

        while ((t = (Time.time - startTime) / duration) < 1)
        {
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);

            if (useMaterialColor && material)
            {
                material.color = color;
            }
            else
            {
                targetImage.color = color;
            }

            await UniTask.NextFrame();
        }

        color.a = targetAlpha;
        targetImage.color = color;
    }

    public async UniTask HighLight(Vector2 pos)
    {
        _highlightRect.position = pos;
        _highlightRect.localScale = new Vector3(1, 1, 1);
        _highlightRect.gameObject.SetActive(true);

        await Fade(_highlightImage, 0, 1, _highlightFadeDuration, true);
        await UniTask.Delay(200);

        float startTime = Time.time;
        float t;

        while ((t = (Time.time - startTime) / _highlightScaleDuration) < 1)
        {
            float l = Mathf.Lerp(1, 0.5f, t);
            _highlightRect.localScale = new Vector3(l, l, 1);

            await UniTask.NextFrame();
        }

        _highlightRect.localScale = new Vector3(0.5f, 0.5f, 1);
    }
}

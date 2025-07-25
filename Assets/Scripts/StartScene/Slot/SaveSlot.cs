using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _slotNameText;
    [SerializeField]
    private TMP_Text _dateText;
    [SerializeField]
    private TMP_Text _inGameDateText;
    [SerializeField]
    private TMP_Text _questTypeText;
    [SerializeField]
    private TMP_Text _questInfoText;
    [SerializeField]
    private TMP_Text _locationText;

    private ScrollRect _scrollRect;
    private EventTrigger _eventTrigger;
    private Outline _outline;

    public bool Exist;
    public SaveSlotIndex Index;
    public SaveFileManager SaveFileManager;

    private bool _isDragging;
    private Vector2 _pressPosition;
    public float dragThreshold = 5f;

    private void Awake()
    {
        _scrollRect = GetComponentInParent<ScrollRect>();
        _outline = GetComponentInChildren<Outline>();
        _eventTrigger = GetComponent<EventTrigger>();
        _eventTrigger.triggers.Clear();

        AddEntry(EventTriggerType.InitializePotentialDrag, e =>
            _scrollRect.OnInitializePotentialDrag((PointerEventData)e));
        AddEntry(EventTriggerType.BeginDrag, OnBeginDrag);
        AddEntry(EventTriggerType.Drag, OnDrag);
        AddEntry(EventTriggerType.EndDrag, OnEndDrag);
        AddEntry(EventTriggerType.PointerDown, OnPointerDown);
        AddEntry(EventTriggerType.PointerClick, OnPointerClick);
    }

    private void AddEntry(EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(callback);
        _eventTrigger.triggers.Add(entry);
    }

    public void Set(SaveSlotData data, bool exist)
    {
        Index = data.Index;
        Exist = exist;
        _slotNameText.text = data.SlotName;
        _dateText.text = data.Date;
        _inGameDateText.text = $"{data.InGameDate}Day";
        _questTypeText.text = data.QuestType;
        _questInfoText.text = data.QuestInfo;
        _locationText.text = data.Location;
    }

    public void Select()
    {
        SaveFileManager.SetSelectSlot(this);
    }

    public void OnOutline()
    {
        _outline.enabled = true;
    }

    public void OffOutline()
    {
        _outline.enabled = false;
    }

    public void OnPointerDown(BaseEventData baseData)
    {
        PointerEventData eventData = (PointerEventData)baseData;
        _isDragging = false;
        _pressPosition = eventData.position;
    }

    public void OnBeginDrag(BaseEventData baseData)
    {
        PointerEventData eventData = (PointerEventData)baseData;
        _isDragging = true;
        _scrollRect.OnBeginDrag(eventData);
    }

    public void OnDrag(BaseEventData baseData)
    {
        PointerEventData eventData = (PointerEventData)baseData;
        if (!_isDragging &&
            Vector2.Distance(_pressPosition, eventData.position) > dragThreshold)
        {
            _isDragging = true;
        }
        _scrollRect.OnDrag(eventData);
    }

    public void OnEndDrag(BaseEventData baseData)
    {
        PointerEventData eventData = (PointerEventData)baseData;
        _scrollRect.OnEndDrag(eventData);
    }

    public void OnPointerClick(BaseEventData baseData)
    {
        PointerEventData eventData = (PointerEventData)baseData;
        if (_isDragging)
            return;
        Select();
    }
}

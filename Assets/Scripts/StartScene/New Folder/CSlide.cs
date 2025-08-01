using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CSlide : MonoBehaviour
{
    public Action<float> ClickAction;

    [Header("Default Settings")]
    public GameObject CirclePrefab;
    public Transform CircleRoot;
    public EventTrigger LeftBtn;
    public EventTrigger RightBtn;
    public TMP_Text NameText;

    [Header("Slide Settings")]
    public int _circleCnt;
    public float Padding;

    private int _selectIndex;

    private float[] _pos;
    private Image[] _circleImage;

    public void Init(Action<float> action, int index, int circleCnt, string name)
    {
        NameText.text = name;
        _circleCnt = circleCnt;
        _selectIndex = index;
        ClickAction = action;
        GenerateCircle();
        SetCircles();

        var entryLeft = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entryLeft.callback.AddListener((eventData) => { LeftBtnClick(); });
        LeftBtn.triggers.Add(entryLeft);

        var entryRight = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entryRight.callback.AddListener((eventData) => { RightBtnClick(); });
        RightBtn.triggers.Add(entryRight);
    }

    private void LeftBtnClick()
    {
        _selectIndex--;
        if (_selectIndex < -1)
            _selectIndex = -1;
        Set();
    }

    private void RightBtnClick()
    {
        _selectIndex++;
        if (_selectIndex >= _circleCnt)
            _selectIndex = _circleCnt - 1;
        Set();
    }

    private void Set()
    {
        ClickAction.Invoke((float)(_selectIndex + 1) / _circleCnt);
        SetCircles();
    }

    private void SetCircles()
    {
        for (int i = 0; i < _circleCnt; i++)
        {
            if (i > _selectIndex)
                _circleImage[i].color = Color.white;
            else
                _circleImage[i].color = Color.gray;
        }
    }

    private void GenerateCircle()
    {
        if (_circleCnt < 2)
        {
            Debug.LogWarning("CSlide : Circle 수가 2개이 미만입니다. 최소 2개 이상을 입력해주세요.");
        }

        float wholeSize = CircleRoot.GetComponent<RectTransform>().sizeDelta.x;
        float paddingSize = wholeSize - Padding * 2;
        float interval = paddingSize / (_circleCnt - 1);

        _pos = new float[_circleCnt];
        _circleImage = new Image[_circleCnt];

        _pos[0] = Padding;
        for (int i = 1; i <= _circleCnt - 2; i++)
            _pos[i] = interval * i + Padding;
        _pos[_circleCnt - 1] = wholeSize - Padding;

        for (int i = 0; i < _circleCnt; i++)
        {
            GameObject circleObj = Instantiate(CirclePrefab, CircleRoot);
            _circleImage[i] = circleObj.GetComponent<Image>();
            _circleImage[i].color = Color.white;
            RectTransform rect = circleObj.GetComponent<RectTransform>();
            rect.anchorMin = new(0, 0.5f);
            rect.anchorMax = new(0, 0.5f);
            rect.anchoredPosition = new(_pos[i], 0);
            EventTrigger trigger = circleObj.GetComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            int index = i;
            entry.callback.AddListener((eventData) => { CircleClick(index); } );
            trigger.triggers.Add(entry);
        }
    }

    private void CircleClick(int index)
    {
        _selectIndex = index;
        Set();
    }
}

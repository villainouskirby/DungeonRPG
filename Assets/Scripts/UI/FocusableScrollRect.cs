using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FocusableScrollRect : ScrollRect
{
    public override void OnScroll(PointerEventData data)
    {
        if (UIPopUpHandler.Instance.IsUIFocusing())
        {
            data.Use();
            return;
        }

        base.OnScroll(data);
    }
}
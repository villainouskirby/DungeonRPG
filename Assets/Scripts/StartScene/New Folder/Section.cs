using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Section : MonoBehaviour
{
    public TMP_Text SectionNameText;

    public void Init(string name)
    {
        SectionNameText.text = name;
    }
}

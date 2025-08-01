using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Option : MonoBehaviour
{
    private OptionData _optionData;

    public AudioSource BGMSource;
    public AudioSource SFXSource;

    public Transform SectionRoot;

    [Header("Content Prefabs")]
    public GameObject SectionPrefab;
    public GameObject DropdownPrefab;
    public GameObject CSlidePrefab;

    public void Start()
    {
        Init();
    }

    public void Init()
    {
        _optionData = new();
        SetOption(_optionData);

        RefreshVerticals();
    }

    public void RefreshVerticals()
    {
        var verticals = transform.GetComponentsInChildren<VerticalLayoutGroup>(includeInactive: true);

        foreach (var v in verticals)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(v.GetComponent<RectTransform>());
        }
    }

    public void SetOption(OptionData data)
    {
        Section graphicSection = GenerateSection("그래픽");
        GenerateDropdown<ScreenSize>(graphicSection, (value) => { data.ScreenSize = value; });
        GenerateDropdown<ScreenMode>(graphicSection, (value) => { data.ScreenMode = value; });
        Section soundSection = GenerateSection("사운드");
        GenerateCSlide(soundSection, BGMSource, null, "배경음", 10);
        GenerateCSlide(soundSection, SFXSource, null, "효과음", 10);
        Section controllSection = GenerateSection("조작");
        Section gamePlaySection = GenerateSection("게임 플레이");
        GenerateCSlide(gamePlaySection, (value) => { data.ShakePower = value; }, "화면 흔들림", 10);
    }

    private Section GenerateSection(string sectionName)
    {
        GameObject sectionObj = Instantiate(SectionPrefab, SectionRoot);
        sectionObj.name = $"{sectionObj.name}_{sectionName}";
        Section section = sectionObj.GetComponent<Section>();
        section.Init(sectionName);

        return section;
    }

    private void GenerateCSlide(Section targetSection, AudioSource audioSource, AudioSourceData data, string name, int circleCnt)
    {
        if (data != null)
        {
            audioSource.volume = data.Value;
            audioSource.mute = data.Mute;
        }
        GenerateCSlide(targetSection, (value) => { audioSource.volume = value; }, name, circleCnt);
    }

    private void GenerateCSlide(Section targetSection, Action<float> action, string name, int circleCnt)
    {
        GameObject cslideObj = Instantiate(CSlidePrefab, targetSection.transform);
        cslideObj.name = $"CSlide_{name}";
        CSlide cslide = cslideObj.GetComponent<CSlide>();

        cslide.Init(action, circleCnt, circleCnt, name);

    }

    private void GenerateDropdown<type>(Section targetSection, Action<type> action) where type : Enum
    {
        GameObject dropdownObj = Instantiate(DropdownPrefab, targetSection.transform);
        dropdownObj.name = $"Dropdown_{nameof(type)}";
        TMP_Dropdown dropdown = dropdownObj.GetComponentInChildren<TMP_Dropdown>();

        dropdown.ClearOptions();

        Array enumNames = Enum.GetNames(typeof(type));

        List<string> options = new();

        foreach (var name in enumNames)
        {
            options.Add(name.ToString());
        }

        dropdown.AddOptions(options);
        dropdown.RefreshShownValue();

        dropdown.onValueChanged.AddListener((index) =>
        {
            type value = (type)Enum.Parse(typeof(type), dropdown.options[index].text);
            action(value);
        }
        );
    }
}
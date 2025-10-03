using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class Palette : UIBase
{
    public enum PaletteType
    {
        Bag = 0,
        Quest,
        Map,
        Book
    }

    [Serializable]
    private class SpriteArr
    {
        public Sprite[] Sprites = new Sprite[3];
    }

    [SerializeField] private Image[] _images = new Image[4];
    [SerializeField] private SpriteArr[] _sprites = new SpriteArr[4];

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
        _isActvieOnStart = true;
    }

    public void SetPalette(string paletteName, int spriteIdx = 0)
    {
        if (Enum.TryParse(paletteName, out PaletteType type))
        {
            SetPalette(type, spriteIdx);
        }
    }

    public void SetPalette(PaletteType type, int spriteIdx = 0)
    {
        SetPalette((int)type, spriteIdx);
    }

    /// <param name="imageIdx"> 0 : bag, 1 : quest, 2 : map, 3 : book </param>
    /// <param name="spriteIdx"> 0 : normal, 1 : changed, 2 : tutorial </param>
    public void SetPalette(int imageIdx, int spriteIdx = 0)
    {
        try
        {
            _images[imageIdx].sprite = _sprites[imageIdx].Sprites[spriteIdx];
        }
        catch (IndexOutOfRangeException)
        {

        }
    }
}

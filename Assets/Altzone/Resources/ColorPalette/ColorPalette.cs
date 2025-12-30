using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ColorPalette",
    menuName = "Design System/Color Palette",
    order = 1
)]
public class ColorPalette : ScriptableObject
{
    [Header("Main UI Colors")]
    [SerializeField] private Color settingsGrey = Color.grey;
    [SerializeField] private Color settingsBlack = Color.black;
    [SerializeField] private Color settingsWhite = Color.white;

    [Header("UI Color List")]
    [SerializeField] private List<ColourPaletteBlock> colors = new List<ColourPaletteBlock>();

    public Color SettingsGrey => settingsGrey;

    public Color GetColor(Colours colour)
    {
        switch (colour)
        {
            case Colours.SettingsWhite:
                return settingsWhite;
            case Colours.SettingsBlack:
                return settingsBlack;
            case Colours.SettingsGrey:
                return settingsGrey;
            default:
                return settingsWhite;
        }
    }
}

[Serializable]
public class ColourPaletteBlock
{
    public string name;
    public Color colour;
}

public enum Colours
{
    SettingsWhite,
    SettingsBlack,
    SettingsGrey
}






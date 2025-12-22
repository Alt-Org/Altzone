using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ColorPalette",
    menuName = "Design System/Color Palette",
    order = 1
)]
public class ColorPalette : ScriptableObject
{
    [Header("UI Colors")]
    public Color settingsGrey;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorType
{
    Red,
    Orange,
    Yellow,
    Green,
    Turquoise,
    Indigo,
    Violet
}

public class ColorConstants
{
    public const string Red = "#FF0000";
    public const string Orange = "#FF8E00";
    public const string Yellow = "#FFFF00";
    public const string Green = "#008E00";
    public const string Turquoise = "#00C0C0";
    public const string Indigo = "#400098";
    public const string Violet = "#8E008E";

    public static Color GetColorConstant(ColorType color)
    {

        Color myColor;

        switch (color)
        {
            case ColorType.Red:
                ColorUtility.TryParseHtmlString(Red, out myColor);
                return myColor;
            case ColorType.Orange:
                ColorUtility.TryParseHtmlString(Orange, out myColor);
                return myColor;
            case ColorType.Yellow:
                ColorUtility.TryParseHtmlString(Yellow, out myColor);
                return myColor;
            case ColorType.Green:
                ColorUtility.TryParseHtmlString(Green, out myColor);
                return myColor;
            case ColorType.Turquoise:
                ColorUtility.TryParseHtmlString(Turquoise, out myColor);
                return myColor;
            case ColorType.Indigo:
                ColorUtility.TryParseHtmlString(Indigo, out myColor);
                return myColor;
            case ColorType.Violet:
                ColorUtility.TryParseHtmlString(Violet, out myColor);
                return myColor;
            default:
                return Color.white;
        }
    }
}

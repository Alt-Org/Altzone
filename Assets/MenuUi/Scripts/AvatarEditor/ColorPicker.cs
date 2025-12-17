using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;
using System.Linq;

namespace MenuUi.Scripts.AvatarEditor
{
    //public class ColorPicker : MonoBehaviour
    //{
    //    [SerializeField] private AvatarEditorCharacterHandle _avatarEditorCharacterHandle;
    //    [SerializeField] private AvatarEditorFeatureButtonsHandler _featureButtonsHandler;
    //    [Space]
    //    [SerializeField] private GameObject _colorButtonPrefab;
    //    [SerializeField] private GameObject _defaultColorButtonPrefab;
    //    [SerializeField] private List<Color> _colors;
    //    [SerializeField] private List<Transform> _colorButtonPositions;
    //    [SerializeField] private Transform _characterImageParent;
    //    [SerializeField] private Sprite _colorImage;

    //    private List<string> _currentColors = new()
    //    {
    //        "#ffffff",
    //    };
    //    private FeatureSlot _currentlySelectedCategory;

    //    public void OnEnable()
    //    {
    //        SetColorButtons();
    //    }

    //    public void OnDisable()
    //    {
    //        for (int i = 0; i < 8; i++)
    //            if (i < _colors.Count)
    //                _featureButtonsHandler.SetOnClick(SetColor, null, Color.white, i);
    //    }

    //    public void SelectFeature(FeatureSlot feature)
    //    {
    //        _currentlySelectedCategory = feature;
    //        //_colorChangeTarget = _characterImageParent.GetChild(0).GetChild((int)feature).GetComponent<Image>();
    //    }

    //    private void SetColorButtons()
    //    {
    //        for (int i = 0; i < 8; i++)
    //        {
    //            if (i < _colors.Count)
    //            {
    //                _featureButtonsHandler.SetOnClick(SetColor, _colorImage, _colors[i], i);
    //                continue;
    //            }

    //            _featureButtonsHandler.SetOff(i);
    //        }
    //    }

    //    private void SetColor(Color color)
    //    {
    //        _currentColors[0] = "#" + ColorUtility.ToHtmlStringRGB(color);
    //        _avatarEditorCharacterHandle.SetHeadColor(color);
    //    }

    //    public string GetCurrentColors()
    //    {
    //        string[] colors = new string[_currentColors.Count];
    //        _currentColors.CopyTo(colors);
    //        return colors[0];
    //    }

    //    public Color GetCurrentColorsAsColors()
    //    {
    //        List<Color> colors = new List<Color>();

    //        foreach (string stringColor in _currentColors)
    //        {
    //            if (stringColor != null && ColorUtility.TryParseHtmlString(stringColor, out Color color))
    //            {
    //                colors.Add(color);
    //                return color;
    //            }

    //            colors.Add(Color.white);
    //        }

    //        return Color.white;
    //    }

    //    public void SetLoadedColors(PlayerAvatar avatar)
    //    {
    //        string colorCode = avatar.Color;
    //        List<string> avatarFeatures = new List<string>{ avatar.HairId, 
    //                                                        avatar.EyesId,
    //                                                        avatar.NoseId,
    //                                                        avatar.MouthId,
    //                                                        avatar.BodyId,
    //                                                        avatar.HandsId,
    //                                                        avatar.FeetId
    //                                                        };
    //        if (string.IsNullOrWhiteSpace(colorCode) && ColorUtility.TryParseHtmlString(colorCode, out Color color))
    //            SetColor(color);
    //        //for (int i = 0; i < _currentColor.Count; i++)
    //        //{
    //        //    SelectFeature((FeatureSlot)i);

    //        //    if (features[i] == "")
    //        //    {
    //        //        Debug.LogError($"{i} sdasd");
    //        //        SetTransparentColor();
    //        //    }
    //        //    //else if (features[i] == FeatureID.Default)
    //        //    //{
    //        //    //    Debug.Log("feature was default when coloring!");
    //        //    //}
    //        //    else if (i < _currentColor.Count)
    //        //    {
    //        //        if (ColorUtility.TryParseHtmlString(colors[i], out Color color))
    //        //            SetColor(color);
    //        //    }
    //        //}
    //    }

    //    public void RestoreDefaultColor(FeatureSlot slot)
    //    {
    //        _currentColors[0] = "#ffffff";
    //    }

    //    // internal void SetCurrentCategoryAndColors(FeatureSlot category, List<FeatureColor> colors) {
    //    //     _currentlySelectedCategory = category;
    //    //     _currentColors = colors;
    //    // }
    //}
}

using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;

namespace Altzone.Scripts.Store
{
    [Serializable]
    public class AdStoreObject
    {
        private string _borderFrame;
        private string _backgroundColour;
        private TMP_FontAsset _textFont;
        private string _textColour;
        private Sprite _furniture;
        private string _adText;
        private bool _isAdText = true;

        public string previousColor;
        public string previousBorder;
        public Sprite previousFurniture;
        public string previousTextColor;
        public TMP_FontAsset previousFont;
        public string previousText;
        public bool previousTextState = true;

        public string BorderFrame
        {
            get => _borderFrame;
            set
            {
                if (AdDecorationReference.Instance.GetBorderFrameSprite(value) != null) _borderFrame = value;
                else
                {
                    Debug.LogError($"Invalid border id: \"{value}\". Border not changed.");
                }
            }
        }
        public string BackgroundColour
        {
            get => _backgroundColour;
            set
            {
                if (ColorUtility.TryParseHtmlString(value, out Color colour)) _backgroundColour = value;
                else
                {
                    Debug.LogError($"Invalid colour value: \"{value}\". Background colour not changed.");
                }
            }
        }

        public TMP_FontAsset TextFont
        {
            get => _textFont;
            set
            {
                if (value != null) _textFont = value;
                else
                {
                    Debug.LogError($"Given font value is null. Font not changed.");
                }
            }
        }

        public string TextColour
        {
            get => _textColour;
            set
            {
                if (ColorUtility.TryParseHtmlString(value, out Color colour)) _textColour = value;
                else
                {
                    Debug.LogError($"Invalid colour value: \"{value}\". Text colour not changed.");
                }
            }
        }

        public Sprite Furniture
        {
            get => _furniture;
            set
            {
                if (value != null) _furniture = value;
                else
                {
                    Debug.LogError($"Given sprite value is null. Font not changed.");
                }
            }
        }

        public string AdText
        {
            get => _adText;
            set
            {
                if (value != null) _adText = value;
                else
                {
                    Debug.LogError($"Given string value is null. Font not changed.");
                }
            }
        }

        public bool IsAdText
        {
            get => _isAdText;
            set
            {
                _isAdText = value;
            }
        }

        public AdStoreObject(string border, string backgroundColour)
        {
            if (AdDecorationReference.Instance.GetBorderFrameSprite(border) != null) _borderFrame = border;
            else
            {
                Debug.LogWarning($"Invalid border id: \"{border}\". Using the default border.");
                _borderFrame = AdDecorationReference.Instance.FrameList[0].Name;
            }

            if (ColorUtility.TryParseHtmlString(backgroundColour, out Color colour)) _backgroundColour = backgroundColour;

            if (string.IsNullOrEmpty(AdText))
            {
                _adText = "Myyntikoju avattu!";
            }

            if (TextFont == null) _textFont = AdDecorationReference.Instance.FontList[0].Font;

            if (_textColour == null) _textColour = "#000000";
            //if (ColorUtility.TryParseHtmlString(adTextColour, out Color textColour)) _textColour = adTextColour;
            //else _textColour = "#000000";

            //if (AdDecorationReference.Instance.GetFurnitureSprite(furniture) != null) _furniture = furniture;
            //else
            //{
            //    Debug.LogWarning($"Invalid furniture id: \"{border}\". Using the default furniture.");
            //    _furniture = AdDecorationReference.Instance.FurnitureList[0].Name;
            //}
        }
    }
}

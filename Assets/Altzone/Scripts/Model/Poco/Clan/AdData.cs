using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;

namespace Altzone.Scripts.Store
{
    [Serializable]
    public class AdStoreObject
    {
        public string _borderFrame;
        public string _backgroundColour;
        public TMPro.TMP_FontAsset _textFont; // Uusi lisäys (Perttu)
        public string _textColour; // Uusi lisäys (Perttu)
        public string _furniture; // Uusi lisäys (Perttu)
        public string _adText; // Uusi lisäys (Perttu)
        public bool _isAdText = true; // Uusi lisäys (Perttu)

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

        public TMPro.TMP_FontAsset TextFont // Uusi lisäys (Perttu)
        {
            get => _textFont;
            set
            {
                if (value != null) _textFont = value;
                else
                {
                    Debug.LogError($"Invalid font value: \"{value}\". Font not changed.");
                }
            }
        }

        public string TextColour // Uusi lisäys (Perttu)
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

        public string Furniture // Uusi lisäys (Perttu)
        {
            get => _furniture;
            set
            {
                if (AdDecorationReference.Instance.GetFurnitureSprite(value) != null) _furniture = value;
                else
                {
                    Debug.LogError($"Invalid furniture id: \"{value}\". Furniture not changed.");
                }
            }
        }

        public string AdText // Uusi lisäys (Perttu)
        {
            get => _adText;
            set
            {
                if (value != null) _adText = value;
                else
                {
                    Debug.LogError($"Invalid text value: \"{value}\". Text not changed.");
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
            else _backgroundColour = "#E35000";

            if (string.IsNullOrEmpty(_adText)) // Uusi lisäys (Perttu)
            {
                _adText = "Myyntikoju avattu!";
            }

            // Uusi lisäys (Perttu)
            if (_textColour == null) _textColour = "#000000";
            //if (ColorUtility.TryParseHtmlString(adTextColour, out Color textColour)) _textColour = adTextColour;
            //else _textColour = "#000000";

            //if (AdDecorationReference.Instance.GetFurnitureSprite(furniture) != null) _furniture = furniture;
            //else if (AdDecorationReference.Instance.FurnitureList == null)
            //{

            //}
            //else
            //{
            //    Debug.LogWarning($"Invalid furniture id: \"{border}\". Using the default furniture.");
            //    _furniture = AdDecorationReference.Instance.FurnitureList[0].Name;
            //}
        }
    }
}

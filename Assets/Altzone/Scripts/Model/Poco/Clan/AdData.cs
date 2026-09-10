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
        public TMPro.TMP_FontAsset _adTextFont; // Uusi lisäys (Perttu)
        public string _adTextColour; // Uusi lisäys (Perttu)

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

        public TMPro.TMP_FontAsset AdTextFont // Uusi lisäys (Perttu)
        {
            get => _adTextFont;
            set
            {
                if (value != null) _adTextFont = value;
                else
                {
                    Debug.LogError($"Invalid font value: \"{value}\". Ad text font not changed.");
                }
            }
        }

        public string AdTextColour // Uusi lisäys (Perttu)
        {
            get => _adTextColour;
            set
            {
                if (ColorUtility.TryParseHtmlString(value, out Color colour)) _adTextColour = value;
                else
                {
                    Debug.LogError($"Invalid colour value: \"{value}\". Ad text colour not changed.");
                }
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

            // Uusi lisäys (Perttu)
            //if (ColorUtility.TryParseHtmlString(adTextColour, out Color textColour)) _adTextColour = adTextColour;
            //else _adTextColour = "#000000";
        }
    }
}

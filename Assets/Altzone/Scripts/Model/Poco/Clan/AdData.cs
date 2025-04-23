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
        }
    }
}

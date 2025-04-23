using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    //[CreateAssetMenu(menuName = "ALT-Zone/AdDecorationReference", fileName = "AdDecorationReference")]
    public class AdDecorationReference : ScriptableObject
    {

        [SerializeField] private List<AdBorderFrameObject> _frameList;

        [Header("Colours")]
        [SerializeField] private Color _orangeColor;
        [SerializeField] private Color _yellowColor;
        [SerializeField] private Color _lightGreenColor;
        [SerializeField] private Color _lightBlueColor;
        [SerializeField] private Color _blueColor;
        [SerializeField] private Color _purpleColor;
        [SerializeField] private Color _darkPinkColor;
        [SerializeField] private Color _redColor;
        private List<Color> _colourList;


        private static AdDecorationReference _instance = null;
        private static bool _hasInstance = false;

        public List<AdBorderFrameObject> FrameList => _frameList; // Public accessor for _info

        public List<Color> ColourList
        {
            get
            {
                if (_colourList == null || _colourList.Count == 0)
                {
                    _colourList = new();
                    _colourList.Add(_orangeColor);
                    _colourList.Add(_yellowColor);
                    _colourList.Add(_lightGreenColor);
                    _colourList.Add(_lightBlueColor);
                    _colourList.Add(_blueColor);
                    _colourList.Add(_purpleColor);
                    _colourList.Add(_darkPinkColor);
                    _colourList.Add(_redColor);
                }
                return _colourList;
            }
        }


        public static AdDecorationReference Instance
        {
            get
            {
                if (!_hasInstance)
                {
                    _instance = Resources.Load<AdDecorationReference>("Data/AdDecorationReference");
                    _hasInstance = _instance != null;
                }
                return _instance;
            }
        }

        public Sprite GetBorderFrameSprite(string name)
        {
            AdBorderFrameObject data = GetBorderFrame(name);
            if (data == null) return null;
            return data.Image;
        }

        private AdBorderFrameObject GetBorderFrame(string name)
        {
            //Debug.LogWarning($"Full name: {name}");
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            foreach (AdBorderFrameObject info in _frameList)
            {
                if (info.Name == name)
                {
                    return info;
                }
            }
            return null;
        }
    }

    [Serializable]
    public class AdBorderFrameObject
    {
        public string Name;
        public Sprite Image;
    }
}

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

        [SerializeField] private List<AdFurnitureObject> _furnitureList; // Uusi lisäys (Perttu)

        [Header("Colours")]
        [SerializeField] private Color _orangeColor;
        [SerializeField] private Color _yellowColor;
        [SerializeField] private Color _lightGreenColor;
        [SerializeField] private Color _lightBlueColor;
        [SerializeField] private Color _blueColor;
        [SerializeField] private Color _purpleColor;
        [SerializeField] private Color _darkPinkColor;
        [SerializeField] private Color _redColor;
        [SerializeField] private Color _blackColor; // Uusi lisäys (Perttu)
        private List<Color> _colourList;
        private List<Color> _textColourList; // Uusi lisäys (Perttu)

        private List<AdBorderFrameObject> _validatedFrameList = null;
        private List<AdFurnitureObject> _validatedFurnitureList = null; // Uusi lisäys (Perttu)
        private static AdDecorationReference _instance = null;
        private static bool _hasInstance = false;

        public List<AdBorderFrameObject> FrameList {
            get
            {
                if (_validatedFrameList == null) ValidateFrames();
                ValidateFrames();
                return _validatedFrameList;
            }
        } // Public accessor for _info

        public List<AdFurnitureObject> FurnitureList // Uusi lisäys (Perttu)
        {
            get
            {
                if (_validatedFurnitureList == null) ValidateFurniture();
                ValidateFurniture();
                return _validatedFurnitureList;
            }
        }

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

        public List<Color> TextColourList // Uusi lisäys (Perttu)
        {
            get
            {
                if (_textColourList == null || _textColourList.Count == 0)
                {
                    _textColourList = new();
                    _textColourList.Add(_blackColor);
                    _textColourList.Add(_yellowColor);
                    _textColourList.Add(_lightGreenColor);
                    _textColourList.Add(_lightBlueColor);
                    _textColourList.Add(_blueColor);
                    _textColourList.Add(_purpleColor);
                    _textColourList.Add(_darkPinkColor);
                    _textColourList.Add(_redColor);
                }
                return _textColourList;
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

        private void ValidateFrames()
        {
            HashSet<string> uniqueNames = new();
            HashSet<Sprite> uniqueMap = new();

            if (_validatedFrameList != null && _validatedFrameList.Count > 0) return;
            List<AdBorderFrameObject> frames = new();
            foreach (AdBorderFrameObject frame in _frameList)
            {
                if (!frame.IsValid()) continue;

                if (!uniqueNames.Add(frame.Name))
                {
                    Debug.LogError($"duplicate frame Name {frame.Name}");
                }
                if (!uniqueMap.Add(frame.Image))
                {
                    Debug.LogError($"duplicate frame Image {frame.Image}");
                    continue;
                }
                frames.Add(frame);
            }
            _validatedFrameList = frames;
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
                    if(info.IsValid()) return info;
                    else return null;
                }
            }
            return null;
        }

        // Uusi lisäys (Perttu)
        private void ValidateFurniture()
        {
            HashSet<string> uniqueNames = new();
            HashSet<Sprite> uniqueMap = new();

            if (_validatedFurnitureList != null && _validatedFurnitureList.Count > 0) return;
            List<AdFurnitureObject> furnitures = new();
            foreach (AdFurnitureObject furniture in _furnitureList)
            {
                if (!furniture.IsValid()) continue;

                if (!uniqueNames.Add(furniture.Name))
                {
                    Debug.LogError($"duplicate furniture Name {furniture.Name}");
                }
                if (!uniqueMap.Add(furniture.Image))
                {
                    Debug.LogError($"duplicate furniture Image {furniture.Image}");
                    continue;
                }
                furnitures.Add(furniture);
            }
            _validatedFurnitureList = furnitures;
        }
        // Uusi lisäys (Perttu)
        public Sprite GetFurnitureSprite(string name)
        {
            AdFurnitureObject data = GetFurniture(name);
            if (data == null) return null;
            return data.Image;
        }
        // Uusi lisäys (Perttu)
        private AdFurnitureObject GetFurniture(string name)
        {
            //Debug.LogWarning($"Full name: {name}");
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            foreach (AdFurnitureObject info in _furnitureList)
            {
                if (info.Name == name)
                {
                    if (info.IsValid()) return info;
                    else return null;
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

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Name)) return false;
            if (Image == null) return false;
            return true;
        }
    }

    [Serializable]
    public class AdFurnitureObject // Uusi lisäys (Perttu)
    {
        public string Name;
        public Sprite Image;

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Name)) return false;
            if (Image == null) return false;
            return true;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    [CreateAssetMenu(menuName = "ALT-Zone/AdDecorationReference", fileName = "AdDecorationReference")]
    public class AdDecorationReference : ScriptableObject
    {
        [SerializeField] private List<AdBorderFrameObject> _info;

        public List<AdBorderFrameObject> Info => _info; // Public accessor for _info

        public Sprite GetBorderFrameSprite(string name)
        {
            AdBorderFrameObject data = GetFurnitureDataSet(name);
            if (data == null) return null;
            return data.Image;
        }

        public AdBorderFrameObject GetFurnitureDataSet(string name)
        {
            //Debug.LogWarning($"Full name: {name}");
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            foreach (AdBorderFrameObject info in _info)
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

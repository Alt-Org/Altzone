using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace MenuUi.Scripts.Storage
{
    //[CreateAssetMenu(menuName = "ALT-Zone/StorageFurnitureReference", fileName = "StorageFurnitureReference")]
    public class StorageFurnitureReference : ScriptableObject
    {
        [SerializeField] private List<FurnitureSetInfo> _info;


        public FurnitureInfo GetFurnitureInfo(string name)
        {
            //Debug.LogWarning($"Full name: {name}");
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            string[] parts = name.Split("_");

            //Debug.LogWarning($"Set name: {parts[1]}");
            if (parts.Length != 2) { return null; }

            foreach(FurnitureSetInfo info in _info)
            {
                if(info.SetName == parts[1])
                {
                    foreach(FurnitureInfo info2 in info.list)
                    {
                        if(info2.Name == parts[0]) return info2;
                    }
                }
            }
            return null;
        }
    }

    [Serializable]
    public class FurnitureInfo
    {
        public string Name; 
        public Sprite Image;
        public string VisibleName;
    }

    [Serializable]
    public class FurnitureSetInfo
    {
        public string SetName;
        public List<FurnitureInfo> list;
    }

}

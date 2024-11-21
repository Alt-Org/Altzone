using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using MenuUI.Scripts.SoulHome;
using UnityEngine;

namespace MenuUi.Scripts.Storage
{
    //[CreateAssetMenu(menuName = "ALT-Zone/StorageFurnitureReference", fileName = "StorageFurnitureReference")]
    public class StorageFurnitureReference : ScriptableObject
    {
        [SerializeField] private List<FurnitureSetInfo> _info;

        public List<FurnitureSetInfo> Info => _info; // Public accessor for _info


        public FurnitureInfo GetFurnitureInfo(string name)
        {
            FurnitureInfoObject data = GetFurnitureData(name);
            if (data == null) return null;
            return new(data);
        }

        public GameObject GetSoulHomeFurnitureObject(string name)
        {
            FurnitureInfoObject data = GetFurnitureData(name);
            return data.FurnitureHandling.gameObject;
        }

        public GameObject GetSoulHomeTrayFurnitureObject(string name)
        {
            FurnitureInfoObject data = GetFurnitureData(name);
            return data.TrayFurniture.gameObject;
        }


        private FurnitureInfoObject GetFurnitureData(string name)
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
                    foreach(FurnitureInfoObject info2 in info.list)
                    {
                        if(info2.Name == parts[0]) return info2;
                    }
                }
            }
            return null;
        }
    }

    public class FurnitureInfo
    {
        public Sprite Image;
        public string VisibleName;

        public FurnitureInfo(FurnitureInfoObject data)
        {
            Image = data.Image;
            VisibleName = data.VisibleName;
        }
    }

    [Serializable]
    public class FurnitureInfoObject
    {
        public string Name; 
        public Sprite Image;
        public string VisibleName;
        public FurnitureHandling FurnitureHandling;
        public TrayFurniture TrayFurniture;
    }

    [Serializable]
    public class FurnitureSetInfo
    {
        public string SetName;
        public List<FurnitureInfoObject> list;
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace AltZone.Scripts.ReferenceSheets
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

        public FurnitureInfoObject GetFurnitureData(string name)
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
        public string ArtisticDescription;
        public string DiagnoseNumber;
        public BaseFurniture BaseFurniture;
    }

    [Serializable]
    public class FurnitureSetInfo
    {
        public string SetName;
        public string ArtistName;
        
        public List<FurnitureInfoObject> list;
    }

}

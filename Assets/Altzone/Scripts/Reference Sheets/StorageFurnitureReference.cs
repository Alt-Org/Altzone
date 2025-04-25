using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace Altzone.Scripts.ReferenceSheets
{
    //[CreateAssetMenu(menuName = "ALT-Zone/StorageFurnitureReference", fileName = "StorageFurnitureReference")]
    public class StorageFurnitureReference : ScriptableObject
    {
        [SerializeField] private List<FurnitureSetInfo> _info;

        public List<FurnitureSetInfo> Info => _info; // Public accessor for _info

        public FurnitureInfo GetFurnitureInfo(string name)
        {
            (FurnitureInfoObject data, FurnitureSetInfo setData) = GetFurnitureDataSet(name);
            if (data == null) return null;
            return new(data, setData);
        }

        public FurnitureInfoObject GetFurnitureData(string name)
        {
            (FurnitureInfoObject data, FurnitureSetInfo setData) = GetFurnitureDataSet(name);
            return data;
        }

        public (FurnitureInfoObject, FurnitureSetInfo) GetFurnitureDataSet(string name)
        {
            //Debug.LogWarning($"Full name: {name}");
            if (string.IsNullOrWhiteSpace(name))
            {
                return (null, null);
            }

            string[] parts = name.Split("_");

            //Debug.LogWarning($"Set name: {parts[1]}");
            if (parts.Length != 2) { return (null, null); }

            foreach(FurnitureSetInfo info in _info)
            {
                if(info.SetName == parts[1])
                {
                    foreach(FurnitureInfoObject info2 in info.list)
                    {
                        if(info2.Name == parts[0]) return (info2, info);
                    }
                }
            }
            return (null, null);
        }

        public List<GameFurniture> GetGameFurniture()
        {
            List<GameFurniture> furnitures = new();
            int i = 1;
            foreach (FurnitureSetInfo info in _info)
            {
                foreach (FurnitureInfoObject info2 in info.list)
                {
                    BaseFurniture baseFurniture = info2.BaseFurniture;
                    baseFurniture.Name = info2.Name + "_" + info.SetName;
                    FurnitureInfo furnitureInfo = new(info2, info);
                    furnitures.Add(new(i.ToString(), baseFurniture, furnitureInfo));
                    i++;
                }
            }
            return furnitures;
        }
        public bool AddFurniture(FurnitureSetInfo setInfo)
        {
            if (Application.isPlaying)
            {
                Debug.LogError("Don't try to add furniture while the game is running.");
                return false;
            }

            FurnitureSetInfo localSet = _info.FirstOrDefault(x => x.SetName == setInfo.SetName);
            if (localSet == null)
            {
                _info.Add(setInfo);
                return true;
            }
            else
            {
                bool added = false;
                foreach (FurnitureInfoObject furnitureInfo in setInfo.list)
                {
                    FurnitureInfoObject localFurniture = localSet.list.FirstOrDefault(x => x.Name == furnitureInfo.Name);
                    if (localFurniture == null)
                    {
                        localSet.list.Add(furnitureInfo);
                        added = true;
#if UNITY_EDITOR
                        AssetDatabase.Refresh();
                        EditorUtility.SetDirty(this);
                        AssetDatabase.SaveAssets();
#endif
                    }
                    else
                    {
                        Debug.LogError("Furniture already exist.");
                    }
                }
                return added;
            }
        }
    }

    public class FurnitureInfo
    {
        public Sprite Image;
        public Sprite PosterImage;
        public string VisibleName;
        public string SetName;
        public string ArtistName;
        public string ArtisticDescription;
        public string DiagnoseNumber;

        public FurnitureInfo(FurnitureInfoObject data, FurnitureSetInfo setData)
        {
            Image = data.Image;
            PosterImage = data.PosterImage;
            VisibleName = data.VisibleName;
            SetName = setData.SetName;
            ArtistName = setData.ArtistName;
            ArtisticDescription = data.ArtisticDescription;
            DiagnoseNumber = data.DiagnoseNumber;
        }
    }

    [Serializable]
    public class FurnitureInfoObject
    {        
        public string Name;
        public Sprite Image;
        public Sprite PosterImage;
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

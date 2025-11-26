using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Altzone.Scripts.Model.Poco.Game;
using UnityEditor;
using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    //[CreateAssetMenu(menuName = "ALT-Zone/StorageFurnitureReference", fileName = "StorageFurnitureReference")]
    public class StorageFurnitureReference : ScriptableObject
    {
        [SerializeField] private List<FurnitureSetInfo> _info;

        public List<FurnitureSetInfo> Info => _info; // Public accessor for _info

        private static StorageFurnitureReference _instance;
        private static bool _hasInstance;

        public static StorageFurnitureReference Instance
        {
            get
            {
                if (!_hasInstance)
                {
                    _instance = Resources.Load<StorageFurnitureReference>(nameof(StorageFurnitureReference));
                    _hasInstance = _instance != null;
                }
                return _instance;
            }
        }


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

        public List<GameFurniture> GetAllGameFurniture()
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

        public GameFurniture GetGameFurniture(string name)
        {
            (FurnitureInfoObject data, FurnitureSetInfo setData) = GetFurnitureDataSet(name);
            if (data == null || setData == null)
            {
                Debug.LogWarning($"No Furniture can be found with name: {name}");
                return null;
            }

            FurnitureInfo furnitureInfo = new(data, setData);
            GameFurniture furniture = new("0", data.BaseFurniture, furnitureInfo);

            return furniture;
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
        public Sprite RibbonImage;
        public string VisibleName;
        public string EnglishName;
        public string SetName;
        public string SetNameEnglish;
        public string ArtistName;
        public string ArtisticDescription;
        public string EnglishArtisticDescription;
        public string DiagnoseNumber;

        public FurnitureInfo(FurnitureInfoObject data, FurnitureSetInfo setData)
        {
            Image = data.Image;
            PosterImage = data.PosterImage;
            RibbonImage = data.RibbonImage;
            VisibleName = data.VisibleName;
            EnglishName = data.EnglishName;
            SetName = setData.SetName;
            SetNameEnglish = setData.SetNameEnglish;
            ArtistName = setData.ArtistName;
            ArtisticDescription = data.ArtisticDescription;
            EnglishArtisticDescription = data.EnglishArtisticDescription;
            DiagnoseNumber = data.DiagnoseNumber;
        }
    }

    [Serializable]
    public class FurnitureInfoObject
    {        
        public string Name;
        public string EnglishName;
        public Sprite Image;
        public Sprite PosterImage;
        public Sprite RibbonImage;
        public string VisibleName;
        public string ArtisticDescription;
        public string EnglishArtisticDescription;
        public string DiagnoseNumber;
        public BaseFurniture BaseFurniture;
    }

    [Serializable]
    public class FurnitureSetInfo
    {
        public string SetName;
        public string SetNameEnglish;
        public string ArtistName;
        
        public List<FurnitureInfoObject> list;
    }
}

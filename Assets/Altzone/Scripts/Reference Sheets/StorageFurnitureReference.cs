using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Altzone.Scripts.Model.Poco.Game;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

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
        public Sprite SidewaysImage;
        public Sprite PosterImage;
        public Sprite RibbonImage;
        [FormerlySerializedAs("VisibleName")] public string FinnishName;
        public string EnglishName;
        [FormerlySerializedAs("SetName")] public string SetNameFinnish;
        public string SetNameEnglish;
        public Sprite SetRibbonBackground;
        public Sprite SetPosterBackground;
        public Sprite SetFontName;
        public string ArtistName;
        [FormerlySerializedAs("ArtisticDescription")] public string FinnishArtisticDescription;
        public string EnglishArtisticDescription;
        public string DiagnoseNumber;

        public string VisibleName
        {
            get
            {
                if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
                    return FinnishName;
                else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
                {
                    if (string.IsNullOrEmpty(EnglishName)) return FinnishName;
                    return EnglishName;
                }
                else
                    return FinnishName;
            }
        }
        public string SetName
        {
            get
            {
                if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
                    return SetNameFinnish;
                else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
                {
                    if (string.IsNullOrEmpty(SetNameEnglish)) return SetNameFinnish;
                    return SetNameEnglish;
                }
                else
                    return SetNameFinnish;
            }
        }
        public string ArtisticDescription
        {
            get
            {
                if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
                    return FinnishArtisticDescription;
                else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
                {
                    if (string.IsNullOrEmpty(EnglishArtisticDescription)) return FinnishArtisticDescription;
                    return EnglishArtisticDescription;
                }
                else
                    return FinnishArtisticDescription;
            }
        }

        public FurnitureInfo(FurnitureInfoObject data, FurnitureSetInfo setData)
        {
            Image = data.Image;
            SidewaysImage = data.SidewaysImage;
            PosterImage = data.PosterImage;
            SetRibbonBackground = setData.SetRibbonBackground;
            SetPosterBackground = setData.SetPosterBackground;
            SetFontName = setData.SetFontName;
            RibbonImage = data.RibbonImage;
            FinnishName = data.VisibleName;
            EnglishName = data.EnglishName;
            SetNameFinnish = setData.SetName;
            SetNameEnglish = setData.SetNameEnglish;
            ArtistName = setData.ArtistName;
            FinnishArtisticDescription = data.ArtisticDescription;
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
        public Sprite SidewaysImage;
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
        public Sprite SetRibbonBackground;
        public Sprite SetPosterBackground;
        public Sprite SetFontName;
        public string ArtistName;

        public List<FurnitureInfoObject> list;
    }
}

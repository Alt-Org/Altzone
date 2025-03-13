using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "ALT-Zone/AvatarPartsData")]
public class AvatarPartsReference : ScriptableObject
{
    [SerializeField] private Sprite _head;
    [SerializeField] private List<AvatarBaseCategoriesInfo> _info;

    public List<AvatarBaseCategoriesInfo> Info => _info; // Public accessor for _info

    //public FurnitureInfo GetFurnitureInfo(string name)
    //{
    //    (AvatarCategoriesInfo data, AvatarPartsInfo setData) = GetFurnitureDataSet(name);
    //    if (data == null) return null;
    //    return new(data, setData);
    //}

    public AvatarCategoriesInfo GetFurnitureData(string name)
    {
        (AvatarCategoriesInfo data, AvatarBaseCategoriesInfo setData) = GetFurnitureDataSet(name);
        return data;
    }

    public (AvatarCategoriesInfo, AvatarBaseCategoriesInfo) GetFurnitureDataSet(string name)
    {
        //Debug.LogWarning($"Full name: {name}");
        if (string.IsNullOrWhiteSpace(name))
        {
            return (null, null);
        }

        string[] parts = name.Split("_");

        //Debug.LogWarning($"Set name: {parts[1]}");
        if (parts.Length != 2) { return (null, null); }

        foreach (AvatarBaseCategoriesInfo info in _info)
        {
            if (info.SetName == parts[1])
            {
                //foreach (AvatarCategoriesInfo info2 in info.list)
                //{
                //    if (info2.Name == parts[0]) return (info2, info);
                //}
            }
        }
        return (null, null);
    }

    public List<GameFurniture> GetGameFurniture()
    {
        List<GameFurniture> furnitures = new();
        int i = 1;
        foreach (AvatarBaseCategoriesInfo info in _info)
        {
            //foreach (AvatarCategoriesInfo info2 in info.list)
            //{

            //}
        }
        return furnitures;
    }

    public bool AddFurniture(AvatarBaseCategoriesInfo setInfo)
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Don't try to add furniture while the game is running.");
            return false;
        }

        AvatarBaseCategoriesInfo localSet = _info.FirstOrDefault(x => x.SetName == setInfo.SetName);
        if (localSet == null)
        {
            _info.Add(setInfo);
            return true;
        }
        else
        {
            bool added = false;
            //foreach (AvatarCategoriesInfo furnitureInfo in setInfo.list)
            //{
            //    AvatarCategoriesInfo localFurniture = localSet.list.FirstOrDefault(x => x.Name == furnitureInfo.Name);
            //    if (localFurniture == null)
            //    {
            //        localSet.list.Add(furnitureInfo);
            //        added = true;
            //    }
            //    else
            //    {
            //        Debug.LogError("Furniture already exist.");
            //    }
            //}
            return added;
        }
    }

    [System.Serializable]
    public class PartInfo
    {
        public string Name;
        public string Id;
        public string VisibleName;
        public Sprite AvatarImage;
        public Sprite IconImage;
    }

    [System.Serializable]
    public class AvatarCategoriesInfo
    {
        public string Name;
        public string Id;
        public List<PartInfo> Parts;
    }

    [System.Serializable]
    public class AvatarBaseCategoriesInfo
    {
        public string SetName;
        public string Id;
        public List<AvatarCategoriesInfo> AvatarCategories;
    }
}

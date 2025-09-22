using System.Collections.Generic;
using UnityEngine;

public class AvatarPartsReference : ScriptableObject
{
    [SerializeField] private Sprite _head;
    [SerializeField] private List<AvatarPartCategoryInfo> _info;

    public List<AvatarPartCategoryInfo> AvatarPartData { get => _info;}

    public AvatarPartInfo GetAvatarPartById(string Id)
    {
        try
        {
            var data = _info.Find(category => category.Id == Id.Substring(0,2)).
                AvatarCategories.Find(character => character.Id == Id.Substring(2, 1)).
                Parts.Find(part => part.Id == Id);

            return (data);
        }
        catch
        {
            Debug.Log($"Error: Could not find avatar part with ID: {Id}");
            return (null);
        }
    }

    /// <param name="Id">Insert the two digit character class id.</param>
    public List<AvatarPartInfo> GetAvatarPartsByCategory(string Id)
    {
        AvatarPartCategoryInfo data = _info.Find(category => category.Id == Id);

        if (data == null)
            Debug.LogError($"Could not find avatar parts category with ID: {Id}");

        List<AvatarPartInfo> avatarParts = new List<AvatarPartInfo>();

        foreach (AvatarClassCategoryInfo acci in data.AvatarCategories)
            foreach (AvatarPartInfo part in acci.Parts)
                avatarParts.Add(part);

        return (avatarParts);
    }

    [System.Serializable]
    public class AvatarPartInfo
    {
        public string Name;
        public string Id;
        public string VisibleName;
        public Sprite AvatarImage;
        public Sprite IconImage;
    }

    [System.Serializable]
    public class AvatarClassCategoryInfo
    {
        public string Name;
        public string Id;
        public List<AvatarPartInfo> Parts;
    }

    [System.Serializable]
    public class AvatarPartCategoryInfo
    {
        public string SetName;
        public string Id;
        public List<AvatarClassCategoryInfo> AvatarCategories;
    }
}

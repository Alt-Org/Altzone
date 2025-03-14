using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ALT-Zone/AvatarPartsData")]
public class AvatarPartsReference : ScriptableObject
{
    [SerializeField] private Sprite _head;
    [SerializeField] private List<AvatarPartCategoryInfo> _info;

    public AvatarPartInfo GetAvatarPartById(string Id)
    {
        string[] parts = Id.Split("-");
        try
        {
            var data = _info.Find(category => category.Id == parts[0]).
                AvatarCategories.Find(character => character.Id == parts[1]).
                Parts.Find(part => part.Id == Id);

            return (data);
        }
        catch
        {
            Debug.LogError($"Could not find avatar part with ID: {Id}");
            return (null);
        }
    }

    public AvatarPartCategoryInfo GetAvatarPartsByCategory(string Id)
    {
        string[] parts = Id.Split("-");

        var data = _info.Find(category => category.Id == parts[0]);

        if (data == null)
            Debug.LogError($"Could not find avatar parts category with ID: {Id}");

        return (data);
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

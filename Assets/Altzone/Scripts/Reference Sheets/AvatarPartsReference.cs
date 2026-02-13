using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Altzone.Scripts.AvatarPartsInfo;
[CreateAssetMenu(fileName = "AvatarPartsReference", menuName = "Avatar/Parts Reference")]
public class AvatarPartsReference : ScriptableObject
{
    //[SerializeField] private List<AvatarPartCategoryInfo> _info = new List<AvatarPartCategoryInfo>();
    private static AvatarPartsReference s_instance;
    public static AvatarPartsReference Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = Resources.Load<AvatarPartsReference>("AvatarParts");
            }
            return s_instance;
        }
    }

    public List<AvatarPartCategoryInfo> AvatarPartData => _info;

    private List<AvatarPartCategoryInfo> _info => new List<AvatarPartCategoryInfo>
    {
        Hair,
        Eyes,
        Nose,
        Mouth,
        Body,
        Arms,
        Legs
    };


    [SerializeField] private AvatarPartCategoryInfo _hair = new AvatarPartCategoryInfo();
    public AvatarPartCategoryInfo Hair => _hair;
    [SerializeField] private AvatarPartCategoryInfo _eyes = new AvatarPartCategoryInfo();
    public AvatarPartCategoryInfo Eyes => _eyes;
    [SerializeField] private AvatarPartCategoryInfo _nose = new AvatarPartCategoryInfo();
    public AvatarPartCategoryInfo Nose => _nose;
    [SerializeField] private AvatarPartCategoryInfo _mouth = new AvatarPartCategoryInfo();
    public AvatarPartCategoryInfo Mouth => _mouth;
    [SerializeField] private AvatarPartCategoryInfo _body = new AvatarPartCategoryInfo();
    public AvatarPartCategoryInfo Body => _body;
    [SerializeField] private AvatarPartCategoryInfo _arms = new AvatarPartCategoryInfo();
    public AvatarPartCategoryInfo Arms => _arms;
    [SerializeField] private AvatarPartCategoryInfo _legs = new AvatarPartCategoryInfo();
    public AvatarPartCategoryInfo Legs => _legs;
    public AvatarPartInfo GetAvatarPartById(string id)
    {
        if (!IsValidId(id, minLength: 4))
        {
            Debug.LogWarning($"Invalid avatar part ID format: {id}. Expected format: CategoryId(2) + ClassId(1) + PartId");
            return null;
        }

        try
        {
            string categoryId = id.Substring(0, 2);
            string classId = id.Substring(2, 1);
            
            var category = _info.FirstOrDefault(c => c.Id == categoryId);
            if (category == null)
            {
                Debug.LogWarning($"Avatar part category not found with ID: {categoryId}");
                return null;
            }

            var avatarPart = category.AvatarParts.FirstOrDefault(ac => ac.Id == id);
            if (avatarPart == null)
            {
                Debug.LogWarning($"Avatar class not found with ID: {id} in category: {categoryId}");
                return null;
            }

            return avatarPart;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error retrieving avatar part with ID: {id}. Exception: {e.Message}");
            return null;
        }
    }

    public List<AvatarPartInfo> GetAvatarPartsByCategory(string categoryId)
    {
        if (!IsValidId(categoryId, minLength: 2, maxLength: 2))
        {
            Debug.LogWarning($"Invalid category ID format: {categoryId}. Expected 2 characters.");
            return new List<AvatarPartInfo>();
        }

        var category = _info.FirstOrDefault(c => c.Id == categoryId);
        if (category == null)
        {
            Debug.LogWarning($"Could not find avatar parts category with ID: {categoryId}");
            return new List<AvatarPartInfo>();
        }

        return category.AvatarParts;
    }

    /*public List<AvatarPartInfo> GetAvatarPartsByClass(string categoryId, string classId)
    {
        if (!IsValidId(categoryId, minLength: 2, maxLength: 2) || !IsValidId(classId, minLength: 1, maxLength: 1))
        {
            Debug.LogWarning($"Invalid ID format. CategoryId: {categoryId}, ClassId: {classId}");
            return new List<AvatarPartInfo>();
        }
        
        var category = _info.FirstOrDefault(c => c.Id == categoryId);
        var avatarPart = category?.AvatarParts?.FirstOrDefault(ac => ac.Id == classId);
        
        return avatarPart?.Parts?.ToList() ?? new List<AvatarPartInfo>();
    }*/

    public List<string> GetAllCategoryIds()
    {
        return _info.Where(c => !string.IsNullOrEmpty(c.Id)).Select(c => c.Id).ToList();
    }

    public AvatarPartCategoryInfo GetCategoryById(string categoryId)
    {
        if (string.IsNullOrEmpty(categoryId))
        {
            Debug.LogWarning("Category ID cannot be null or empty");
            return null;
        }

        var category = _info.FirstOrDefault(c => c.Id == categoryId);
        if (category == null)
        {
            Debug.LogWarning($"Avatar part category not found with ID: {categoryId}");
        }

        return category;
    }

    public bool HasData => _info != null && _info.Count > 0;

    public int GetTotalPartsCount()
    {
        return _info?.SelectMany(ac => ac.AvatarParts ?? new List<AvatarPartInfo>())
                   .Count() ?? 0;
    }

    private bool IsValidId(string id, int minLength, int? maxLength = null)
    {
        if (string.IsNullOrEmpty(id) || id.Length < minLength)
            return false;

        if (maxLength.HasValue && id.Length > maxLength.Value)
            return false;

        return true;
    }

    [ContextMenu("Validate Data Structure")]
    public void ValidateDataStructure()
    {
        if (_info == null)
        {
            Debug.LogError("Avatar parts info is null!");
            return;
        }

        int totalParts = 0;
        int issues = 0;

        foreach (var category in _info)
        {
            if (string.IsNullOrEmpty(category.Id))
            {
                Debug.LogWarning($"Category '{category.SetName}' has empty ID");
                issues++;
            }

            if (category.AvatarParts != null)
            {
                foreach (var avatarPart in category.AvatarParts)
                {
                    if (string.IsNullOrEmpty(avatarPart.Id))
                    {
                        Debug.LogWarning($"Avatar class '{avatarPart.Name}' in category '{category.SetName}' has empty ID");
                        issues++;
                    }

                    if (avatarPart.Id != null)
                    {
                        totalParts++;
                    }

                }
            }
        }

        Debug.Log($"Data validation complete. Total parts: {totalParts}, Issues found: {issues}");
    }

    // [System.Serializable]
    // public class AvatarClassCategoryInfo
    // {
    //     [Header("Class Info")]
    //     public string Name = "";
    //     public string Id = "";
    //     
    //     [Header("Parts")]
    //     public List<AvatarPartInfo> Parts = new List<AvatarPartInfo>();
    //     
    //     public int PartsCount => Parts?.Count ?? 0;
    // }

    [System.Serializable]
    public class AvatarPartCategoryInfo
    {
        [Header("Category Info")]
        public string SetName = "";
        public string Id = "";

        [Header("Avatar Parts")]
        public List<AvatarPartInfo> AvatarParts = new List<AvatarPartInfo>();

        public int TotalPartsCount => AvatarParts?.Count ?? 0;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AvatarPartsReference", menuName = "Avatar/Parts Reference")]
public class AvatarPartsReference : ScriptableObject
{
    [SerializeField] private List<AvatarPartCategoryInfo> _info = new List<AvatarPartCategoryInfo>();
    
    public List<AvatarPartCategoryInfo> AvatarPartData => _info;
    
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
            
            var avatarClass = category.AvatarCategories?.FirstOrDefault(ac => ac.Id == classId);
            if (avatarClass == null)
            {
                Debug.LogWarning($"Avatar class not found with ID: {classId} in category: {categoryId}");
                return null;
            }
            
            var part = avatarClass.Parts?.FirstOrDefault(p => p.Id == id);
            if (part == null)
            {
                Debug.LogWarning($"Avatar part not found with ID: {id}");
                return null;
            }
            
            return part;
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
        
        var avatarParts = new List<AvatarPartInfo>();
        
        if (category.AvatarCategories != null)
        {
            foreach (var avatarClass in category.AvatarCategories)
            {
                if (avatarClass?.Parts != null)
                {
                    avatarParts.AddRange(avatarClass.Parts);
                }
            }
        }
        
        return avatarParts;
    }
    
    public List<AvatarPartInfo> GetAvatarPartsByClass(string categoryId, string classId)
    {
        if (!IsValidId(categoryId, minLength: 2, maxLength: 2) || !IsValidId(classId, minLength: 1, maxLength: 1))
        {
            Debug.LogWarning($"Invalid ID format. CategoryId: {categoryId}, ClassId: {classId}");
            return new List<AvatarPartInfo>();
        }
        
        var category = _info.FirstOrDefault(c => c.Id == categoryId);
        var avatarClass = category?.AvatarCategories?.FirstOrDefault(ac => ac.Id == classId);
        
        return avatarClass?.Parts?.ToList() ?? new List<AvatarPartInfo>();
    }
    
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
        return _info?.SelectMany(c => c.AvatarCategories ?? new List<AvatarClassCategoryInfo>())
                   .SelectMany(ac => ac.Parts ?? new List<AvatarPartInfo>())
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
            
            if (category.AvatarCategories != null)
            {
                foreach (var avatarClass in category.AvatarCategories)
                {
                    if (string.IsNullOrEmpty(avatarClass.Id))
                    {
                        Debug.LogWarning($"Avatar class '{avatarClass.Name}' in category '{category.SetName}' has empty ID");
                        issues++;
                    }
                    
                    if (avatarClass.Parts != null)
                    {
                        foreach (var part in avatarClass.Parts)
                        {
                            if (string.IsNullOrEmpty(part.Id))
                            {
                                Debug.LogWarning($"Avatar part '{part.Name}' has empty ID");
                                issues++;
                            }
                            totalParts++;
                        }
                    }
                }
            }
        }
        
        Debug.Log($"Data validation complete. Total parts: {totalParts}, Issues found: {issues}");
    }

    [System.Serializable]
    public class AvatarPartInfo
    {
        [Header("Basic Info")]
        public string Name = "";
        public string Id = "";
        public string VisibleName = "";
        
        [Header("Visual Assets")]
        public Sprite AvatarImage;
        public Sprite IconImage;
        
        public bool IsValid => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Id);
    }

    [System.Serializable]
    public class AvatarClassCategoryInfo
    {
        [Header("Class Info")]
        public string Name = "";
        public string Id = "";
        
        [Header("Parts")]
        public List<AvatarPartInfo> Parts = new List<AvatarPartInfo>();
        
        public int PartsCount => Parts?.Count ?? 0;
    }

    [System.Serializable]
    public class AvatarPartCategoryInfo
    {
        [Header("Category Info")]
        public string SetName = "";
        public string Id = "";
        
        [Header("Avatar Classes")]
        public List<AvatarClassCategoryInfo> AvatarCategories = new List<AvatarClassCategoryInfo>();
        
        public int TotalPartsCount => AvatarCategories?.SelectMany(ac => ac.Parts ?? new List<AvatarPartInfo>()).Count() ?? 0;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

[CreateAssetMenu(fileName = "AvatarDefaults", menuName = "Avatar/Default Reference")]
public class AvatarDefaultReference : ScriptableObject
{
    [SerializeField] private List<AvatarDefault> _avatarDefaults = new List<AvatarDefault>();
    
    [SerializeField, HideInInspector] private List<AvatarDefaultClassCategoryInfo> _info;
    
    private void OnValidate()
    {
        if ((_avatarDefaults == null || _avatarDefaults.Count == 0) && _info != null && _info.Count > 0)
        {
            MigrateOldData();
        }
    }
    
    private void MigrateOldData()
    {
        _avatarDefaults = new List<AvatarDefault>();
        
        foreach (var classCategory in _info)
        {
            foreach (var character in classCategory.Characters)
            {
                if (character.Variations != null && character.Variations.Count > 0)
                {
                    var firstVariation = character.Variations[0];
                    var avatarDefault = new AvatarDefault
                    {
                        CharacterName = character.Name,
                        CharacterId = int.TryParse(classCategory.Id + character.Id, out int id) ? id : 701,
                        HairId = firstVariation.HairId,
                        EyesId = firstVariation.EyesId,
                        NoseId = firstVariation.NoseId,
                        MouthId = firstVariation.MouthId,
                        BodyId = firstVariation.BodyId,
                        HandsId = firstVariation.HandsId,
                        FeetId = firstVariation.FeetId
                    };
                    
                    for (int i = 1; i < character.Variations.Count; i++)
                    {
                        var variation = character.Variations[i];
                        avatarDefault.AlternativeVariations.Add(new AvatarPartVariation
                        {
                            VariationName = variation.Name,
                            HairId = variation.HairId,
                            EyesId = variation.EyesId,
                            NoseId = variation.NoseId,
                            MouthId = variation.MouthId,
                            BodyId = variation.BodyId,
                            HandsId = variation.HandsId,
                            FeetId = variation.FeetId
                        });
                    }
                    
                    _avatarDefaults.Add(avatarDefault);
                }
            }
        }
        
        Debug.Log($"Migrated {_avatarDefaults.Count} avatar defaults from old format");
    }
    
    public List<AvatarDefaultPartInfo> Get(string Id)
    {
        try
        {
            var avatar = GetAvatar(Id);
            if (avatar == null) 
            {
                avatar = _avatarDefaults.FirstOrDefault(a => a.AlternativeVariations.Count > 0) 
                        ?? _avatarDefaults.FirstOrDefault();
                
                if (avatar == null) return null;
            }
            
            var result = new List<AvatarDefaultPartInfo>();
            
            result.Add(new AvatarDefaultPartInfo
            {
                Name = avatar.CharacterName,
                HairId = avatar.HairId,
                EyesId = avatar.EyesId,
                NoseId = avatar.NoseId,
                MouthId = avatar.MouthId,
                BodyId = avatar.BodyId,
                HandsId = avatar.HandsId,
                FeetId = avatar.FeetId
            });
            
            foreach (var altVariation in avatar.AlternativeVariations)
            {
                result.Add(new AvatarDefaultPartInfo
                {
                    Name = altVariation.VariationName,
                    HairId = altVariation.HairId,
                    EyesId = altVariation.EyesId,
                    NoseId = altVariation.NoseId,
                    MouthId = altVariation.MouthId,
                    BodyId = altVariation.BodyId,
                    HandsId = altVariation.HandsId,
                    FeetId = altVariation.FeetId
                });
            }
            
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not find avatar part with ID: {Id}. Error: {e.Message}");
            return null;
        }
    }
    
    public List<AvatarDefaultPartInfo> GetByCharacterId(int Id)
    {
        if (!Enum.IsDefined(typeof(CharacterID), Id)) Id = 701;
        else if (CustomCharacter.IsTestCharacter((CharacterID)Id)) Id = 701;
        return (Get("0" + Id.ToString()[0] + Id.ToString()));
    }
    
    // Overloaded method for string ID
    public AvatarDefault GetAvatar(string stringId)
    {
        try
        {
            if (stringId.Length == 3)
            {
                if (int.TryParse(stringId.Substring(1), out int id))
                    return GetAvatar(id);
            }
            else if (int.TryParse(stringId, out int directId))
            {
                return GetAvatar(directId);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not find avatar part with ID: {stringId}. Error: {e.Message}");
        }
        
        return _avatarDefaults.FirstOrDefault();
    }
    
    // Overloaded method for int ID
    public AvatarDefault GetAvatar(int characterId)
    {
        if (!Enum.IsDefined(typeof(CharacterID), characterId)) 
            characterId = 701;
            
        return _avatarDefaults.FirstOrDefault(a => a.CharacterId == characterId) 
               ?? _avatarDefaults.FirstOrDefault();
    }
    
    [System.Serializable]
    public class AvatarDefaultPartInfo
    {
        public string Name = "Default";
        [Space]
        public string HairId;
        public string EyesId;
        public string NoseId;
        public string MouthId;
        public string BodyId;
        public string HandsId;
        public string FeetId;
    }
    
    [System.Serializable]
    public class AvatarDefaultCharacterInfo
    {
        public string Name;
        public string Id;
        public List<AvatarDefaultPartInfo> Variations;
    }
    
    [System.Serializable]
    public class AvatarDefaultClassCategoryInfo
    {
        public string Name;
        public string Id;
        public List<AvatarDefaultCharacterInfo> Characters;
    }
}

[System.Serializable]
public class AvatarDefault
{
    [Header("Character Info")]
    public string CharacterName = "Default Character";
    public int CharacterId = 701;
    
    [Header("Avatar Parts")]
    public string HairId = "";
    public string EyesId = "";
    public string NoseId = "";
    public string MouthId = "";
    public string BodyId = "";
    public string HandsId = "";
    public string FeetId = "";
    
    [Header("Alternative Parts (Optional)")]
    public List<AvatarPartVariation> AlternativeVariations = new List<AvatarPartVariation>();
}

[System.Serializable]
public class AvatarPartVariation
{
    public string VariationName = "Variation 1";
    [Space]
    public string HairId = "";
    public string EyesId = "";
    public string NoseId = "";
    public string MouthId = "";
    public string BodyId = "";
    public string HandsId = "";
    public string FeetId = "";
}

using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

public class AvatarDefaultReference : ScriptableObject
{
    [SerializeField] private List<AvatarDefaultClassCategoryInfo> _info;

    /// <summary>
    /// Get character parts by string Id.
    /// </summary>
    /// <param name="Id">
    /// First 2 char numbers are the character class id<br/>
    /// and last 3 chars are the character id.<br/>
    /// <br/>
    /// (e.g. "011")
    /// </param>
    public List<AvatarDefaultPartInfo> Get(string Id)
    {
        try
        {
            var characters = _info.Find(characterClass => characterClass.Id == Id.Substring(0, 2)).
                Characters;
            var data = characters.Find(character => character.Id == Id.Substring(2, 3)).Variations;
            if(data.Count > 0) return (data);

            foreach(var character in characters)
            {
                if(character.Variations.Count > 0) return (data);
            }

            return (null);
        }
        catch
        {
            Debug.LogError($"Could not find avatar part with ID: {Id}");
            return (null);
        }
    }

    /// <summary>
    /// Get character parts by int Id.
    /// </summary>
    /// <param name="Id">
    /// First number is the character class<br/>
    /// and last number is the character.
    /// </param>
    public List<AvatarDefaultPartInfo> GetByCharacterId(int Id)
    {
        if(!Enum.IsDefined(typeof(CharacterID), Id)) Id = 701;
        return (Get("0" + Id.ToString()[0] + Id.ToString()));
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

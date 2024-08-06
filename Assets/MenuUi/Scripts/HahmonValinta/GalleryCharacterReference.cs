using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace MenuUi.Scripts.CharacterGallery
{
    //[CreateAssetMenu(menuName = "ALT-Zone/GalleryCharacterReference", fileName = "GalleryCharacterReference")]
    public class GalleryCharacterReference : ScriptableObject
    {
        [SerializeField] private List<CharacterClassInfo> _info;


        public CharacterInfo GetCharacterPrefabInfo(int prefabId)
        {
            CharacterClassID characterClass = CustomCharacter.GetClassID((CharacterID)prefabId);

            int classValue = (int)characterClass >> 8;
            classValue--;

            if (classValue < 0 || classValue >= _info.Count)
            {
                return null;
            }
            CharacterClassInfo classObject = _info[classValue];

            int characterValue = CustomCharacter.GetInsideCharacterID((CharacterID)prefabId);
            characterValue--;

            if (characterValue < 0 || characterValue >= classObject.list.Count)
            {
                return null;
            }
            CharacterInfo character = classObject.list[characterValue];
            return character;
        }
    }

    [Serializable]
    public class CharacterInfo
    {
        public string Name;
        public CharacterID id;
        public Sprite Image;
    }

    [Serializable]
    public class CharacterClassInfo
    {
        public string Name;
        public CharacterClassID id;
        public List<CharacterInfo> list;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace MenuUi.Scripts.CharacterGallery
{
    public class GalleryCharacterReference : MonoBehaviour
    {
        [SerializeField] private List<CharacterClassInfo> _info;


        public CharacterInfo GetCharacterPrefabInfo(int prefabId)
        {
            CharacterClassID characterClass = CustomCharacter.GetClassID((CharacterID)prefabId);

            int classValue = (int)characterClass >> 8;
            classValue--;

            int characterValue = CustomCharacter.GetInsideCharacterID((CharacterID)prefabId);
            characterValue--;

            if (classValue < 0 || classValue >= _info.Count)
            {
                return null;
            }
            CharacterClassInfo classObject = _info[classValue];
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
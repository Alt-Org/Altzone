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
        [SerializeField] private List<GalleryCharacterClassInfo> _info;


        public GalleryCharacterInfo GetCharacterPrefabInfoFast(int prefabId)
        {
            CharacterClassType characterClass = CustomCharacter.GetClass((CharacterID)prefabId);

            int classValue = (int)characterClass >> 8;
            classValue--;

            if (classValue < 0 || classValue >= _info.Count)
            {
                return null;
            }
            GalleryCharacterClassInfo classObject = _info[classValue];

            int characterValue = CustomCharacter.GetInsideCharacterID((CharacterID)prefabId);
            characterValue--;

            if (characterValue < 0 || characterValue >= classObject.list.Count)
            {
                return null;
            }
            GalleryCharacterInfo character = classObject.list[characterValue];

            return character;
        }

        public GalleryCharacterInfo GetCharacterPrefabInfoSafe(int prefabId)
        {
            CharacterClassType characterClass = CustomCharacter.GetClass((CharacterID)prefabId);

            GalleryCharacterClassInfo classObject = null;
            foreach (GalleryCharacterClassInfo classInfo in _info)
            {
                if (classInfo.Type == characterClass)
                {
                    classObject = classInfo;
                    break;
                }
            }

            if (classObject == null)
            {
                return null;
            }

            GalleryCharacterInfo character = null;
            foreach (GalleryCharacterInfo CharacterInfo in classObject.list)
            {
                if (CharacterInfo.id == (CharacterID)prefabId)
                {
                    character = CharacterInfo;
                    break;
                }
            }
            return character;
        }
    }

    [Serializable]
    public class GalleryCharacterInfo
    {
        public string Name;
        public CharacterID id;
        public Sprite Image;
    }

    [Serializable]
    public class GalleryCharacterClassInfo
    {
        public string Name;
        public CharacterClassType Type;
        public List<GalleryCharacterInfo> list;
    }
}

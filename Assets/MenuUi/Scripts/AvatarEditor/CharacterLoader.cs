using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
    public class CharacterLoader : MonoBehaviour
    {
        [SerializeField] private List<AvatarClassInfo> _avatarClassInfoList;
        // [SerializeField] private Image _characterImage;
        [SerializeField] private Transform _characterImageParent;
        [SerializeField] private Image _divanImage;
        [SerializeField]private GameObject _defaultAvatarImagePrefab;
        [SerializeField]private GameObject _confluenceAvatarImagePrefab;
        PlayerData _playerData = null;
        private CharacterClassType _characterClassType;
        private CharacterClassType _previousType = CharacterClassType.None;

        public void RefreshPlayerCurrentCharacter()
        {
            Storefront.Get().GetPlayerData(ServerManager.Instance.Player.uniqueIdentifier, p => _playerData = p);
            int selectedCharacterId = _playerData.SelectedCharacterId;
            UpdateDivanImage(selectedCharacterId);
        }

        private AvatarInfo GetCharacterPrefabInfo_bkp(int prefabId)
        {
            CharacterClassType characterClass = CustomCharacter.GetClass((CharacterID)prefabId);
            _previousType = _characterClassType;
            _characterClassType = characterClass;

            AvatarClassInfo classObject = null;
            foreach (AvatarClassInfo classInfo in _avatarClassInfoList)
            {
                Debug.Log("Script GetCharacterPrefabInfo: going over classInfo.Type: " + classInfo.Type);
                if (classInfo.Type == characterClass)
                {
                    classObject = classInfo;
                    break;
                }
            }

            if (classObject == null)
            {
                classObject = _avatarClassInfoList[0];
                _characterClassType = classObject.Type;
                Debug.LogError($"Could not select AvatarClassInfo! Current character class id is: {characterClass}. Using first AvatarClassInfo: {_avatarClassInfoList[0].Type}.");
            }

            AvatarInfo character = null;
            foreach (AvatarInfo CharacterInfo in classObject.list)
            {
                if (CharacterInfo.id == (CharacterID)prefabId)
                {
                    character = CharacterInfo;
                    break;
                }
            }

            if (character == null)
            {
                character = classObject.list[0];
                Debug.LogError($"Could not select AvatarInfo! Current CharacterId is: {(CharacterID)prefabId}. Using first AvatarInfo: {classObject.list[0].id}");
            }

            return character;
        }

        private void UpdateDivanImage(int prefabId)
        {
            AvatarInfo character = GetCharacterPrefabInfo_bkp(prefabId);

            if (character != null)
                _divanImage.color = character.DivanImage;

            if(_previousType == CharacterClassType.None)
                ResetAvatarDataToDefaults();
        }

        public CharacterClassType GetCharacterClass()
        {
            return _characterClassType;
        }

        private void ResetAvatarDataToDefaults()
        {
            for(int i = 0; i < 10; i++)
            {
                PlayerPrefs.SetInt(((FeatureSlot)i).ToString()+"Feature", 0);
            }
            for(int i = 0; i < 10; i++)
            {
                PlayerPrefs.SetInt(((FeatureSlot)i).ToString()+"Color", 0);
            }
        }
    }

    [Serializable]
    public class AvatarInfo
    {
        public string Name;
        public CharacterID id;
        public Color DivanImage;
    }

    [Serializable]
    public class AvatarClassInfo
    {
        public string Name;
        public CharacterClassType Type;
        public List<AvatarInfo> list;
    }
}

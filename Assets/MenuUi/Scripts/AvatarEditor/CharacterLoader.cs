using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
        private CharacterClassID _characterClassID;
        private CharacterClassID _previousID = CharacterClassID.None;

        public void RefreshPlayerCurrentCharacter(Action onComplete)
        {
            Storefront.Get().GetPlayerData(ServerManager.Instance.Player.uniqueIdentifier, p => _playerData = p);
            int selectedCharacterId = _playerData.SelectedCharacterId;
            StartCoroutine(UpdateCharacterImage(selectedCharacterId, onComplete));
        }

        private AvatarInfo GetCharacterPrefabInfo_bkp(int prefabId)
        {
            CharacterClassID characterClass = CustomCharacter.GetClassID((CharacterID)prefabId);
            _previousID = _characterClassID;
            _characterClassID = characterClass;

            AvatarClassInfo classObject = null;
            foreach (AvatarClassInfo classInfo in _avatarClassInfoList)
            {
                if (classInfo.id == characterClass)
                {
                    classObject = classInfo;
                    break;
                }
            }

            if (classObject == null)
            {
                classObject = _avatarClassInfoList[0];
                _characterClassID = classObject.id;
                Debug.LogError($"Could not select AvatarClassInfo! Current character class id is: {characterClass}. Using first AvatarClassInfo: {_avatarClassInfoList[0].id}.");
                //return null;
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

        private IEnumerator UpdateCharacterImage(int prefabId, Action onComplete)
        {
            AvatarInfo character = GetCharacterPrefabInfo_bkp(prefabId);
            GameObject avatarImage = null;
            if (character != null)
            {
                foreach(Transform child in _characterImageParent){
                    Destroy(child.gameObject);
                }
                if(_characterClassID == CharacterClassID.Confluent){
                    avatarImage = Instantiate(_confluenceAvatarImagePrefab, _characterImageParent);
                }
                else{
                    avatarImage = Instantiate(_defaultAvatarImagePrefab, _characterImageParent);
                }
                
                _divanImage.color = character.DivanImage;
            }
            if(_previousID == CharacterClassID.None)
            {
                ResetAvatarDataToDefaults();
            }


            //This is to prevent a race condition in AvatarEditorController and FeaturePicker
            yield return new WaitWhile(() => avatarImage == null);
            onComplete?.Invoke();
        }

        public CharacterClassID GetCharacterClassID()
        {
            return _characterClassID;
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
        public CharacterClassID id;
        public List<AvatarInfo> list;
    }
}

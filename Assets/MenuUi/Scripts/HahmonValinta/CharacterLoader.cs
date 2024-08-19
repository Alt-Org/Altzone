using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using MenuUi.Scripts.CharacterGallery;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.CompilerServices;


public class CharacterLoader : MonoBehaviour
{
    [SerializeField] private List<GalleryCharacterClassInfo> _info;
    [SerializeField] private Image _characterImage;  

    private void OnEnable()
    {
        RefreshPlayerCurrentCharacter();
    }

    public void RefreshPlayerCurrentCharacter()
    {
        PlayerData _playerData = null;
        Storefront.Get().GetPlayerData(ServerManager.Instance.Player.uniqueIdentifier, p => _playerData = p);

        int selectedCharacterId = _playerData.SelectedCharacterId;
        UpdateCharacterImage(selectedCharacterId);  
    }

    private GalleryCharacterInfo GetCharacterPrefabInfo(int prefabId)
    {
        CharacterClassID characterClass = CustomCharacter.GetClassID((CharacterID)prefabId);

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

    private void UpdateCharacterImage(int prefabId)
    {
        GalleryCharacterInfo character = GetCharacterPrefabInfo(prefabId);
        if (character != null && _characterImage != null)
        {
            _characterImage.sprite = character.Image;
        }
    }
}

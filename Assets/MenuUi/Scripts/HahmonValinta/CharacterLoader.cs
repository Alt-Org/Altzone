using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.CompilerServices;


public class CharacterLoader : MonoBehaviour
{
    [SerializeField] private List<AvatarClassInfo> _avatarClassInfoList;
    private Image _characterImage;
    private Image _divanImage;
    private string _divanObjectName = "divaani";
    PlayerData _playerData = null;


    private void OnEnable()
    {
        RefreshPlayerCurrentCharacter();
    }

    public void RefreshPlayerCurrentCharacter()
    {
        _characterImage = GetComponent<Image>();
        _divanImage = GameObject.Find(_divanObjectName).GetComponent<Image>();
        Storefront.Get().GetPlayerData(ServerManager.Instance.Player.uniqueIdentifier, p => _playerData = p);

        int selectedCharacterId = _playerData.SelectedCharacterId;
        UpdateCharacterImage(selectedCharacterId);
    }

    private AvatarInfo GetCharacterPrefabInfo_bkp(int prefabId)
    {
        CharacterClassID characterClass = CustomCharacter.GetClassID((CharacterID)prefabId);

        int classValue = (int)characterClass >> 8;
        classValue--;

        if (classValue < 0 || classValue >= _avatarClassInfoList.Count)
        {
            return null;
        }
        AvatarClassInfo classObject = _avatarClassInfoList[classValue];

        int characterValue = CustomCharacter.GetInsideCharacterID((CharacterID)prefabId);
        characterValue--;

        if (characterValue < 0 || characterValue >= classObject.list.Count)
        {
            return null;
        }
        AvatarInfo character = classObject.list[characterValue];
        return character;
    }

    private void UpdateCharacterImage(int prefabId)
    {
        AvatarInfo character = GetCharacterPrefabInfo_bkp(prefabId);
        if (character != null && _characterImage != null)
        {
            _characterImage.sprite = character.CharacterImage;
            _divanImage.sprite = character.DivanImage;
        }
    }
}

    [Serializable]
public class AvatarInfo
{
    public string Name;
    public CharacterID id;
    public Sprite CharacterImage;
    public Sprite DivanImage;
}


[Serializable]
public class AvatarClassInfo
{
    public string Name;
    public CharacterClassID id;
    public List<AvatarInfo> list;
}


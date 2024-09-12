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
    // [SerializeField] private Image _characterImage;
    [SerializeField] private Transform _characterImageParent;
    [SerializeField] private Image _divanImage;
    private string _divanObjectName = "divaani";
    PlayerData _playerData = null;


    private void OnEnable()
    {
        RefreshPlayerCurrentCharacter();
    }

    public void RefreshPlayerCurrentCharacter()
    {
        // _characterImage = GetComponent<Image>();
        _divanImage = GameObject.Find(_divanObjectName).GetComponent<Image>();
        Storefront.Get().GetPlayerData(ServerManager.Instance.Player.uniqueIdentifier, p => _playerData = p);

        int selectedCharacterId = _playerData.SelectedCharacterIds[0];
        UpdateCharacterImage(selectedCharacterId);
    }

    private AvatarInfo GetCharacterPrefabInfo_bkp(int prefabId)
    {
        CharacterClassID characterClass = CustomCharacter.GetClassID((CharacterID)prefabId);

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
            return null;
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
        return character;
    }

    private void UpdateCharacterImage(int prefabId)
    {
        
        AvatarInfo character = GetCharacterPrefabInfo_bkp(prefabId);
        // Debug.Log("prefab id on: " + prefabId + " it corresponds to character: " +character.Name);
        if (character != null && character.characterImagePrefab != null)
        {
            // Debug.Log("Instantinating character");
            foreach(Transform child in _characterImageParent){
                Destroy(child.gameObject);
            }
            Instantiate(character.characterImagePrefab, _characterImageParent);
            _divanImage.sprite = character.DivanImage;
        }
    }
}

    [Serializable]
public class AvatarInfo
{
    public string Name;
    public CharacterID id;
    // public Sprite CharacterImage;
    public GameObject characterImagePrefab;
    public Sprite DivanImage;
}


[Serializable]
public class AvatarClassInfo
{
    public string Name;
    public CharacterClassID id;
    public List<AvatarInfo> list;
}


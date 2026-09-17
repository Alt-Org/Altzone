using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroCharacters : MonoBehaviour
{
    [SerializeField]
    private List<CharacterThumbnailHandler> characterCards;

    private List<int> characterIDs = new List<int>();

    public delegate void InitialAvatarSelection(CharacterClassType classType);
    public static event InitialAvatarSelection OnInitialAvatarSelection;

    void Start()
    {

        foreach (int i in Enum.GetValues(typeof(CharacterClassType))) //add character class type IDs to list
        {
            characterIDs.Add(i);
        }

        int j = 0;                     //for going through the classtype list
        AvatarReference avatarreference= AvatarReference.Instance;
        foreach (CharacterThumbnailHandler characterCard in characterCards)
        {
            j++;

            CharacterClassType classType = (CharacterClassType)characterIDs[j]; //get classtype from list

            characterCard.SetData(classType, SelectInitialAvatar);
        }
    }

    private void SelectInitialAvatar(CharacterClassType classType)
    {
        OnInitialAvatarSelection?.Invoke(classType);
    }

}

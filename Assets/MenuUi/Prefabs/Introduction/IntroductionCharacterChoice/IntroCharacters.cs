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
    private List<GameObject> characterCards;

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
        foreach (GameObject characterCard in characterCards)
        {
            j++;

            CharacterClassType classType = (CharacterClassType)characterIDs[j]; //get classtype from list

            characterCard.GetComponent<Image>().color = avatarreference.GetColour(classType); //change card color

            CharacterThumbnailHandler characterThumbnailHandler = characterCard.GetComponent<CharacterThumbnailHandler>(); // get correct thumbnailhandler 

            Image characterSprite = characterThumbnailHandler._characterSprite;
            characterSprite.sprite = avatarreference.GetCharacterSprite(classType); //set character sprite

            AvatarData data = new(avatarreference.GetDefaultAvatar(classType));
            AvatarVisualData visualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(data);
            characterThumbnailHandler._avatarLoader.UpdateVisuals(visualData); //set avatar design


            TextMeshProUGUI nameSprite = characterThumbnailHandler._nameText; //find name-child
            nameSprite.text = avatarreference.GetName(classType); //set name sprite
            nameSprite.color = avatarreference.GetColour(classType);

            characterThumbnailHandler._selectButton.onClick.AddListener(() => { OnInitialAvatarSelection?.Invoke(classType); });
        }
    }


}

using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroCharacters : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> characterCards;

    private List<int> characterIDs = new List<int>();


    void Start()
    {

        foreach (int i in Enum.GetValues(typeof(CharacterClassType))) //add character class type IDs to list
        {
            characterIDs.Add(i);
        }


        CharacterClassType classType;
        int j = 0;                     //for going through the classtype list
        AvatarReference avatarreference= AvatarReference.Instance;
        foreach (GameObject characterCard in characterCards)
        {
            j++;

            classType = (CharacterClassType)characterIDs[j]; //get classtype from list

            characterCard.GetComponent<Image>().color = avatarreference.GetColour(classType); //change card color


            CharacterThumbnailHandler characterThumbnailHandler = characterCard.GetComponent<CharacterThumbnailHandler>(); // get correct thumbnailhandler 

            Image characterSprite = characterThumbnailHandler._characterSprite;
            characterSprite.sprite = avatarreference.GetCharacterSprite(classType); //set character sprite


            TextMeshProUGUI nameSprite = characterThumbnailHandler._nameText; //find name-child
            nameSprite.text = avatarreference.GetName(classType); //set name sprite
            nameSprite.color = avatarreference.GetColour(classType);

           
        }
    }


}

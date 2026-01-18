using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;
using UnityEngine.UI;

public class IntroCharacters : MonoBehaviour
{
    [SerializeField]
    private ClassReference _classReference;

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

        foreach (GameObject characterCard in characterCards)
        {
            j++;

            classType = (CharacterClassType)characterIDs[j]; //get classtype from list

            characterCard.GetComponent<Image>().color = _classReference.GetAlternativeColor(classType); //change card color


            CharacterThumbnailHandler characterThumbnailHandler = characterCard.GetComponent<CharacterThumbnailHandler>(); // get correct thumbnailhandler 

            Image characterSprite = characterThumbnailHandler._characterSprite;
            characterSprite.sprite = _classReference.GetCharacter(classType); //set character sprite


            Image emblemSprite = characterThumbnailHandler._emblemSprite; //find emblem-child
            emblemSprite.sprite = _classReference.GetCornerIcon(classType); //set emblem sprite

            Image nameSprite = characterThumbnailHandler._nameSprite; //find name-child
            nameSprite.sprite = _classReference.GetNameIcon(classType); //set name sprite

           
        }
    }


}

using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;
using UnityEngine.UI;

public class IntroCharacters : MonoBehaviour
{
    [SerializeField] private ClassReference _classReference;

    [SerializeField]
    private List<GameObject> characterCards;

    void Start()
    {
        CharacterClassType classType = (int) CharacterClassType.None;


        foreach (GameObject characterCard in characterCards)
        {
            classType += 100; //move to next class type


            characterCard.GetComponent<Image>().color = _classReference.GetAlternativeColor(classType); //change card color

            Image characterSprite = characterCard.transform.Find("Character").GetComponent<Image>(); //find character-child
            characterSprite.sprite = _classReference.GetCharacter(classType); //set character sprite

            Image emblemSprite = characterCard.transform.Find("CornerIcon").GetComponent<Image>(); //find emblem-child
            emblemSprite.sprite = _classReference.GetCornerIcon(classType); //set emblem sprite

            Image nameSprite = characterCard.transform.Find("CharacterTypeName").GetComponent<Image>(); //find name-child
            nameSprite.sprite = _classReference.GetNameIcon(classType); //set name sprite


        }
    }


}

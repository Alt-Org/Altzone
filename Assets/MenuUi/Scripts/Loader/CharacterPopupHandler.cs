using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;

public class CharacterPopupHandler : MonoBehaviour
{
    [SerializeField]
    List<CharacterListElement> popupOptions = new List<CharacterListElement>();
    [SerializeField]
    Sprite backupImage;
    [SerializeField]
    TextMeshProUGUI classChoiseText;
    [SerializeField]
    Image charaterImage;

    [SerializeField]
    TextMeshProUGUI classIntroductionText;

    [SerializeField]
    Image className;

    [SerializeField]
    private ClassReference _classReference;

    //function that changes the name and image

    public void UpdateImageAndText(int id)
    {

        CharacterClassType classType = (int)CharacterClassType.None;
        classType = (CharacterClassType)(id * 100);                             // gets button id and connects it to correct class


        if (id <= popupOptions.Count)
        {
            //string cname = popupOptions[id].className;
            /*
            if (popupOptions[id].characterImage == null)
            {
                charaterImage.sprite = backupImage; //character image switching
            }
            else
            {
                charaterImage.sprite = popupOptions[id].characterImage;
            }
            //classChoiseText.text = $"Oletko varma että haluat edustaa {cname} suojelijaluokkaa pelaajien keskuudessa?"; //character name switching
            */

            charaterImage.sprite = _classReference.GetCharacter(classType); //show correct character sprite

            classIntroductionText.text = _classReference.GetDescription(classType); //show correct description

            className.sprite = _classReference.GetNameIcon(classType); // show correct name sprite
        }
        else //backup image and text just in case an error happens or something
        {
            charaterImage.sprite = backupImage;
            //classChoiseText.text = "Oletko varma että haluat edustaa ERROR suojelijaluokkaa pelaajien keskuudessa?";
        }
    }
}

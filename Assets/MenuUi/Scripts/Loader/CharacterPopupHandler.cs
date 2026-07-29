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
    TextMeshProUGUI classNameText;

    //function that changes the name and image

    public void UpdateImageAndText(int id)
    {

        CharacterClassType classType = (int)CharacterClassType.None;
        classType = (CharacterClassType)(id * 100);                             // gets button id and connects it to correct class
        AvatarReference reference = AvatarReference.Instance;

        if (classType is not CharacterClassType.None)
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

            charaterImage.sprite = reference.GetCharacterSprite(classType); //show correct character sprite

            classIntroductionText.text = reference.GetDescription(classType); //show correct description

            //className.sprite = reference.GetNameIcon(classType); // show correct name sprite
            classNameText.text = reference.GetName(classType);
            classNameText.color = reference.GetColour(classType);
        }
        else //backup image and text just in case an error happens or something
        {
            charaterImage.sprite = backupImage;
            //classChoiseText.text = "Oletko varma että haluat edustaa ERROR suojelijaluokkaa pelaajien keskuudessa?";
        }
    }
}

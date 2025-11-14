using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    //function that changes the name and image

    public void UpdateImageAndText(int id)
    {
        if (id <= popupOptions.Count)
        {
            //string cname = popupOptions[id].className;

            if (popupOptions[id].characterImage == null)
            {
                charaterImage.sprite = backupImage; //character image switching
            }
            else
            {
                charaterImage.sprite = popupOptions[id].characterImage;
            }
            //classChoiseText.text = $"Oletko varma että haluat edustaa {cname} suojelijaluokkaa pelaajien keskuudessa?"; //character name switching
        }
        else //backup image and text just in case an error happens or something
        {
            charaterImage.sprite = backupImage;
            //classChoiseText.text = "Oletko varma että haluat edustaa ERROR suojelijaluokkaa pelaajien keskuudessa?";
        }
    }
}

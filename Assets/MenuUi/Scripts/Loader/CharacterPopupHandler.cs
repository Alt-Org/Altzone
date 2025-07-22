using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterPopupHandler : MonoBehaviour
{

    //this crap is harder than usual cause it doesn't sugest these, i dunno why, maybe it's broken?
    //finally got it to work, these are the serialized fields for variables
    [SerializeField]
    List<CharacterListElement> popupOptions = new List<CharacterListElement>();
    [SerializeField]
    Sprite backupImage;
    [SerializeField]
    TextMeshProUGUI classChoiseText;
    [SerializeField]
    Image charaterImage;
    
    // Start is called before the first frame update
    //void Start()
    //{
        
    //}

    // Update is called once per frame
    //void Update()
    //{
        
    //}

    //function that changes the name and image

    public void UpdateImageAndText(int id)
    {
        if (id <= popupOptions.Count)
        {
            string cname = popupOptions[id].className;

            if (popupOptions[id].characterImage == null)
            {
                charaterImage.sprite = backupImage; //character image switching
            }
            else
            {
                charaterImage.sprite = popupOptions[id].characterImage;
            }
            classChoiseText.text = "Olet valitsemassa " + cname + " hahmoluokan edustajan edustmaan itseäsi"; //character name switching
        }
        else //backup image and text just in case an error happens or something
        {
            charaterImage.sprite = backupImage;
            classChoiseText.text = "Olet valitsemassa ERROR hahmoluokan edustajan edustmaan itseäsi";
        }
    }
}

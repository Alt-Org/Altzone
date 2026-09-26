using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPopupHandler : MonoBehaviour
{
    [SerializeField]
    Sprite backupImage;
    [SerializeField]
    TextMeshProUGUI classChoiseText;
    [SerializeField]
    Image charaterImage;
    [SerializeField]
    AvatarLoader _charaterAvatar;

    [SerializeField]
    TextMeshProUGUI classIntroductionText;

    [SerializeField]
    Image className;
    [SerializeField]
    TextMeshProUGUI classNameText;

    //function that changes the name and image

    public void UpdateImageAndText(CharacterClassType classType, AvatarData avatar)
    {                             
        AvatarReference reference = AvatarReference.Instance;

        if (classType is not CharacterClassType.None)
        {
            charaterImage.sprite = reference.GetCharacterSprite(classType); //show correct character sprite
            AvatarVisualData avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(avatar);
            _charaterAvatar.UpdateVisuals(avatarVisualData);

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

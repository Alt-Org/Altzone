using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.UI;

public class ToCharacterCreationButton : MonoBehaviour
{

    private void Start()
    {
        // Attach a click event handler to the button
        Button button = GetComponent<Button>();
        button.onClick.AddListener(()=>SelectInitialAvatar(CharacterClassType.Desensitizer));
    }

    private void SelectInitialAvatar(CharacterClassType classType)
    {
        IntroCharacters.SelectInitialAvatar(classType);
    }
}

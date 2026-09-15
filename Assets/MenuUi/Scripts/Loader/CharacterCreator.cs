using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.AvatarEditor;
using UnityEngine;

public class CharacterCreator : MonoBehaviour
{
    [SerializeField] private List<IntroPresetHandler> _presetButtons;
    [SerializeField] private AvatarEditorController _avatarEditor;

    public void SetCharacter(CharacterClassType classType)
    {
        _avatarEditor.SetPresetAvatar(classType);
    }
}

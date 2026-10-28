using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.AvatarEditor;
using UnityEngine;

public class CharacterCreator : MonoBehaviour
{
    [SerializeField] private List<IntroPresetHandler> _presetButtons;
    [SerializeField] private AvatarEditorController _avatarEditor;
    private bool _initialized = false;

    void Start()
    {
        if (!_initialized) Initialize();
    }

    private void Initialize()
    {
        List<int> characterIDs = new();

        foreach (int i in Enum.GetValues(typeof(CharacterClassType))) //add character class type IDs to list
        {
            characterIDs.Add(i);
        }

        int j = 0;                     //for going through the classtype list
        foreach (IntroPresetHandler presetSlot in _presetButtons)
        {
            j++;
            if (characterIDs.Count <= j) break;
            CharacterClassType classType = (CharacterClassType)characterIDs[j]; //get classtype from list

            presetSlot.SetPresetData(classType, SetCharacter);

        }
        _initialized = true;
    }

    public void SetInitialSelectCharacter(CharacterClassType classType, Action<CharacterClassType, AvatarData> saveOverride)
    {
        if (!_initialized) Initialize();
        _presetButtons.FirstOrDefault(x => x.ClassType == classType)?.Toggle(true);
        _avatarEditor.SetPresetAvatar(classType);
        _avatarEditor.SetSaveOverride(saveOverride);
    }

    public void SetCharacter(CharacterClassType classType)
    {
        _avatarEditor.SetPresetAvatar(classType);
    }
}

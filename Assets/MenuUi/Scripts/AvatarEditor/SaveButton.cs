using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SaveButtonScript : MonoBehaviour
{
    public Button saveButton; 
    public TMP_InputField nameInput;

    private PlayerAvatar _playerAvatar;


    private void OnEnable()
    {
        _playerAvatar = new PlayerAvatar(PlayerPrefs.GetString("CharacterName"));
        nameInput.text = _playerAvatar.Name;
        saveButton.onClick.AddListener(SaveName); 
    }


    private void SaveName()
    {
        string characterName = nameInput.text;
        Debug.Log("Character name saved: " + characterName);

        _playerAvatar.Name = characterName;
        PlayerPrefs.SetString("CharacterName", characterName);
        PlayerPrefs.Save();
    }
}

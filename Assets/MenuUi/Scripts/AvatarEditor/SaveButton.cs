using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveButtonScript : MonoBehaviour
{
    public Button saveButton; 
    public TMP_InputField nameInput; 

    void Start()
    {
        saveButton.onClick.AddListener(SaveName); 
    }

    private void OnEnable()
    {
        nameInput.text = PlayerPrefs.GetString("CharacterName");
    }
    private void SaveName()
    {
        string characterName = nameInput.text;
        Debug.Log("Character name saved: " + characterName);

        PlayerPrefs.SetString("CharacterName", characterName);
        PlayerPrefs.Save();
    }
}

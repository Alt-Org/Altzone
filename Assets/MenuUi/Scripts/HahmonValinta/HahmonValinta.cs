using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class CharacterData
{
    public int uniqueID;
    public string characterName;

    public CharacterData(int id, string name)
    {
        uniqueID = id;
        characterName = name;
    }
}

public class HahmonValinta : MonoBehaviour
{
    [SerializeField] private Button[] characterButtons;
    [SerializeField] private Button lockInButton;
    [SerializeField] private CharacterData[] characterData;
    [SerializeField] private GameObject popupWindow; // Reference to the pop-up window panel

    private int selectedCharacterIndex = -1;

    void Start()
    {
        // Initialize the pop-up window as inactive
        popupWindow.SetActive(false);

        // Assign onClick events for character buttons
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int characterIndex = i;
            characterButtons[i].onClick.AddListener(() => CharacterSelected(characterIndex));
        }

        // Assign onClick event for lock-in button
        lockInButton.onClick.AddListener(LockInCharacter);
    }

    void CharacterSelected(int characterIndex)
    {
        // Update the selected character index
        selectedCharacterIndex = characterIndex;

        // Activate the pop-up window
        popupWindow.SetActive(true);

        // Log the selected character's name
        Debug.Log("Selected character: " + characterData[characterIndex].characterName);
    }

    public void LockInCharacter()
    {
        // Check if a character is selected
        if (selectedCharacterIndex != -1)
        {
            // Log the selected character's information
            Debug.Log("Locked in character: " + characterData[selectedCharacterIndex].characterName);

            // Reset the selected character index and disable the lock-in button
            selectedCharacterIndex = -1;
            lockInButton.interactable = false;

            // Deactivate the pop-up window
            popupWindow.SetActive(false);
        }
        else
        {
            // No character selected, log a message or handle the case as needed
            Debug.Log("No character selected.");
        }
    }
}

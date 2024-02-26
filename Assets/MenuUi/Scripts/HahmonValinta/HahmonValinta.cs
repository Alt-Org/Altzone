using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


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

    private bool characterChosen = false;
    private int selectedCharacterIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        // Disable the lock-in button initially
        lockInButton.interactable = false;

        // Assign onClick events for character buttons
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int characterIndex = i;
            characterButtons[i].onClick.AddListener(() => CharacterSelected(characterIndex));
        }

        // Assign onClick event for lock-in button
        lockInButton.onClick.AddListener(LockInCharacter);
    }

    // Method to handle character selection
    void CharacterSelected(int characterIndex)
    {
        // Update the selected character index
        selectedCharacterIndex = characterIndex;

        // Enable the lock-in button
        lockInButton.interactable = true;

        // Log the character name associated with the button clicked
        Debug.Log("Selected character: " + characterData[characterIndex].characterName);
    }

    // Method to handle locking in the character
    void LockInCharacter()
    {
        // Check if a character is selected
        if (selectedCharacterIndex != -1)
        {
            // Log the selected character's information
            Debug.Log("Locked in character: " + characterData[selectedCharacterIndex].characterName);

            // Reset the selected character index and disable the lock-in button
            selectedCharacterIndex = -1;
            lockInButton.interactable = false;
        }
        else
        {
            // No character selected, log a message or handle the case as needed
            Debug.Log("No character selected.");
        }
    }
}